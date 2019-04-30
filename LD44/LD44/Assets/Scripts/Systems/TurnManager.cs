using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Manage turn-based logic
/// Also detect player mouse input on the game map
/// </summary>
public class TurnManager : MonoBehaviour
{
    public GameObject playerTurnTip; // UI text shows now is player's turn
    public GameObject playerSelectUnitTip; // UI text shows player should select an unit
    public GameObject playerMoveTip; // UI text shows player can move the selected unit
    public GameObject playerActTip; // UI text shows player should act the unit that just moved
    public GameObject reproductionPhaseTip; // UI text shows now is reproduction phase
    public GameObject chooseNewClonePositionTip; // UI shows need to choose position for new clone when a unit is ready to reproduce
    public GameObject enemyTurnTip; // UI shows now is enemy turn
    //public GameObject evolvePhaseTip; // UI shows now is evolve phase when each level is done

    public GridTileInfo currentMouseOverTile; // The grid tile that is currently under player mouse cursor
    public static bool playerTurn; // Is now player's turn
    public static bool inPlayerUnitAnimation; // Is a player unit currently in animation
    public static bool playerUnitAct; // Has the player moved a unit and should choose what the unit should do
    public static bool playerUnitReproducing; // Is a player's unit ready to reproduce
    public static GridTileInfo currentSelectedUnitTile; // The tile that the player is currently selected that has a player unit on it

    public static TurnManager sTurnManager;

