﻿using System.Collections;
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
    public Image healthBar; // Health UI
    public Image healthAbility; // Health ability UI
    public Image moveAbility; // Move range ability UI
    public Image attackPowerAbility; // Attack power ability UI
    public Image attackRangeAbility; // Attack range ability UI
    public TMP_Text reproduceText; // The text shows this clone's reproduce turn

    public bool hasMoved; // Has this unit moved in this turn
    public int maxHealth; // Max initial health
    public int moveRange; // How far can this unit move each turn (diagnol is considered 2 distance)
    public int attackRange; // How far can this unit attack
    public int attackPower; // How much is this unit's attack power
    public int health; // How much health this unit left
    public int gatheredBiomass; // How much biomass this clone currently own (currently only need 1, each enemy also has 1, if an enemy is killed its biomass will be given to the clone that killed it)
    public bool isReproducing; // Is this clone currently reproducing
    public int turnsLeftForReproduce; // How many turns are left for the reproduction process to finish

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
        healthBar.fillAmount = (float)health / (float)maxHealth; // Update health bar

        // Update reproducing UI
        TurnManager.sTurnManager.UpdateTurnTip(reproduceText.transform.parent.gameObject, isReproducing);
        if (isReproducing)
        {
            reproduceText.text = turnsLeftForReproduce.ToString();
        }
    }

    public void InitiateClone()
    {
        isReproducing = false;
        hasMoved = false;
        // GetComponent<MapObjectInfo>().currentOccupyingTile = 

        maxHealth = GameManager.playerUnitEvolvedMaxHealth;
        health = GameManager.playerUnitEvolvedMaxHealth;
        moveRange = GameManager.playerUnitEvolvedMoveRange;
        attackRange = GameManager.playerUnitEvolvedAttackRange;
        attackPower = GameManager.playerUnitEvolvedAttackPower;

        CreateCloneLook(); // Change new clone's look
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
    /// Create new clone body features based on current evolved abilities
    /// </summary>
    public void CreateCloneLook()
    {
        // Update ability indicators
        UpdateAbilityIndicator(healthAbility, maxHealth - GameManager.sGameManager.playerUnitInitialMaxHealth, GameManager.maxPlayerMaxHealth - GameManager.sGameManager.playerUnitInitialMaxHealth);
        UpdateAbilityIndicator(moveAbility, moveRange - GameManager.sGameManager.playerUnitInitialMoveRange, GameManager.maxPlayerMoveRange - GameManager.sGameManager.playerUnitInitialMoveRange);
        UpdateAbilityIndicator(attackPowerAbility, attackPower - GameManager.sGameManager.playerUnitInitialAttackPower, GameManager.maxPlayerAttackPower - GameManager.sGameManager.playerUnitInitialAttackPower);
        UpdateAbilityIndicator(attackRangeAbility, attackRange - GameManager.sGameManager.playerUnitInitialAttackPower, GameManager.maxPlayerAttackRange - GameManager.sGameManager.playerUnitInitialAttackPower);
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
        yield return new WaitForSeconds(moveAnimationInterval);

        GameObject newClone = Instantiate(gameObject);
        PlayerUnit newCloneUnit = newClone.GetComponent<PlayerUnit>();
        newCloneUnit.GetComponent<MapObjectInfo>().currentOccupyingTile = null; // Reset new clone's MapObjectInfo's occupying tile info, because it has not been placed on the map yet

        // Place new clone on target tile
        MapManager.PlaceObject(newClone.transform, targetTile.xCoord, targetTile.zCoord);

        newCloneUnit.InitiateClone();

        // Add new clone to new clone list
        TurnManager.newCloneReproduced.Add(newCloneUnit);

        // Clear selected tile in TurnManager;
        TurnManager.currentSelectedUnitTile = null;

        yield return new WaitForSeconds(moveAnimationInterval);

        // If new clone meet the win requirement
        if (GameManager.sGameManager.CheckWinCondition(newCloneUnit))
        {
            //GameManager.playerUnits.Add(newCloneUnit);
            GameManager.sGameManager.PlayerWin();
        }

        // Finish reproduction
        TurnManager.playerUnitReproducing = false;
        isReproducing = false;

        // Finish animation
        TurnManager.inPlayerUnitAnimation = false;
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
        TurnManager.currentSelectedUnitTile = GetComponent<MapObjectInfo>().currentOccupyingTile; // Update selected unit tile
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

        // If the enemy dead
        if (enemy.health <= 0)
        {
            enemy.EnemyDie(); // Enemy dies
            isReproducing = true; // Start reproducing
            turnsLeftForReproduce = reproduceTurns; // Start counting turns to reproduce
        }

        // Finish animation
        TurnManager.inPlayerUnitAnimation = false;

        TurnManager.sTurnManager.UnitFinishAct(); // Unit finish act phase
    }
}
