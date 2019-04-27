using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Info and maybe some functions for player unit
/// </summary>
public class PlayerUnit : MonoBehaviour
{


    public bool hasMoved; // Has this unit moved in this turn
    public int moveRange; // How far can this unit move each turn (diagnol is considered 2 distance)
    public int attackRange; // How far can this unit attack
    public float attackPower; // How much is this unit's attack power
    public float health; // How much health this unit left

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
