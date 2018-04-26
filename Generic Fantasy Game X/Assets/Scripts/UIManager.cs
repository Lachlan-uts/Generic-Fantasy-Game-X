using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	public Slider healthBar;
	public Text HPText;
	public PlayerManager playerManager;
	public GameObject slot1;


	//TO DO: Replace Slot1 with "Current Player"
	void Start()
	{
        playerManager = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<PlayerManager>();

        //Calling the variables from Player Stats
        int MaxHP = playerManager.maxHP;
       int CurrentHP = playerManager.currentHP;
       healthBar = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<Slider>();
       healthBar.maxValue = MaxHP;//getMaxHP(); //playerStat.playerMaxHealth;
       healthBar.value = CurrentHP;  //playerStat.playerCurrentHealth;
	   HPText.text = "HP " + CurrentHP + "/" + MaxHP;

      Debug.Log("UI MANAGER: " + CurrentHP + "/" + MaxHP);
	}


}
