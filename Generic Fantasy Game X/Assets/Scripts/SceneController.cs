using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {

    private LevelGenerationScript restart;
    private Scene currentScene;
	// Use this for initialization
	void Start () {
        currentScene = SceneManager.GetActiveScene();
    }
	
	// Update is called once per frame
	void Update () {
        currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "UI - Test Scene")
        {
            restart = GameObject.Find("LevelGenerator").GetComponent<LevelGenerationScript>();
        }
	}

    public void StartGame()
    {
        //For Future
        //SceneManager.LoadScene("Main Scene");

        //For Testing
        if (restart != null)
        {
            restart.Restart();
        }
        SceneManager.LoadScene("UI - Test Scene");
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("StartMenu");
    }

    public void CloseApp()
    {
        Application.Quit();
    }
}