    private void OnEnable()
    {
        sTurnManager = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ControlTurnTips(); // Update different UI tips about game phase

        // Player input
        currentMouseOverTile = null; // Clear the last hovered tile

        // If the player cursor is not on UI element
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // If the player cursor is on a tile
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
            {
                // Debug.Log(hit.collider.tag);
                if (hit.collider.tag == "Tile")
                {
                    currentMouseOverTile = hit.collider.GetComponent<GridTileInfo>();
                }
            }

            // If the player cursor is on a tile
            if (currentMouseOverTile != null)
            {
                PlayerHoverTile(currentMouseOverTile);

                // If the player clicked LMB on hovering tile
                if (Input.GetMouseButtonDown(0))
                {
                    PlayerSelectedTile(currentMouseOverTile);
                }
            }
        }
    }

    /// <summary>
    /// Control the UI tips tell player what to do
    /// </summary>
    public void ControlTurnTips()
    {

        UpdateTurnTip(playerTurnTip, playerTurn);

        UpdateTurnTip(playerSelectUnitTip, (playerTurn && currentSelectedUnitTile == null));

        UpdateTurnTip(playerMoveTip, (playerTurn && currentSelectedUnitTile != null && currentSelectedUnitTile.GetComponent<PlayerUnit>()));

        UpdateTurnTip(playerActTip, playerUnitAct);

        //UpdateTurnTip(reproductionPhaseTip, (!playerTurn && );

        UpdateTurnTip(chooseNewClonePositionTip, playerUnitReproducing);

        //UpdateTurnTip(enemyTurnTip, (!playerTurn);

        //UpdateTurnTip(evolvePhaseTip, GameManager.inEvolvePhase);
    }

    /// <summary>
    /// Update a turn tip based on current turn phase
    /// </summary>
    /// <param name="tip"></param>
    /// <param name="state"></param>
    public void UpdateTurnTip(GameObject tip, bool state)
    {
        if (tip.activeInHierarchy != state)
        {
            tip.SetActive(state);
        }
    }

    /// <summary>
    /// Start a new round
    /// </summary>
    public void StartNewRound()
    {
        StartCoroutine(UpdateReproducingPlayerUnits()); // Update the reproducing clones
    }

    /// <summary>
    /// Update the player units that are reproducing
    /// </summary>
    public IEnumerator UpdateReproducingPlayerUnits()
    {
        reproductionPhaseTip.SetActive(true); // Show reproduction phase UI tip

        foreach (PlayerUnit p in GameManager.playerUnits)
        {
            // Wait if there is a player unit currently ready to reproduce
            while (playerUnitReproducing)
            {
                yield return null;
            }

            yield return new WaitForSeconds(p.moveAnimationInterval);

            // Focus cam on reproducing unit

            p.turnsLeftForReproduce--;

            // If the clone is ready to reproduce
            if (p.turnsLeftForReproduce == 0)
            {
                // If there is empty neighbor to place new clone
                if (ShowRange(p.GetComponent<MapObjectInfo>().currentOccupyingTile, 1, true) > 0)
                {
                    currentSelectedUnitTile = p.GetComponent<MapObjectInfo>().currentOccupyingTile;
                    playerUnitReproducing = true;
                }
            }
        }

        reproductionPhaseTip.SetActive(false); // Hide reproduction phase UI tip

        // Make existing player units be able to move in the next turn
        foreach (PlayerUnit p in GameManager.playerUnits)
        {
            p.hasMoved = false;
        }

        playerTurn = true; // Let player play the turn
    }

    /// <summary>
    /// If the player hover the mouse above a tile
    /// </summary>
    /// <param name="hoverTile"></param>
    public void PlayerHoverTile(GridTileInfo hoverTile)
    {
        // Player cannot select tile when it's not player's turn
        if (!playerTurn)
        {
            // If there is a player unit ready to reproduce
            if (playerUnitReproducing)
            {
                // Clear previous map marks
                bool[] clearFlags = { false, true };
                ClearMapMarks(clearFlags);

                // If the hovered tile is valid for place new clone
                if (hoverTile.marks[0].activeInHierarchy)
                {
                    hoverTile.marks[1].SetActive(true); // Highlight hovering tile
                }
            }

            return;
        }

        // If player did not select a unit 
        if (currentSelectedUnitTile == null)
        {
            // Clear previous map marks
            bool[] clearFlags = { true, true };
            ClearMapMarks(clearFlags);

            // If the tile has a player unit
            if (hoverTile.containingObject != null && hoverTile.containingObject.GetComponent<PlayerUnit>())
            {
                hoverTile.marks[0].SetActive(true); // Highlight tile
            }
        }

        // If player selected a unit, and the unit has not moved yet
        if (currentSelectedUnitTile != null && !playerUnitAct)
        {
            // Clear previous map marks
            bool[] clearFlags = { false, true };
            ClearMapMarks(clearFlags);

            // If the tile is within the unit's move range
            if (hoverTile.marks[0].activeInHierarchy)
            {
                ShowPath(currentSelectedUnitTile, hoverTile);
            }
        }

        // If a player unit just moved and is in act phase
        if (playerUnitAct)
        {
            // Clear previous map marks
            bool[] clearFlags = { false, true };
            ClearMapMarks(clearFlags);

            // If the player selected an enemy within range
            if (hoverTile.marks[0].activeInHierarchy && hoverTile.containingObject != null && hoverTile.containingObject.GetComponent<EnemyUnit>())
            {
                // Mark selected enemy
                hoverTile.marks[1].SetActive(true);
            }
        }
    }

    /// <summary>
    /// When player clicked on a tile
    /// </summary>
    /// <param name="selectedTile"></param>
    public void PlayerSelectedTile(GridTileInfo selectedTile)
    {
        // Player cannot select tile during an unit animation
        if (inPlayerUnitAnimation)
        {
            return;
        }

        // If it's currently in evolve phase
        if (GameManager.inEvolvePhase)
        {
            // If player selected a clone unit
            if (selectedTile.containingObject != null)
            {
                // Open evolve shop for the selected unit
                GameManager.sGameManager.StartEvolve(selectedTile.containingObject.GetComponent<PlayerUnit>());
            }

            return;
        }

        // If one of the player's unit is ready to reproduce
        if (playerUnitReproducing)
        {
            // If the player selected a valid tile
            if (selectedTile.marks[0].activeInHierarchy)
            {
                inPlayerUnitAnimation = true; // Start animation

                StartCoroutine(currentSelectedUnitTile.containingObject.GetComponent<PlayerUnit>().UnitReproduce(selectedTile));
            }

            // Stop player from doing other things before finish the reproduction phase
            return;
        }

        // Player cannot select tile when it's not player's turn
        if (!playerTurn)
        {
            return;
        }

        // If there is no unit that has moved but did not finish act yet
        if (!playerUnitAct)
        {
            // If the player selected a tile with a player unit
            if (selectedTile.containingObject != null && selectedTile.containingObject.GetComponent<PlayerUnit>())
            {
                PlayerUnit selectedUnit = selectedTile.containingObject.GetComponent<PlayerUnit>();

                // If the unit has not moved in this turn yet and is not reproducing
                if (!selectedUnit.hasMoved && !selectedUnit.isReproducing)
                {
                    currentSelectedUnitTile = selectedTile;

                    // Show move range of just selected unit
                    ShowRange(selectedTile, selectedUnit.moveRange, true);
                }
            }

            // If the player selected a tile with no object and there is already a selected unit and the selected tile is within move range
            if (currentSelectedUnitTile != null && selectedTile.containingObject == null && selectedTile.marks[1].activeInHierarchy)
            {
                // Clear previous map marks
                bool[] clearFlags = { true, true };
                ClearMapMarks(clearFlags);

                // If their is a valid path to the selected tile
                List<GridTileInfo> path = MapManager.sMapManager.FindPath(currentSelectedUnitTile, selectedTile);

                if (path != null)
                {
                    inPlayerUnitAnimation = true; // Start animation

                    // Start move player unit
                    StartCoroutine(currentSelectedUnitTile.containingObject.GetComponent<PlayerUnit>().UnitMove(path));
                }
            }
        }
        else
        {
            // If the player selected an enemy that's within range
            if (selectedTile.marks[0].activeInHierarchy && selectedTile.containingObject != null && selectedTile.containingObject.GetComponent<EnemyUnit>())
            {
                inPlayerUnitAnimation = true; // Start animation

                StartCoroutine(currentSelectedUnitTile.containingObject.GetComponent<PlayerUnit>().UnitAttack(selectedTile.containingObject.GetComponent<EnemyUnit>())); // Unit attack selected enemy
            }
        }
    }

    /// <summary>
    /// When player selected a unit
    /// </summary>
    /// <param name="selectedUnit"></param>
    //public void PlayerSelectUnit(PlayerUnit selectedUnit)
    //{

    //}

    /// <summary>
    /// When player ended a unit's act
    /// </summary>
    public void UnitFinishAct()
    {
        // Mark the selected unit as moved
        currentSelectedUnitTile.containingObject.GetComponent<PlayerUnit>().hasMoved = true;
        currentSelectedUnitTile = null;

        // Clear previous map marks
        bool[] clearFlags = { true, true };
        ClearMapMarks(clearFlags);

        // Mark the player finished an unit's act, and can select other unit
        playerUnitAct = false;

        // Check if finish turn
        if (!GameManager.playerUnits.Exists(u => !u.hasMoved))
        {
            PlayerFinishTurn();
        }
    }

    /// <summary>
    /// When player finish current turn
    /// </summary>
    public void PlayerFinishTurn()
    {
        // Player cannot finish turn when an unit is still in animation
        if (inPlayerUnitAnimation)
        {
            return;
        }

        playerTurn = false;
        currentSelectedUnitTile = null;


        // Clear map marks
        bool[] clearFlags = { true, true };
        ClearMapMarks(clearFlags);

        // If there is no more enemy left in this level
        if (EnemyManager.thisLevelEnemies.Count == 0)
        {
            GameManager.sGameManager.LevelFinished(); // Finish current level and enters evolve phase
        }
        else
        {
            // Zoom out camera
            if (!GameManager.cameraZoomedOut)
            {
                GameManager.sGameManager.ZoomCamera();
            }

            enemyTurnTip.SetActive(true); // Show enemy turn UI tip
            StartCoroutine(EnemyManager.sEnemyManager.ActEnemies()); // Start enemy turn
        }
    }

    /// <summary>
    /// Show a range for the current action of the current selected player unit
    /// Can show move range, attack range, etc.
    /// </summary>
    /// <param name="selectedTile"></param>
    /// <param name="range"></param>
    /// <param name="isEmpty"></param>
    /// <returns></returns>
    public int ShowRange(GridTileInfo selectedTile, int range, bool isEmpty)
    {
        // Clear previous map marks
        bool[] clearFlags = { true, true };
        ClearMapMarks(clearFlags);

        int availableTileCount = 0;

        // Show range
        foreach (GridTileInfo tile in MapManager.sMapManager.currentMap)
        {
            // If tile is not obstacle and empty (if showed tile has to be empty)
            if (!tile.isObstacle && (tile.containingObject == null || !isEmpty) && MapManager.sMapManager.GetManhattenDistance(selectedTile, tile) <= range)
            {
                tile.marks[0].SetActive(true);

                // Count available tile
                availableTileCount++;
            }
        }

        return availableTileCount;
    }

    /// <summary>
    /// Show the path from the selected player clone tile to the player selected destination tile
    /// </summary>
    /// <param name="selectedTile"></param>
    /// <param name="destinationTile"></param>
    public void ShowPath(GridTileInfo selectedTile, GridTileInfo destinationTile)
    {
        // Clear previous map marks
        bool[] clearFlags = { false, true };
        ClearMapMarks(clearFlags);

        // Show route
        List<GridTileInfo> route = MapManager.sMapManager.FindPath(selectedTile, destinationTile);

        if (route == null || route.Count > currentSelectedUnitTile.containingObject.GetComponent<PlayerUnit>().moveRange)
        {
            // No valid route
        }
        else
        {
            foreach (GridTileInfo tile in MapManager.sMapManager.FindPath(selectedTile, destinationTile))
            {
                tile.marks[1].SetActive(true);
            }
        }
    }

    /// <summary>
    /// Clear the UI marks on the map
    /// </summary>
    /// <param name="clearFlags"></param>
    public void ClearMapMarks(bool[] clearFlags)
    {
        foreach (GridTileInfo g in MapManager.sMapManager.currentMap)
        {
            for (int i = 0; i < clearFlags.Length; i++)
            {
                g.marks[i].SetActive(g.marks[i].activeInHierarchy && !clearFlags[i]);
            }
        }
    }
}
