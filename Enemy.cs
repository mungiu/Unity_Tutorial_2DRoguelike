using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject
{
    public int playerDamage;
    public AudioClip enemyAtack1;
    public AudioClip enemyAtack2;

    private Animator animator;
    private Transform target;
    private bool skipMove;

	// Use this for initialization
	protected override void Start ()
    {
        //making the enemy automatically register itself in the GameManagers enemy list
        GameManager.instance.AddEnemyToList(this);
        animator = GetComponent<Animator>();
        //storing transform of player as "target"
        target = GameObject.FindGameObjectWithTag("Player").transform;

        base.Start();
	}

    /// <summary>
    /// Attempts t move while expecting to encounter T. (aka the Player in this case)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="xDir"></param>
    /// <param name="yDir"></param>
    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        //making enemy only move every other turn
        if (skipMove)
        {
            skipMove = false;
            return;
        }

        base.AttemptMove<T>(xDir, yDir);
        skipMove = true;
    }

    /// <summary>
    /// Checking position of self vs target and figuring out which direction to move (x or y axis)
    /// </summary>
    public void MoveEnemy()
    {
        int xDir = 0;
        int yDir = 0;

        //checking relative x axis position
        if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
            //if above true, generating required movement of .this on y axis towards player
            yDir = target.position.y > transform.position.y ? 1 : -1;
        else
            //attempting to move .this closer to player x axis position
            xDir = target.position.x > transform.position.x ? 1 : -1;

        AttemptMove<Player>(xDir, yDir);
    }

    /// <summary>
    /// Defines what to do if object can't move while encountering Player
    /// </summary>
    /// <typeparam name="T">Player</typeparam>
    /// <param name="Component"></param>
    protected override void OnCantMove<T>(T Component)
    {
        //storing enemy attack
        Player hitPlayer = Component as Player;
        animator.SetTrigger("enemyAttack");
        SoundManager.instance.RandomizeSfx(enemyAtack1, enemyAtack2);
        //how much food should the player lose
        hitPlayer.LoseFood(playerDamage);
    }
}
