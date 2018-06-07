using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PowerGridInventory;

public class UIManager : MonoBehaviour {


	//UI Slots
	[SerializeField]
	private GameObject[] slots;

    //PauseMenu
    private Canvas canvas;
    private GameObject[] heroes;
    private GameObject[] enemies;

    [SerializeField]
    private GameObject pauseMenu;
	[SerializeField]
	private GameObject scoreScreen;

    public GameObject statusScreen;

    //Switch Buttons Off
    public GameObject pauseBtn;
    public GameObject statusBtn;


	private bool pauseMenuStatus;
	public bool scoreScreenStatus { get; private set; }
    private bool statusScreenStatus;


    private int level = 1;
    //public Text gameOverText;
    //public GameObject gameOverImage;
    public static UIManager instance = null;

    //Audio for buttons
    public AudioSource audio;
	public AudioSource BGM;

    void Awake()
    {
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

		// Establish UI Slot assignage for each hero

		heroes = GameObject.FindGameObjectsWithTag ("Hero");

		int slotCount = 0;
		int heroCount = 0;
		while (heroCount < heroes.Length && slotCount < slots.Length) {
			heroes [heroCount].GetComponent<EntityStatisticsScript> ().staticHealthText = slots [slotCount].GetComponent<UISlotScript> ().HPText;
			heroes [heroCount].GetComponent<EntityStatisticsScript> ().staticHealthUI = slots [slotCount].GetComponent<UISlotScript> ().HPBar;
			heroes [heroCount].GetComponent<EntityStatisticsScript> ().staticXPText = slots [slotCount].GetComponent<UISlotScript> ().XPText;
			heroes [heroCount].GetComponent<EntityStatisticsScript> ().staticXPUI = slots [slotCount].GetComponent<UISlotScript> ().XPBar;
			heroes [heroCount].GetComponent<EntityStatisticsScript> ().currentLevelText = slots [slotCount].GetComponent<UISlotScript> ().LevelText;
			slots [slotCount].GetComponent<UISlotScript> ().inventoryView.GetComponent<PGIView> ().Model = heroes [heroCount].GetComponent<EntityStatisticsScript> ().inventory;

			slots [slotCount].SetActive (true);

			heroCount++;
			slotCount++;
		}

        //LoadCanvas
        pauseMenu.SetActive(false);
        pauseMenuStatus = false;

		scoreScreen.SetActive (false);
		scoreScreenStatus = false;

        statusScreen.SetActive(false);
        statusScreenStatus = false;

        BGM.Play ();
    }

    //TO DO: Replace Slot1 with "Current Player"
    void Start()
	{
        canvas = GameObject.FindGameObjectWithTag("GameUI").GetComponent<Canvas>();

        //Calling the variables from Player Stats

        enemies = GameObject.FindGameObjectsWithTag("Enemy");
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
			audio.Play ();
            PauseMenu();
        }

		if (scoreScreenStatus) {
			if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
			{
				ScoreScreen();
			}
		}
    }


    public void PauseMenu()
	{
		audio.Play ();
        if (!pauseMenuStatus)
        {
            pauseBtn.SetActive(false);
            statusBtn.SetActive(false);
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            pauseBtn.SetActive(true);
            statusBtn.SetActive(true);
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

	public void ScoreScreen()
	{
		if (!scoreScreenStatus)
		{
			scoreScreen.SetActive(true);
			Time.timeScale = 0;
		}
		else
		{
			scoreScreen.SetActive(false);
			Time.timeScale = 1;
		}

		scoreScreenStatus = !scoreScreenStatus;
	}

    public void StatusScreen()
    {
        
        audio.Play();
        if (!statusScreenStatus)
        {
            pauseBtn.SetActive(false);
            statusBtn.SetActive(false);
            statusScreen.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            pauseBtn.SetActive(true);
            statusBtn.SetActive(true);
            statusScreen.SetActive(false);

            Time.timeScale = 1;
        }

        statusScreenStatus = !statusScreenStatus;

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
