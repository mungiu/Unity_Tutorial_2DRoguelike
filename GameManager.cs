using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour {

    //belongs to the class (not to instance of class)
    //which means another instance of it can not be created as it was already created
    //as strongly typed static/null object
    public static GameManager instance = null;
    public BoardManager boardScript;
    public int playerFoodPoints = 100;
    [HideInInspector]public bool playersTurn = true;
    public float turnDelay = .1f;
    public float levelStartDelay = 2f;

    private Text levelText;
    private GameObject levelImage;
    private int level = 1;
    private List<Enemy> enemies;
    private bool enemiesMoving;
    private bool doingSetup;

    /// <summary>
    /// Used to disable the game editor.
    /// </summary>
    public void GameOver()
    {
        levelText.text = $"After {level} days, you starved.";
        //enabling black background
        levelImage.SetActive(true);
        enabled = false;
    }

    void Update()
    {
        //code execution stopped when players turn or doingSetup = true
        if (playersTurn || enemiesMoving || doingSetup)
            return;

        StartCoroutine(MoveEnemies());
    }

    /// <summary>
    /// Allowing to register enemies with the GameManager, so that it can issue move order to them.
    /// </summary>
    /// <param name="script"></param>
    public void AddEnemyToList(Enemy script)
    {
        enemies.Add(script);
    }

    // Use this for initialization
    void Awake()
    {
        //checking this instance of GameManager
        if (instance == null)
            instance = this;
        //ensuring game manager doesn;t spawn additional instances of itself
        else if (instance != this)
            Destroy(gameObject);

        //making sure we have the same game manager throughout all scenes (since it keeps track of game progress)
        DontDestroyOnLoad(gameObject);

        enemies = new List<Enemy>();
        boardScript = GetComponent<BoardManager>();

        InitGame();
	}

    private void OnLevelWasLoaded(int level)
    {
        instance.level++;
        instance.InitGame();
    }

    /// <summary>
    /// Game initializer.
    /// </summary>
    void InitGame()
    {
        //player can't move
        doingSetup = true;
        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();
        levelText.text = $"Day {level}";
        levelImage.SetActive(true);
        Invoke("HideLevelImage", levelStartDelay);

        //since game manager is not reset when level starts, we clear out any enemies from last level
        enemies.Clear();
        //telling BoardManager boardScript what level the scene we are setting up is
        boardScript.SetupScene(level);
    }

    private void HideLevelImage()
    {
        levelImage.SetActive(false);
        //player can move again
        doingSetup = false;
    }

    /// <summary>
    /// Moving enemies one at a time in sequence.
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveEnemies()
    {
        enemiesMoving = true;

        yield return new WaitForSeconds(turnDelay);

        if (enemies.Count == 0)
            yield return new WaitForSeconds(turnDelay);

        //loop through enemy list and issue move enemy command to each
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].MoveEnemy();
            //giving enemy time to finalize maneuver
            yield return new WaitForSeconds(enemies[i].moveTime);
        }

        playersTurn = true;
        enemiesMoving = false;
    }
}
