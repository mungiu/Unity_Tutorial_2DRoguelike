using System.Collections.Generic;
using System;
using UnityEngine;
//because this class exists both in System and unity engine name space
using Random = UnityEngine.Random;

/// <summary>
/// Controls the procedural code for generating levels.
/// </summary>
public class BoardManager : MonoBehaviour {

    [Serializable]
    public class Count
    {
        public int minimum;
        public int maximum;

        public Count(int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }

    public int columns = 8;
    public int rows = 8;
    public Count wallCount = new Count(5, 9);
    public Count foodCount = new Count(1, 5);
    public GameObject exit;
    public GameObject[] floorTiles;
    public GameObject[] wallTiles;
    public GameObject[] outerWallTiles;
    public GameObject[] foodTiles;
    public GameObject[] enemyTiles;

    //used to keep hierarchy clean
    private Transform boardHolder;
    //keeps track of all active game position and weather an object was spawned there or not
    private List<Vector3> gridPositions = new List<Vector3>();

    /// <summary>
    /// A list of positions for walls, enemies or pickups on the active game board.
    /// </summary>
    void InitialiseList()
    {
        gridPositions.Clear();
        //the 1 & -1 ensure the edges are always clear
        for (int x = 1; x < columns -1; x++)
        {
            for (int y = 0; y < rows -1; y++)
            {
                gridPositions.Add(new Vector3(x, y, 0f));
            }
        }
    }

    /// <summary>
    /// Creates the outer wall and floor background on the game board.
    /// </summary>
    void BoardSetup()
    {
        boardHolder = new GameObject("Board").transform;
        //the -1 & +1 allows to build an edge arround the active portion of the game board
        for (int x = -1; x < columns +1; x++)
        {
            for (int y = -1; y < rows +1; y++)
            {
                //Instantiating all floor tiles
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
                //checking if we are in an outer wall position
                if (x == -1 || x == columns || y == -1 || y == rows)
                    //selecting what to instantiate
                    toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                //instantiating selection
                GameObject instance = Instantiate(
                    toInstantiate, 
                    new Vector3(x, y, 0f), 
                    Quaternion.identity) as GameObject;
                //assigning parent to instantiated object
                instance.transform.SetParent(boardHolder);
            }
        }
    }

    /// <summary>
    /// Creates a new random position which can not be duplicated later.
    /// </summary>
    Vector3 RandomPosition()
    {
        //storing a random number of the active game board coordinates
        int randomIndex = Random.Range(0, gridPositions.Count);
        //creating a random position based on above random index
        Vector3 randomPosition = gridPositions[randomIndex];
        //removing the occupied grind position from the griPosition[]
        gridPositions.RemoveAt(randomIndex);
        return randomPosition;
    }

    /// <summary>
    /// Spawning tiles of choice at chosen random position.
    /// </summary>
    /// <param name="tileArray">Array of chosen tiles</param>
    /// <param name="minimum">Minimum coordinates</param>
    /// <param name="maximum">Maximum coordinates</param>
    void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
    {
        //how many of a given object are we going to spawn
        int objectCount = Random.Range(minimum, maximum+1);
        for (int i = 0; i < objectCount; i++)
        {
            //creating new random position on active game board
            Vector3 randomPosition = RandomPosition();
            //chosinga random tile to spawn
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
            //instantiating the chosen tile and random position
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }

    }

    /// <summary>
    /// Called by game manager when it is time to setup the new game board.
    /// </summary>
    /// <param name="level"></param>
    public void SetupScene(int level)
    {
        BoardSetup();
        InitialiseList();
        LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);
        LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);
        //deciding on amount of enemies to spawn each round NOTE: casting result to an int
        int enemyCount = (int)Mathf.Log(level, 2f);
        LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);
        Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);
    }
}
