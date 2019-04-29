using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 
/// 
/// The path finding algorithm is from: https://github.com/danielmccluskey/A-Star-Pathfinding-Tutorial/blob/master/Assets/Pathfinding.cs
/// All credit goes to Daniel McCluskey
/// </summary>
public class MapManager : MonoBehaviour
{
    public int mapSizeX;
    public int mapSizeZ;
    public GameObject tilePrefab;

    public GridTileInfo[,] currentMap; // The current map
    public List<GridTileInfo> borderTiles; // The border tiles

    public static MapManager sMapManager;

    private void OnEnable()
    {
        sMapManager = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

#if UNITY_EDITOR
    [MenuItem("LD44/CreateNewMap")]
    public static void CreateMap()
    {
        MapManager instance = (MapManager)FindObjectOfType(typeof(MapManager));

        instance.currentMap = new GridTileInfo[instance.mapSizeX, instance.mapSizeZ];

        GameObject newMapObject = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity);
        newMapObject.name = "Grid";

        for (int i = 0; i < instance.mapSizeX; i++)
        {
            GameObject newColumnObject = Instantiate(new GameObject(), newMapObject.transform);
            newColumnObject.name = "Column_" + i.ToString();

            for (int j = 0; j < instance.mapSizeZ; j++)
            {
                GameObject newTile = Instantiate(instance.tilePrefab, Vector3.right * i + Vector3.forward * j, instance.tilePrefab.transform.rotation);
                newTile.transform.parent = newColumnObject.transform;
                GridTileInfo newTileInfo = newTile.GetComponent<GridTileInfo>();
                newTileInfo.xCoord = i;
                newTileInfo.zCoord = j;
                instance.currentMap[i, j] = newTileInfo;
            }
        }
    }

    [MenuItem("LD44/GetMapInfo")]
    public static void GetMapInfo()
    {
        MapManager instance = (MapManager)FindObjectOfType(typeof(MapManager));

        print("Map size: " + instance.currentMap.GetLength(0).ToString() + ", " + instance.currentMap.GetLength(1).ToString());

        for (int i = 0; i < instance.mapSizeX; i++)
        {
            for (int j = 0; j < instance.mapSizeZ; j++)
            {
                print(i.ToString() + ", " + j.ToString() + ": " + instance.currentMap[i, j].transform.position);
            }
        }
    }
#endif

    /// <summary>
    /// Place an object on a grid tile
    /// </summary>
    /// <param name="objectTrans"></param>
    /// <param name="xCoord"></param>
    /// <param name="zCoord"></param>
    public static void PlaceObject(Transform objectTrans, int xCoord, int zCoord)
    {
        MapObjectInfo movedObject = objectTrans.GetComponent<MapObjectInfo>();

        if (movedObject.currentOccupyingTile != null)
        {
            movedObject.currentOccupyingTile.containingObject = null; // Erase it from the previous tile
        }

        sMapManager.currentMap[xCoord, zCoord].containingObject = objectTrans.gameObject; // Assign the object to tile
        movedObject.currentOccupyingTile = sMapManager.currentMap[xCoord, zCoord]; // Update the tile this moved object is occuping
        objectTrans.position = sMapManager.currentMap[xCoord, zCoord].transform.position + Vector3.up * 0.5f; // Move the object
    }

    /// <summary>
    /// Return currently empty border tiles
    /// </summary>
    /// <returns></returns>
    public List<GridTileInfo> GetEmptyBorderTiles()
    {
        List<GridTileInfo> emptyBorders = new List<GridTileInfo>();

        foreach (GridTileInfo tile in borderTiles)
        {
            // If tile is empty
            if (tile.containingObject == null)
            {
                emptyBorders.Add(tile);
            }
        }

        return emptyBorders;
    }

