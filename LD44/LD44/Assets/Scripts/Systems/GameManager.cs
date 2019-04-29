using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int playerUnitInitialMaxHealth; // Player unit's initial max health
    public int playerUnitInitialMoveRange; // Player unit's initial move range
    public int playerUnitInitialAttackRange; // Player unit's initial attack range
    public int playerUnitInitialAttackPower; // Player unit's initial attack power
    public GameObject evolveMenu; // The UI menu control the evolve shop when player selected an unit during evolve phase
    public GameObject evolveInterface; // The UI menu control the evolve interface
    public Transform cameraTrans; // The game camera's transform
    public GameObject pauseMenu; // The pause menu
    public PlayerUnit gameGoal; // The example player clone that shows the goal of current game

    public static int playerTotalPower; // A float that estimates player's total power (include thing like player unit/health count, player upgrades, etc.)
    //public static GridTileInfo currentSelectedTile; // The map grid tile the player is currently selected (used to determine what player is doing / can do)
    public static List<PlayerUnit> playerUnits; // The list store all the current player units
    public static int playerUnitEvolvedMaxHealth; // Player unit's current evolved max health
    public static int playerUnitEvolvedMoveRange; // Player unit's current evolved move range (diagnol is considered 2 distance)
    public static int playerUnitEvolvedAttackRange; // Player unit's current evolved attack range
    public static int playerUnitEvolvedAttackPower; // Player unit's current evolved attack power
    public static bool inEvolvePhase; // Is the game currently in evolve phase where the player can choose to spend clones and evolve abilities
    public static int upgradingAbility; // The ability the player choose to upgrade
    public PlayerUnit cloneToSpend; // The player unit that's selected to be spend for evolve

    public static int maxPlayerMaxHealth;
    public static int maxPlayerMoveRange;
    public static int maxPlayerAttackPower;
    public static int maxPlayerAttackRange;

    public static GameManager sGameManager;

    private void OnEnable()
    {
        sGameManager = this;

        playerUnitEvolvedMaxHealth = playerUnitInitialMaxHealth;
        playerUnitEvolvedMoveRange = playerUnitInitialMoveRange;
        playerUnitEvolvedAttackPower = playerUnitInitialAttackPower;
        playerUnitEvolvedAttackRange = playerUnitInitialAttackRange;

        maxPlayerMaxHealth = playerUnitInitialMaxHealth + 4;
        maxPlayerMoveRange = playerUnitInitialMoveRange + 4;
        maxPlayerAttackPower = playerUnitInitialAttackPower + 4;
        maxPlayerAttackRange = playerUnitInitialAttackRange + 4;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartNewGame();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Start a new game
    /// </summary>
    public void StartNewGame()
    {
        GetWinCondition();
        TurnManager.playerTurn = true;
    }

    /// <summary>
    /// Get the win condition for current game
    /// </summary>
    public void GetWinCondition()
    {
        int requiredEvolvePower = BetterRandom.betterRandom(11, 15);
        int powerDifference = 16 - requiredEvolvePower;
        int[] targetAbilityPowers = { 4, 4, 4, 4 };

        while (powerDifference > 0)
        {
            int ability = BetterRandom.betterRandom(0, 3);

            if (targetAbilityPowers[ability] > 0)
            {
                targetAbilityPowers[ability]--;
                powerDifference--;
            }
        }

        cloneToSpend.maxHealth = playerUnitInitialMaxHealth + targetAbilityPowers[0];
        cloneToSpend.moveRange = playerUnitInitialMoveRange + targetAbilityPowers[1];
        cloneToSpend.attackPower = playerUnitInitialAttackPower + targetAbilityPowers[2];
        cloneToSpend.attackRange = playerUnitInitialAttackRange + targetAbilityPowers[3];
        cloneToSpend.CreateCloneLook();
        cloneToSpend.health = 0; // Hide health bar
    }

    /// <summary>
    /// Check if player win each time player create a new clone
    /// </summary>
    /// <param name="newClone"></param>
    /// <returns></returns>
    public bool CheckWinCondition(PlayerUnit newClone)
    {
        if (playerUnitEvolvedMaxHealth < cloneToSpend.maxHealth ||
            playerUnitEvolvedMoveRange < cloneToSpend.moveRange ||
            playerUnitEvolvedAttackPower < cloneToSpend.attackPower ||
            playerUnitEvolvedAttackRange < cloneToSpend.attackRange)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Player win
    /// </summary>
    public void PlayerWin()
    {
        print("win");
    }

    /// <summary>
    /// Open the evolve menu after player choose to spend a clone
    /// </summary>
    /// <param name="spendClone"></param>
    public void StartEvolve(PlayerUnit spendClone)
    {
        evolveMenu.SetActive(true); // Open the evolve shop
        cloneToSpend = spendClone;
    }

    /// <summary>
    /// Start new level
    /// </summary>
    public void StartNewLevel()
    {
        inEvolvePhase = false; // End previous evolve phase

        // Calculate current player power
        playerTotalPower = 0; // Reset

        if (playerUnits.Count > 0)
        {
            foreach (PlayerUnit p in playerUnits)
            {
                playerTotalPower += (p.maxHealth + p.attackPower + p.attackRange + p.moveRange); // Add this unit's power
            }
        }

        // Generate a new level
        LevelGenerator.sLevelGenerator.GenerateNewLevel();

        // Let turn manager start new round
        TurnManager.sTurnManager.StartNewRound();
    }

    /// <summary>
    /// Set the upgrading ability
    /// </summary>
    /// <param name="ability"></param>
    public void SetUpgradingAbility(int ability)
    {
        upgradingAbility = ability;
    }

    /// <summary>
    /// Upgrade one of the abilities for new player clones
    /// 1: move
    /// 2: hp
    /// 3: power
    /// 4: range
    /// </summary>
    public void UpgradeAbility()
    {
        if (upgradingAbility == 1)
        {
            playerUnitEvolvedMoveRange++;
        }
        if (upgradingAbility == 2)
        {
            playerUnitEvolvedMaxHealth++;
        }
        if (upgradingAbility == 3)
        {
            playerUnitEvolvedAttackPower++;
        }
        if (upgradingAbility == 4)
        {
            playerUnitEvolvedAttackRange++;
        }

        // Make the spent clone die
        cloneToSpend.CloneDie();
        cloneToSpend = null;
    }

    /// <summary>
    /// Cancel the evolve process for a player unit
    /// </summary>
    public void CancelEvolve()
    {
        upgradingAbility = 0;
        cloneToSpend = null;
    }

    /// <summary>
    /// When player killed all enemies
    /// </summary>
    public void LevelFinished()
    {
        EnterEvolvePhase(); // Enter evolve shop phase
    }

    /// <summary>
    /// Enters the evolve phase after each level is finished
    /// </summary>
    public void EnterEvolvePhase()
    {
        inEvolvePhase = true;
        TurnManager.playerTurn = false;
        evolveInterface.SetActive(true); // Show evolve interface
    }

    /// <summary>
    /// If player lose
    /// </summary>
    public void PlayerLose()
    {
        print("lose");
    }
}
