using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores info and some functions for a grid tile on a map
/// 
/// Marks: range mark, route mark
/// </summary>
public class GridTileInfo : MonoBehaviour
{
    public GameObject fogOfWar; // The GameObject used to display fog of war
    public GameObject[] marks; // The GameObjects used to mark dynamic informations on the map such as move range


    public GameObject containingObject; // The object that is currently contained in this tile
    public bool isVisible; // Is this tile visible to the player
    public int xCoord;
    public int zCoord;
    public bool isObstacle; // Is this tile an obstacle and cannot have anything on it or pass through it

    // This is only used for path finding
    public GridTileInfo parentTile;
    public int igCost;
    public int ihCost;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public enum TileType
    //{
    //    Obstacle,
    //    Empty
    //}
}
