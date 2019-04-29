using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public float waitTimeAfterMove; // How long the enemy should wait after this move is finished

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// UnityEvent Helper for start the move coroutine
    /// </summary>
    public void StartMove()
    {
        StartCoroutine(Move());
    }

    virtual public IEnumerator Move()
    {


        // Only play animation if the enemy is visible
        if (GetComponent<EnemyUnit>().isVisible)
        {
            // Wait some time after the move is finished
            yield return new WaitForSeconds(waitTimeAfterMove);
        }

        GetComponent<EnemyUnit>().isInSomeMove = false;
    }
}
