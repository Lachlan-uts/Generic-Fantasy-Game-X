using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class SceneController : MonoBehaviour {

    private LevelGenerationScript restart;
    private Scene currentScene;

	// Session Data Collection Setup
	private static bool hasWritten = false;
	private static string filePath = "Assets/AcquiredData/SessionData.txt";

	[SerializeField]
	private string gameScene = "UI - Inventory"; //This is where we set the game scene from
//	private string mainScene = "UI - Test Scene"; //old value

	// Use this for initialization
	void Start () {
		// Record date and time of the test session
		if (!hasWritten) {
			hasWritten = true;

			StreamWriter writer = new StreamWriter (filePath, true);
			writer.WriteLine("Date/Time of Play session: " 
				+ System.DateTime.Now.Date.Day + "/" 
				+ System.DateTime.Now.Date.Month + "/" 
				+ System.DateTime.Now.Date.Year + " "
				+ System.DateTime.Now.TimeOfDay.Hours + ":" 
				+ System.DateTime.Now.TimeOfDay.Minutes + ":"
				+ System.DateTime.Now.TimeOfDay.Seconds + ".");
			writer.Close ();
		}

		currentScene = SceneManager.GetActiveScene();
		if (currentScene.name == gameScene) {
			restart = GameObject.Find("LevelGenerator").GetComponent<LevelGenerationScript>();
			StartCoroutine (HeroCheck ());
		}

		// All done with setup
    }
	
	// Update is called once per frame
	void Update () {
        currentScene = SceneManager.GetActiveScene();
		if (currentScene.name == gameScene)
        {
            restart = GameObject.Find("LevelGenerator").GetComponent<LevelGenerationScript>();
        }
	}

	private IEnumerator HeroCheck() {
		yield return new WaitUntil (() => LevelGenerationScript.entityLists ["Hero"].Count <= 0);
		Invoke ("GameOver", 3.0f);
		yield return null;

	}

	private void GameOver() {
		SceneManager.LoadScene("GameOver");
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
		SceneManager.LoadScene(gameScene);
    }

	public void ViewInstructions() {
		SceneManager.LoadScene ("Instructions");
	}

    public void ExitGame()
    {
        SceneManager.LoadScene("StartMenu");
    }

    public void CloseApp()
    {
        Application.Quit();
    }


    //GameOverTrigger

    public void GameOverTrigger()
    {
        if(gameObject.name == "Hero")
        SceneManager.LoadScene("GameOver");
    }

	public void LoadNextLevel()
	{
		SceneManager.LoadScene(gameScene);
		//When Combining
		//SceneManager.LoadScene("Main Scene");
	}
}
