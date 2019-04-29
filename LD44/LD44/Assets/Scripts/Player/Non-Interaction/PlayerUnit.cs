using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Info and maybe some functions for player unit
/// </summary>
public class PlayerUnit : MonoBehaviour
{
    public float moveAnimationInterval; // How much time wait before the unit move to next tile during the move animation
    public int biomassToReproduce; // How much biomass is needed for the clone to start reproduce
    public int reproduceTurns; // How many turns is required for the reproduction process to finish
    public GameObject healthMark; // Represent player HP "heart"
    public GameObject moveMark; // Represent player move range "leg"
    public GameObject rangeMark; // Represent player attack range "bullseye"
    public GameObject powerMark; // Represent player attack power "fist"

    public bool hasMoved; // Has this unit moved in this turn
    public int maxHealth; // Max initial health
    public int moveRange; // How far can this unit move each turn (diagnol is considered 2 distance)
    public int attackRange; // How far can this unit attack
    public int attackPower; // How much is this unit's attack power
    public int health; // How much health this unit left
    public int gatheredBiomass; // How much biomass this clone currently own (currently only need 1, each enemy also has 1, if an enemy is killed its biomass will be given to the clone that killed it)
    public bool isReproducing; // Is this clone currently reproducing
    public int turnsLeftForReproduce; // How many turns are left for the reproduction process to finish
    public TMP_Text healthText; // The text shows this clone's current hp

    private void OnEnable()
    {
        InitiateClone();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitiateClone()
    {
        maxHealth = GameManager.playerUnitEvolvedMaxHealth;
        health = GameManager.playerUnitEvolvedMaxHealth;
        moveRange = GameManager.playerUnitEvolvedMoveRange;
        attackRange = GameManager.playerUnitEvolvedAttackRange;
        attackPower = GameManager.playerUnitEvolvedAttackPower;

        CreateCloneLook(); // Change new clone's look
    }

    /// <summary>
    /// Create new clone body features based on current evolved abilities
    /// </summary>
    public void CreateCloneLook()
    {

    }

    public void CloneDie()
    {
        GetComponent<MapObjectInfo>().currentOccupyingTile.containingObject = null; // Clear the tile it is occupying
        GameManager.playerUnits.Remove(this); // Remove it from the GameManager

        // If there is no more player unit left
        if (GameManager.playerUnits.Count == 0)
        {
            GameManager.sGameManager.PlayerLose(); // Player lose
        }

        Destroy(gameObject); // Destroy object
    }

    public IEnumerator UnitReproduce(GridTileInfo targetTile)
    {
        yield return null;

        GameObject newClone = Instantiate(gameObject);

        // Place new clone on target tile
        MapManager.PlaceObject(newClone.transform, targetTile.xCoord, targetTile.zCoord);

        newClone.GetComponent<PlayerUnit>().InitiateClone();

        // Add new clone to GameManager
        GameManager.playerUnits.Add(newClone.GetComponent<PlayerUnit>());

        // Clear selected tile in TurnManager;
        TurnManager.currentSelectedUnitTile = null;

        // Finish reproduction
        TurnManager.playerUnitReproducing = false;
        isReproducing = false;
    }

    /// <summary>
    /// Move a player unit
    /// </summary>
    /// <param name="moveRoute"></param>
    /// <returns></returns>
    public IEnumerator UnitMove(List<GridTileInfo> moveRoute)
    {
        TurnManager.playerUnitAct = true; // Unit enters act phase

        foreach (GridTileInfo t in moveRoute)
        {
            yield return new WaitForSeconds(moveAnimationInterval);

            // Move unit for one tile
            MapManager.PlaceObject(transform, t.xCoord, t.zCoord);
        }

        TurnManager.inPlayerUnitAnimation = false; // Unit move animation finished
        TurnManager.sTurnManager.ShowRange(GetComponent<MapObjectInfo>().currentOccupyingTile, attackRange, false); // Show this unit's attack range
    }

    /// <summary>
    /// Attack an enemy
    /// </summary>
    /// <param name="enemy"></param>
    /// <returns></returns>
    public IEnumerator UnitAttack(EnemyUnit enemy)
    {
        yield return null;

        enemy.health -= attackPower; // Decrease enemy health

        // If the enemy is still alive
        if (enemy.health > 0)
        {
            TurnManager.sTurnManager.UnitFinishAct(); // Unit finish act phase
        }
        else
        {
            enemy.EnemyDie(); // Enemy dies
            isReproducing = true; // Start reproducing
            turnsLeftForReproduce = reproduceTurns; // Start counting turns to reproduce
        }
    }
}