    /// <summary>
    /// Find all the border tiles
    /// </summary>
    public void GetBorderTiles()
    {
        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeZ; j++)
            {
                // If is border tile
                if (i == 0 || i == mapSizeX - 1 || j == 0 || j == mapSizeZ - 1)
                {
                    borderTiles.Add(currentMap[i, j]);
                }
            }
        }
    }

    /// <summary>
    /// Return a list of GridTileInfo that represents the shortest path from start tile to end tile
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public List<GridTileInfo> FindPath(GridTileInfo start, GridTileInfo end)
    {
        List<GridTileInfo> OpenList = new List<GridTileInfo>();//List of nodes for the open list
        HashSet<GridTileInfo> ClosedList = new HashSet<GridTileInfo>();//Hashset of nodes for the closed list

        OpenList.Add(start);//Add the starting node to the open list to begin the program

        while (OpenList.Count > 0)//Whilst there is something in the open list
        {
            GridTileInfo CurrentNode = OpenList[0];//Create a node and set it to the first item in the open list
            for (int i = 1; i < OpenList.Count; i++)//Loop through the open list starting from the second object
            {
                if (OpenList[i].ihCost + OpenList[i].igCost < CurrentNode.ihCost + CurrentNode.igCost ||
                    OpenList[i].ihCost + OpenList[i].igCost == CurrentNode.ihCost + CurrentNode.igCost && OpenList[i].ihCost < CurrentNode.ihCost)//If the f cost of that object is less than or equal to the f cost of the current node
                {
                    CurrentNode = OpenList[i];//Set the current node to that object
                }
            }
            OpenList.Remove(CurrentNode);//Remove that from the open list
            ClosedList.Add(CurrentNode);//And add it to the closed list

            if (CurrentNode == end)//If the current node is the same as the target node
            {
                return GetFinalPath(start, end);//Calculate the final path
            }

            foreach (GridTileInfo NeighborNode in GetNeighboringNodes(CurrentNode))//Loop through each neighbor of the current node
            {
                if (NeighborNode.isObstacle || NeighborNode.containingObject != null || ClosedList.Contains(NeighborNode))//If the neighbor is a wall or not empty or has already been checked
                {
                    continue;//Skip it
                }
                int MoveCost = CurrentNode.igCost + GetManhattenDistance(CurrentNode, NeighborNode);//Get the F cost of that neighbor

                if (MoveCost < NeighborNode.igCost || !OpenList.Contains(NeighborNode))//If the f cost is greater than the g cost or it is not in the open list
                {
                    NeighborNode.igCost = MoveCost;//Set the g cost to the f cost
                    NeighborNode.ihCost = GetManhattenDistance(NeighborNode, end);//Set the h cost
                    NeighborNode.parentTile = CurrentNode;//Set the parent of the node for retracing steps

                    if (!OpenList.Contains(NeighborNode))//If the neighbor is not in the openlist
                    {
                        OpenList.Add(NeighborNode);//Add it to the list
                    }
                }
            }

        }

        // Return null if cannot find path
        return null;
    }


    public List<GridTileInfo> GetFinalPath(GridTileInfo start, GridTileInfo end)
    {
        List<GridTileInfo> finalPath = new List<GridTileInfo>();//List to hold the path sequentially 
        GridTileInfo currentTile = end;//Node to store the current node being checked

        while (currentTile != start)//While loop to work through each node going through the parents to the beginning of the path
        {
            finalPath.Add(currentTile);//Add that node to the final path
            currentTile = currentTile.parentTile;//Move onto its parent node
        }

        finalPath.Reverse();//Reverse the path to get the correct order

        return finalPath; //Return the final path
    }

    /// <summary>
    /// Get manhatten distance from start tile to end tile
    /// </summary>
    /// <param name="a_nodeA"></param>
    /// <param name="a_nodeB"></param>
    /// <returns></returns>
    public int GetManhattenDistance(GridTileInfo a_nodeA, GridTileInfo a_nodeB)
    {
        int ix = Mathf.Abs(a_nodeA.xCoord - a_nodeB.xCoord);//x1 - x2
        int iy = Mathf.Abs(a_nodeA.zCoord - a_nodeB.zCoord);//z1 - z2

        return ix + iy;//Return the sum
    }

    /// <summary>
    /// Return a list of neighbor tiles of a tile
    /// </summary>
    /// <param name="checkedTile"></param>
    /// <returns></returns>
    public static List<GridTileInfo> GetNeighboringNodes(GridTileInfo checkedTile)
    {
        List<GridTileInfo> NeighborList = new List<GridTileInfo>();//Make a new list of all available neighbors.
        int icheckX;//Variable to check if the XPosition is within range of the node array to avoid out of range errors.
        int icheckZ;//Variable to check if the ZPosition is within range of the node array to avoid out of range errors.

        //Check the right side of the current node.
        icheckX = checkedTile.xCoord + 1;
        icheckZ = checkedTile.zCoord;
        if (icheckX >= 0 && icheckX < sMapManager.mapSizeX)//If the XPosition is in range of the array
        {
            if (icheckZ >= 0 && icheckZ < sMapManager.mapSizeZ)//If the YPosition is in range of the array
            {
                NeighborList.Add(sMapManager.currentMap[icheckX, icheckZ]);//Add the grid to the available neighbors list
            }
        }
        //Check the Left side of the current node.
        icheckX = checkedTile.xCoord - 1;
        icheckZ = checkedTile.zCoord;
        if (icheckX >= 0 && icheckX < sMapManager.mapSizeX)//If the XPosition is in range of the array
        {
            if (icheckZ >= 0 && icheckZ < sMapManager.mapSizeZ)//If the YPosition is in range of the array
            {
                NeighborList.Add(sMapManager.currentMap[icheckX, icheckZ]);//Add the grid to the available neighbors list
            }
        }
        //Check the Top side of the current node.
        icheckX = checkedTile.xCoord;
        icheckZ = checkedTile.zCoord + 1;
        if (icheckX >= 0 && icheckX < sMapManager.mapSizeX)//If the XPosition is in range of the array
        {
            if (icheckZ >= 0 && icheckZ < sMapManager.mapSizeZ)//If the YPosition is in range of the array
            {
                NeighborList.Add(sMapManager.currentMap[icheckX, icheckZ]);//Add the grid to the available neighbors list
            }
        }
        //Check the Bottom side of the current node.
        icheckX = checkedTile.xCoord;
        icheckZ = checkedTile.zCoord - 1;
        if (icheckX >= 0 && icheckX < sMapManager.mapSizeX)//If the XPosition is in range of the array
        {
            if (icheckZ >= 0 && icheckZ < sMapManager.mapSizeZ)//If the YPosition is in range of the array
            {
                NeighborList.Add(sMapManager.currentMap[icheckX, icheckZ]);//Add the grid to the available neighbors list
            }
        }

        return NeighborList;//Return the neighbors list.
    }
}
