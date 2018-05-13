using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void StartGame()
    {
        //For Future
        //SceneManager.LoadScene("Main Scene");

        //For Testing
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
