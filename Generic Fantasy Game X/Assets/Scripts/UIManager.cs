using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
   

    //PauseMenu
    private Canvas canvas;
    private GameObject[] heroes;
    private GameObject[] enemies;

    //[SerializeField]
    public GameObject pauseMenu;

	public GameObject scoreScreen;
	public DataCollector dataCollector;
    public GameObject inventoryUI;
    
	public bool pauseMenuStatus;
	private bool scoreScreenStatus;
    public bool inventoryStatus;

    private int level = 1;
    //public Text gameOverText;
    //public GameObject gameOverImage;
    public static UIManager instance = null;

    //Audio for buttons
    public AudioSource audio;

    void Awake()
    {

		//scoreScreen = GameObject.FindGameObjectWithTag ("ScoreScreen");
		//pauseMenu = GameObject.FindGameObjectWithTag ("PauseMenu");
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;


        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
          //  Destroy(gameObject);  - Causes Bug where restarting disables pause.

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        //LoadCanvas
        pauseMenu.SetActive(false);
        pauseMenuStatus = false;

		scoreScreen.SetActive (false);
		scoreScreenStatus = false;

        inventoryUI.SetActive(false);
        inventoryStatus = false;
    }

    //TO DO: Replace Slot1 with "Current Player"
    void Start()
	{
      

		//Calling the variables from Player Stats
       
        heroes = GameObject.FindGameObjectsWithTag("Hero");
        enemies = GameObject.FindGameObjectsWithTag("Enemy");




	}

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenu();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            InventorySwitch();
        }

    }


    public void PauseMenu()
    {
        if (!pauseMenuStatus)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1;
        }

        pauseMenuStatus = !pauseMenuStatus;
        
        foreach (GameObject hero in heroes)
        {
            hero.GetComponent<EntityNavigationScript>().enabled = 
                !hero.GetComponent<EntityNavigationScript>().enabled;
        }
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<EntityNavigationScript>().enabled =
                !enemy.GetComponent<EntityNavigationScript>().enabled;
        }
    }

	public void ScoreScreen(bool switcher)
	{
		if (switcher)
		{
			//dataCollector.StopTimer ();
			scoreScreen.SetActive(true);
			//Time.timeScale = 0;
		}
		else
		{
			scoreScreen.SetActive(false);
			//Time.timeScale = 1;
		}


	}

    public void InventorySwitch()
    {
        if (!inventoryStatus)
        {
            inventoryUI.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            inventoryUI.SetActive(false);
            Time.timeScale = 1;
        }

       inventoryStatus = !inventoryStatus;

        foreach (GameObject hero in heroes)
        {
            hero.GetComponent<EntityNavigationScript>().enabled =
                !hero.GetComponent<EntityNavigationScript>().enabled;
        }
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<EntityNavigationScript>().enabled =
                !enemy.GetComponent<EntityNavigationScript>().enabled;
        }
    }

}
