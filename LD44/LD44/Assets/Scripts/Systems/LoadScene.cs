using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Used to let player start new game
    /// </summary>
    public void ReloadSceneForNewGame()
    {
        SceneManager.LoadScene(1);
    }
}
