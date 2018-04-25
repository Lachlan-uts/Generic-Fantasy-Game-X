using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

	public int currentLevel;
	public int currentExp;
	public int[] toLevelUp;
	public int[] HPUp;
	public int[] attackUp;
	//public int[] defenceUp;

	public int currentHP;
	public int currentAttack;
	//public int[] currentDefence;

	private  PlayerInformation playerInfo;
	public GameObject currentPlayer;

	// Use this for initialization
	void Start () {
		currentHP = HPUp [1];
		currentAttack = attackUp[1];
		currentLevel = 1;
		currentExp = 0;
		currentPlayer = GameObject.FindGameObjectWithTag ("Player");
		playerInfo = currentPlayer.GetComponent<PlayerInformation> ();

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

		playerInfo.MaxHP = currentHP;
		playerInfo.CurrentHP += currentHP - HPUp [currentLevel - 1];
		//playerInfo.

	}


}
