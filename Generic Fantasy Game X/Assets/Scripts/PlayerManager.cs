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
    public float currentAttack;
	//public int[] currentDefence;

	
	public GameObject currentPlayer;
	public GameObject target;

	public float autoAttackCooldown;
	public float autoAttackCurrentTime;

	RaycastHit hit;
    Ray ray;

    //will need to fix this
    public bool canAttack = true;
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

		if(target != null && canAttack){

			if(autoAttackCurrentTime > autoAttackCooldown){

				autoAttackCurrentTime += Time.deltaTime;
			
			}
			else
			{
				BasicAttack();
			}

		}

		//Right Clicking will now set an enemy as the target for the player object (will want to change the target button when the movement is cleaned up a little)
		if(Input.GetKeyDown(KeyCode.Tab)){
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray, out hit)){
				if(hit.collider.tag == "Enemy"){
					Debug.Log(hit.collider.gameObject);
					target = hit.collider.gameObject;
				}
			}
		}
		//TODO add a range check for the AA
		//if()		

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

	void BasicAttack(){

		autoAttackCurrentTime = 0;
		target.GetComponent<BasicEnemyStats>().RecieveDmg(currentAttack);

	}


}
