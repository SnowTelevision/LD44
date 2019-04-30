using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour
{

    public float defaultWaitBeforeEnemyAct; // How long is the default time to wait before each enemy start its actions
    public List<EnemyTypeInfo> enemyTypes; // List of enemy types
    public GameObject enemyPrefab; // Prefab for the enemy

    public static List<EnemyUnit> thisLevelEnemies; // A list store the enemies in the current level
    public static EnemyManager sEnemyManager;
    public static bool enemyUnitActing; // Is there an enemy unit doing its moves currently
    public static int minEnemyPower;
    public static int minEnemyMaxHealth;
    public static int minEnemyMoveRange;
    public static int minEnemyAttackPower;
    public static int minEnemyAttackRange;
    public static int maxEnemyPower;
    public static int maxEnemyMaxHealth;
    public static int maxEnemyMoveRange;
    public static int maxEnemyAttackPower;
    public static int maxEnemyAttackRange;

    private void OnEnable()
    {
        sEnemyManager = this;

        GameManager instance = (GameManager)FindObjectOfType(typeof(GameManager));

        minEnemyMaxHealth = instance.playerUnitInitialMaxHealth;
        minEnemyMoveRange = instance.playerUnitInitialMoveRange;
        minEnemyAttackPower = instance.playerUnitInitialAttackPower;
        minEnemyAttackRange = instance.playerUnitInitialAttackRange;
        minEnemyPower = minEnemyAttackPower + minEnemyAttackRange + minEnemyMaxHealth + minEnemyMoveRange;

        maxEnemyMaxHealth = minEnemyMaxHealth + 4;
        maxEnemyMoveRange = minEnemyMoveRange + 4;
        maxEnemyAttackPower = minEnemyAttackPower + 4;
        maxEnemyAttackRange = minEnemyAttackRange + 4;
        maxEnemyPower = maxEnemyAttackPower + maxEnemyAttackRange + maxEnemyMaxHealth + maxEnemyMoveRange;

        thisLevelEnemies = new List<EnemyUnit>();
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
    /// Create a new enemy and place on a postion on the map
    /// Usually happened during map generation
    /// </summary>
    /// <param name="type"></param>
    /// <param name="xCoord"></param>
    /// <param name="zCoord"></param>
    public void CreateEnemy(EnemyTypeInfo typeInfo, int xCoord, int zCoord)
    {
        //// If there is no such enemy type
        //if (!enemyTypes.Exists(t => t.typeName == type))
        //{
        //    return;
        //}

        //// Create an enemy of this type
        //EnemyUnit newEnemy = new EnemyUnit();
        //newEnemy.Initialize(enemyTypes.Find(t => t.typeName == type));

        // Create an enemy with input type info
        GameObject newEnemy = Instantiate(enemyPrefab);

        newEnemy.GetComponent<EnemyUnit>().Initialize(typeInfo);

        // Add new enemy to EnemyManager
        thisLevelEnemies.Add(newEnemy.GetComponent<EnemyUnit>());

        // Place new enemy on target tile
        MapManager.PlaceObject(newEnemy.transform, xCoord, zCoord);
    }

    /// <summary>
    /// Randomly generate a new enemy type with a certain total power
    /// 1: move
    /// 2: hp
    /// 3: power
    /// 4: range
    /// </summary>
    /// <param name="totalPower"></param>
    /// <returns></returns>
    public EnemyTypeInfo CreateNewEnemyType(int totalPower)
    {
        EnemyTypeInfo newTypeInfo = new EnemyTypeInfo();

        int[] abilityStrength = { maxEnemyMoveRange, maxEnemyMaxHealth, maxEnemyAttackPower, maxEnemyAttackRange };
        int[] minAbilityStrength = { minEnemyMoveRange, minEnemyMaxHealth, minEnemyAttackPower, minEnemyAttackRange };
        int targetDifference = maxEnemyMoveRange + maxEnemyMaxHealth + maxEnemyAttackPower + maxEnemyAttackRange - totalPower;

        // Reduce ability strength to match total power
        while (targetDifference > 0)
        {
            int ability = BetterRandom.betterRandom(0, abilityStrength.Length - 1);

            // If strength can be decreased
            if (abilityStrength[ability] > minAbilityStrength[ability])
            {
                abilityStrength[ability]--;
                targetDifference--;
            }
        }

        newTypeInfo.moveRange = abilityStrength[0];
        newTypeInfo.maxHealth = abilityStrength[1];
        newTypeInfo.attackPower = abilityStrength[2];
        newTypeInfo.attackRange = abilityStrength[3];

        return newTypeInfo;
    }

    /// <summary>
    /// Move enemies and let them use abilities during the enemy turn
    /// For now just let each enemy move in the default order
    /// </summary>
    public IEnumerator ActEnemies()
    {
        enemyUnitActing = false;

        for (int i = 0; i < thisLevelEnemies.Count; i++)
        {
            // Wait if there is an enemy unit currently executing actions
            while (enemyUnitActing)
            {
                yield return null;
            }

            // Only play waiting animation if this enemy is visible to player
            if (thisLevelEnemies[i].isVisible)
            {
                // Wait some time before each enemy start its actions
                yield return new WaitForSeconds(defaultWaitBeforeEnemyAct);

                // Let camera focus on enemy that's visible to the player (not in fog of war)
            }

            // Start waiting for the next enemy to act
            enemyUnitActing = true;

            // Start next enemy's actions
            StartCoroutine(thisLevelEnemies[i].Act());
        }

        TurnManager.sTurnManager.enemyTurnTip.SetActive(true); // Hide enemy turn UI tip
        TurnManager.sTurnManager.StartNewRound(); // Start new round
    }

    public enum EnemyType
    {
        Test
    }

    [Serializable]
    public class EnemyTypeInfo
    {
        public EnemyType typeName;
        public int maxHealth;
        public int moveRange;
        public UnityEvent[] enemyMoves; // The enemy's "abilities"
        public int attackRange;
        public int attackPower;
    }
}
