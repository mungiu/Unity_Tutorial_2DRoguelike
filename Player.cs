using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MovingObject {

    public Text foodText;
    public int wallDamage = 1;
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1f;

    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;

    private Animator animator;
    private int food;
    //touch controls 
    //setting the touch off screen makes it return "false" since technically no touch on screen happened
    private Vector2 touchOrigin = -Vector2.one;

    /// <summary>
    /// Implementation will differe across classes.
    /// </summary>
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        food = GameManager.instance.playerFoodPoints;
        foodText.text = $"Food: {food}";

        base.Start();
    }

    private void OnDisable()
    {
        //saving food colelction progress
        GameManager.instance.playerFoodPoints = food;
    }

    // Update is called once per frame
    void Update()
    {
        //if it's not players turn do nothing
        if (!GameManager.instance.playersTurn)
            return;

        int horizontal = 0;
        int vertical = 0;

        ///applying controlls based on build platform
        #if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER)
        {
            horizontal = (int)(Input.GetAxisRaw("Horizontal"));
            vertical = (int)(Input.GetAxisRaw("Vertical"));

            if (horizontal != 0)
                vertical = 0;
        }
        #else
        //Player mobile touch controlls
        if (Input.touchCount > 0)
        {
            //registering and tracking first touch only
            Touch myTouch = Input.touches[0];
            //saving the beginning of a touch as origin
            if (myTouch.phase == TouchPhase.Began)
                touchOrigin = myTouch.position;
            else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
            {
                Vector2 touchEnd = myTouch.position;
                //tracking finger movement distance on screen
                float x = touchEnd.x - touchOrigin.x;
                float y = touchEnd.y - touchOrigin.y;
                //reseting touch origin to "false"
                touchOrigin.x = -1;

                //generalizing the user touch move as more vertical or horizontal
                if (Mathf.Abs(x) > Mathf.Abs(y))
                    //checking left or right movement
                    horizontal = x > 0 ? 1 : -1;
                else
                    //checking up or down movement
                    vertical = x > 0 ? 1 : -1;
            }
        }
        #endif

        if (horizontal !=0 || vertical !=0)
            AttemptMove<Wall>(horizontal, vertical);

        //storing movement direction
        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        //preventing player from moving 2 directions in one move
        if (horizontal != 0)
            vertical = 0;

        if (horizontal != 0 || vertical != 0)
            //meaning expecting player may encounter wall to interract with
            AttemptMove<Wall>(horizontal, vertical);
    }

    /// <summary>
    /// Attempts to move the player to indicated position while xpecting to encounter a wall <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="xDir"></param>
    /// <param name="yDir"></param>
    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        food--;
        foodText.text = $"Food: {food}";

        base.AttemptMove<T>(xDir, yDir);

        RaycastHit2D hit;
        //moving sound
        if (Move(xDir, yDir, out hit))
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);

        CheckIfGameOver();
        GameManager.instance.playersTurn = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Exit")
        {
            //invoking restart function
            Invoke("Restart",(restartLevelDelay));
            //since level is over player enabled is:
            enabled = false;
        }
        else if (other.tag == "Food")
        {
            food += pointsPerFood;
            foodText.text = $"+ {pointsPerFood} Food: {food}";
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
            //disabling the already pickedup food from screen
            other.gameObject.SetActive(false);
        }
        else if (other.tag == "Soda")
        {
            food += pointsPerSoda;
            foodText.text = $"+ {pointsPerSoda} Food: {food}";
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);

            //disabling the already pickedup food from screen
            other.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Defines what to do if player can't move while encountering generic parameter <typeparamref name="T"/>. 
    /// Also takes one parameter of type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Component"></param>
    protected override void OnCantMove<T>(T Component)
    {
        Wall hitWall = Component as Wall;
        hitWall.DamagedWall(wallDamage);
        animator.SetTrigger("playerChop");
    }

    /// <summary>
    /// Called when player collides with "Exit" and loads a new procedurali generated level.
    /// </summary>
    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Enables player to loos food when hit.
    /// </summary>
    /// <param name="loss"></param>
    public void LoseFood(int loss)
    {
        animator.SetTrigger("playerHit");
        food -= loss;
        foodText.text = $"- {loss} Food: {food}";
        CheckIfGameOver();
    }

    private void CheckIfGameOver()
    {
        if (food <= 0)
        {
            SoundManager.instance.PlaySingle(gameOverSound);
            SoundManager.instance.musicSource.Stop();

            GameManager.instance.GameOver();
        }
    }
}
