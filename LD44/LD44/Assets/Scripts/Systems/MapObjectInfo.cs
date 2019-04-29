using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains info of an object that's on the game map
/// </summary>
public class MapObjectInfo : MonoBehaviour
{


    public ObjectType objectType; // What is this object
    public GridTileInfo currentOccupyingTile; // The map tile that is currently occupied by this object

    private void OnEnable()
    {
        // Initialize object type
        objectType = ObjectType.invalid;

        if (GetComponent<EnemyUnit>())
        {
            objectType = ObjectType.enemy;
        }

        if (GetComponent<PlayerUnit>())
        {
            objectType = ObjectType.player;
        }


    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public enum ObjectType
    {
        invalid,
        player,
        enemy
    }
}
