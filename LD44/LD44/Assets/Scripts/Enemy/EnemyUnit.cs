using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EnemyUnit : MonoBehaviour
{
    public float animationInterval; // Time wait before each animation
    public Image healthBar; // Health UI
    public Image healthAbility; // Health ability UI
    public Image moveAbility; // Move range ability UI
    public Image attackPowerAbility; // Attack power ability UI
    public Image attackRangeAbility; // Attack range ability UI

    public bool isInSomeMove; // Is this enemy currently in some action, if yes then other moves should wait for it to finish
    public bool isVisible; // Is this enemy visible to the player

    public UnityEvent[] enemyMoves;
    public int maxHealth; // Max initial health
    public int health; // Enemy's current health
    public int moveRange;
    public int attackPower;
    public int attackRange;

    private void OnEnable()
    {

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        healthBar.fillAmount = (float)health / (float)maxHealth;
    }

    /// <summary>
    /// Initialize an enemy
    /// </summary>
    public void Initialize(EnemyManager.EnemyTypeInfo info)
    {
        maxHealth = info.maxHealth;
        health = info.maxHealth;
        moveRange = info.moveRange;
        enemyMoves = info.enemyMoves;
        attackPower = info.attackPower;
        attackRange = info.attackRange;

        // Update ability indicators
        UpdateAbilityIndicator(healthAbility, maxHealth - EnemyManager.minEnemyMaxHealth, EnemyManager.maxEnemyMaxHealth - EnemyManager.minEnemyMaxHealth);
        UpdateAbilityIndicator(moveAbility, moveRange - EnemyManager.minEnemyMoveRange, EnemyManager.maxEnemyMoveRange - EnemyManager.minEnemyMoveRange);
        UpdateAbilityIndicator(attackPowerAbility, attackPower - EnemyManager.minEnemyAttackPower, EnemyManager.maxEnemyAttackPower - EnemyManager.minEnemyAttackPower);
        UpdateAbilityIndicator(attackRangeAbility, attackRange - EnemyManager.minEnemyAttackRange, EnemyManager.maxEnemyAttackRange - EnemyManager.minEnemyAttackRange);
    }

    /// <summary>
    /// Update an ability indicator based on unit ability's current strength and max possible strength
    /// </summary>
    /// <param name="mask"></param>
    /// <param name="currentStrength"></param>
    /// <param name="maxStrength"></param>
    public void UpdateAbilityIndicator(Image mask, int currentStrength, int maxStrength)
    {
        mask.fillAmount = (float)(currentStrength + 1) / (float)(maxStrength + 1); // + 1 because initial strength is 1
    }

    /// <summary>
    /// Create new enemy body features based on enemy status
    /// </summary>
    public void CreateEnemyLook()
    {

    }

    /// <summary>
    /// Move an enemy unit
    /// </summary>
    /// <param name="moveRoute"></param>
    /// <returns></returns>
    public IEnumerator EnemyMove()
    {
        List<GridTileInfo> moveRoute = FindRoute(); // Get a move route

        // If cannot find a route then stay in place
        if (moveRoute == null)
        {
            yield return new WaitForSeconds(animationInterval);
        }
        else
        {
            for (int i = 0; i < moveRange; i++)
            {
                yield return new WaitForSeconds(animationInterval);

                // Move unit for one tile
                MapManager.PlaceObject(transform, moveRoute[i].xCoord, moveRoute[i].zCoord);
            }
        }

        isInSomeMove = false; // Finish current move
    }

    /// <summary>
    /// Find a route
    /// </summary>
    /// <returns></returns>
    public List<GridTileInfo> FindRoute()
    {
        int shortestRoute = MapManager.sMapManager.mapSizeX * MapManager.sMapManager.mapSizeZ;
        GridTileInfo occupyingTile = GetComponent<MapObjectInfo>().currentOccupyingTile;
        List<GridTileInfo> finalRoute = null;

        // Test all player units
        foreach (PlayerUnit p in GameManager.playerUnits)
        {
            // Try to get a route to p
            List<GridTileInfo> route = MapManager.sMapManager.FindPath(occupyingTile, p.GetComponent<MapObjectInfo>().currentOccupyingTile);

            // If there is a route and the route is currently the shortest
            if (route != null && route.Count < shortestRoute)
            {
                shortestRoute = route.Count;
                finalRoute = route;
            }
        }

        return finalRoute;

        // If there is no player unit that is reachable by this enemy
        if (finalRoute == null)
        {
            PlayerUnit closestPlayerUnit = null;

            // Test all player manhatten distance
            foreach (PlayerUnit p in GameManager.playerUnits)
            {
                // Get manhatten distance
                int distance = MapManager.sMapManager.GetManhattenDistance(occupyingTile, p.GetComponent<MapObjectInfo>().currentOccupyingTile);

                // If this player has the closest manhatten distance
                if (distance < shortestRoute)
                {
                    shortestRoute = distance;
                    closestPlayerUnit = p;
                }
            }
        }
        else
        {
            return finalRoute;
        }
    }

    /// <summary>
    /// Try to attack a player clone
    /// </summary>
    /// <param name="enemy"></param>
    /// <returns></returns>
    public IEnumerator EnemyAttack(PlayerUnit target)
    {
        yield return new WaitForSeconds(animationInterval);

        // If target is within attack range
        if (MapManager.sMapManager.GetManhattenDistance(GetComponent<MapObjectInfo>().currentOccupyingTile, target.GetComponent<MapObjectInfo>().currentOccupyingTile) <= attackRange)
        {
            target.health -= attackPower; // Decrease enemy health

            // If the player clone dead
            if (target.health <= 0)
            {
                target.CloneDie();
            }
        }

        isInSomeMove = false; // Finish attack
    }

    /// <summary>
    /// An enemy execute its actions, including move and use abilities
    /// </summary>
    public IEnumerator Act()
    {
        //isInSomeMove = false;

        //for (int i = 0; i < enemyMoves.Length; i++)
        //{
        //    // Wait for the previous move to finish
        //    while (isInSomeMove)
        //    {
        //        yield return null;
        //    }

        //    isInSomeMove = true;


        //}

        isInSomeMove = true; // Start move enemy

        StartCoroutine(EnemyMove());

        while (isInSomeMove) // Wait for enemy move
        {
            yield return null;
        }

        PlayerUnit closestUnit = FindClosestPlayerUnit();

        isInSomeMove = true; // Start enemy attack

        StartCoroutine(EnemyAttack(closestUnit));

        EnemyManager.enemyUnitActing = false;
    }

    /// <summary>
    /// Return the closest player unit to this enemy at this time
    /// </summary>
    /// <returns></returns>
    public PlayerUnit FindClosestPlayerUnit()
    {
        int closestPlayerDistance = MapManager.sMapManager.mapSizeX * MapManager.sMapManager.mapSizeZ;
        PlayerUnit closestPlayerUnit = null;

        // Test all player manhatten distance
        foreach (PlayerUnit p in GameManager.playerUnits)
        {
            // Get manhatten distance
            int distance = MapManager.sMapManager.GetManhattenDistance(GetComponent<MapObjectInfo>().currentOccupyingTile, p.GetComponent<MapObjectInfo>().currentOccupyingTile);

            // If this player has the closest manhatten distance
            if (distance < closestPlayerDistance)
            {
                closestPlayerDistance = distance;
                closestPlayerUnit = p;
            }
        }

        return closestPlayerUnit;
    }

    /// <summary>
    /// 
    /// </summary>
    public void EnemyDie()
    {
        GetComponent<MapObjectInfo>().currentOccupyingTile.containingObject = null; // Clear the tile it is occupying
        EnemyManager.thisLevelEnemies.Remove(this); // Remove it from the EnemyManager

        // Finish animation
        TurnManager.inPlayerUnitAnimation = false;

        Destroy(gameObject); // Destroy object
    }
}
