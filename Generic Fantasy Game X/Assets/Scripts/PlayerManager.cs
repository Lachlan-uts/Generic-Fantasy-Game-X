using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    
    public bool isActive;


    public int currentLevel;
	public int currentExp;
	public int[] toLevelUp;
	public int[] HPUp;
	public int[] attackUp;
	//public int[] defenceUp;

	public int currentHP;
    public int maxHP;
    public int currentAttack;
	//public int[] currentDefence;

	
	public GameObject currentPlayer;

	//public GameObject slot1;
	//public GameObject slot2;
	//public GameObject slot3;
	//public GameObject slot4;

	// Use this for initialization
	void Start () {
       
        currentHP = HPUp [0];
		currentAttack = attackUp[0];
		currentLevel = 1;
		currentExp = 0;
		currentPlayer = GameObject.FindGameObjectWithTag ("Hero");
        maxHP = HPUp[currentLevel-1];

        Debug.Log ("Player Manager: " + currentHP + currentAttack + currentLevel + currentExp);
	}
	
	// Update is called once per frame
	void Update () {


		if(currentExp >= toLevelUp[currentLevel])
		{
			LevelUp ();

		}

	}

	public void LevelUp()
	{
		currentLevel++;
		currentHP = HPUp [currentLevel];
		currentAttack = attackUp [currentLevel];
		//currentDefence = defenceUp[currentLevel];
		//playerHealth.playerMaxHealth = currentHP;

		
		currentHP += currentHP - HPUp [currentLevel - 1];
		//playerInfo.

	}

  




}
