﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



//	[System.Serializable]
public class GameManager: MonoBehaviour {

	[SerializeField]
	private GameObject pauseMenu;
	[SerializeField]
	private GameObject mainMenu;

    private bool pauseMenuStatus;
    private int level = 1;
    //public Text gameOverText;
    //public GameObject gameOverImage;
    public static GameManager instance = null;

    void Awake()
    {
        //Check if instance already exists
        if (instance == null)
        
            //if not, set instance to this
            instance = this;
        

        //If instance already exists and it's not this:
        else if (instance != this)
        
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
        
        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

		//LoadCanvas
		pauseMenu.SetActive(false);
		mainMenu.SetActive(false);
     }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log(pauseMenuStatus);
            PauseMenu();
        }
    }


    void OnLevelWasLoaded(int index)
    {
        level++;
       

    }

    public void startGame(int index)
    {
        SceneManager.LoadScene(index);
    }

    /*public void GameOver()
    {
        //set Text for game over
        gameOverText.text = "All players have died, Game Over";

        //Set image for game over screen
        gameOverImage.SetActive(true);


        enabled = false;

        SceneManager.LoadScene(0);
    }*/

    void PauseMenu()
    {
        if (!pauseMenuStatus)
        {
            pauseMenuStatus = !pauseMenuStatus;
            pauseMenu.SetActive(true);
        }
        else ExitMenu();
    }

	public void ExitMenu()
	{
        pauseMenuStatus = !pauseMenuStatus;
        pauseMenu.SetActive(false);
		Debug.Log("Close Menu");
	}


    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
	}


