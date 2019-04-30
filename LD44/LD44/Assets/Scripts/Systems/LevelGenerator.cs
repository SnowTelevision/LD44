using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{


    // Static ref
    public static LevelGenerator sLevelGenerator;

    private void OnEnable()
    {
        sLevelGenerator = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Generate a new level map
    /// </summary>
    public void GenerateNewLevel()
    {


        //PlacePlayerUnits();
        //PlaceObstacles();
        PlaceNewEnemies();
    }

    /// <summary>
    /// Place down obstacles
    /// </summary>
    public void PlaceObstacles()
    {

    }

    /// <summary>
    /// Place down enemy units
    /// </summary>
    public void PlaceNewEnemies()
    {
        // Get a list of empty map border tiles
        List<GridTileInfo> emptyBorderTiles = MapManager.sMapManager.GetEmptyBorderTiles();

        // Get number of enemies to be generated
        int maxEnemyAmount = Mathf.Clamp(Mathf.FloorToInt(GameManager.playerTotalPower / EnemyManager.minEnemyPower), 0, emptyBorderTiles.Count);
        int minEnemyAmount = Mathf.Clamp(Mathf.CeilToInt(GameManager.playerTotalPower / EnemyManager.maxEnemyPower), 1, maxEnemyAmount); // Make sure there is at least one enemy being generated
        int enemyAmount = BetterRandom.betterRandom(minEnemyAmount, maxEnemyAmount);

        // Get individual power for each enemy
        int[] individualEnemyPower = new int[enemyAmount];

        for (int i = 0; i < individualEnemyPower.Length; i++)
        {
            individualEnemyPower[i] = EnemyManager.maxEnemyPower;
        }

        int powerDifference = enemyAmount * EnemyManager.maxEnemyPower - GameManager.playerTotalPower; // Get power difference

        while (powerDifference > 0) // Randomly decrease enemy power
        {
            int selectedEnemy = BetterRandom.betterRandom(0, individualEnemyPower.Length - 1); // Randomly select an enemy

            // Decrement selected enemy power if it has not reached minEnemyPower
            if (individualEnemyPower[selectedEnemy] > EnemyManager.minEnemyPower)
            {
                individualEnemyPower[selectedEnemy]--;
                powerDifference--;
            }
        }

        // Assign spawn positions
        List<int> spawnPosition = new List<int>();

        for (int i = 0; i < enemyAmount; i++)
        {
            int newPosition = BetterRandom.betterRandom(0, emptyBorderTiles.Count - 1);

            while (spawnPosition.Contains(newPosition))
            {
                newPosition = BetterRandom.betterRandom(0, emptyBorderTiles.Count - 1);
            }

            spawnPosition.Add(newPosition);
        }

        // Create new enemies
        for (int i = 0; i < enemyAmount; i++)
        {
            EnemyManager.sEnemyManager.CreateEnemy(EnemyManager.sEnemyManager.CreateNewEnemyType(individualEnemyPower[i]), emptyBorderTiles[spawnPosition[i]].xCoord, emptyBorderTiles[spawnPosition[i]].zCoord);
        }

        // Test
        //EnemyManager.sEnemyManager.CreateEnemy(EnemyManager.EnemyType.Test, 4, 4);
    }

    /// <summary>
    /// Place player units at the beginning of a level
    /// </summary>
    public void PlacePlayerUnits()
    {


        // Test
        MapManager.PlaceObject(GameManager.playerUnits[0].transform, 0, 0);
    }
}
