using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    //Health UI
	private Slider healthBar;
	private Text HPText;
	private PlayerManager playerManager;
	private GameObject slot1;
    private int MaxHP;
    private int CurrentHP;

    //PauseMenu
    private Canvas canvas;
    private GameObject[] heroes;
    private GameObject[] enemies;

    [SerializeField]
    private GameObject pauseMenu;

    private bool pauseMenuStatus;

    private int level = 1;
    //public Text gameOverText;
    //public GameObject gameOverImage;
    public static UIManager instance = null;

    //Audio for buttons
    public AudioSource audio;

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
        pauseMenuStatus = false;
    }

    //TO DO: Replace Slot1 with "Current Player"
    void Start()
	{
        playerManager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<PlayerManager>();
        canvas = GameObject.FindGameObjectWithTag("GameUI").GetComponent<Canvas>();

        //Calling the variables from Player Stats
        slot1 = GameObject.FindGameObjectWithTag("Slot1");
        healthBar = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<Slider>();
        HPText = healthBar.GetComponentInChildren<Text>();

		MaxHP = playerManager.maxHP;
		healthBar.maxValue += MaxHP;//getMaxHP(); //playerStat.playerMaxHealth;

        heroes = GameObject.FindGameObjectsWithTag("Hero");
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
	}

    void Update()
    {
        CurrentHP = playerManager.currentHP;


        healthBar.value = CurrentHP;  //playerStat.playerCurrentHealth;

        HPText.text = "HP " + CurrentHP + "/" + MaxHP;
        Debug.Log("UI MANAGER: " + CurrentHP + "/" + MaxHP);

        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenu();
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
}
