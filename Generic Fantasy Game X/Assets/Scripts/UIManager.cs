using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    //Health UI
	public Slider healthBar;
	public Text HPText;
	public PlayerManager playerManager;
	public GameObject slot1;
    public int MaxHP;
    public int CurrentHP;

    //PauseMenu
    public Transform canvas;
    public Transform Player;

	//TO DO: Replace Slot1 with "Current Player"
	void Start()
	{
        playerManager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<PlayerManager>();

        //Calling the variables from Player Stats
        slot1 = GameObject.FindGameObjectWithTag("Slot1");
        healthBar = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<Slider>();
       
	  
	}

    void Update()
    {
        MaxHP = playerManager.maxHP;
        CurrentHP = playerManager.currentHP;

        healthBar.maxValue += MaxHP;//getMaxHP(); //playerStat.playerMaxHealth;
        healthBar.value = CurrentHP;  //playerStat.playerCurrentHealth;

        HPText.text = "HP " + CurrentHP + "/" + MaxHP;

        Debug.Log("UI MANAGER: " + CurrentHP + "/" + MaxHP);

        if (Input.GetKeyDown(KeyCode.P))
        {
            openMenu();
        }

    }
    public void openMenu()
    {
        
            if(canvas.gameObject.activeInHierarchy == false)
            {
                canvas.gameObject.SetActive(true);
                Time.timeScale = 0;
                Player.GetComponent<EntityNavigationScript>().enabled = false;

            }
         
        

    }

    public void closeMenu()
    {
        if (canvas.gameObject.activeInHierarchy == true)
        {
            canvas.gameObject.SetActive(false);
            Time.timeScale = 1;
            Player.GetComponent<EntityNavigationScript>().enabled = true;

        }
    }
}
