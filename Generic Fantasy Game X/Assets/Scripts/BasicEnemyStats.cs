using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BasicEnemyStats : MonoBehaviour {

public float maxHP;
public float currentHP;
public bool isDead = false;
public int attackDamage;
public float attackCooldown;
public float attackCooldownTime;
public GameObject enemy;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if(currentHP <= 0){
			
			EnemyDeath();
			//Destroy(this);

		}

	}

	public void RecieveDmg(float dmg){

		currentHP -= dmg;

	}

	void EnemyDeath(){

		//TODO needs to have death animation and have some kind of exp gain/loot drop
		if(!isDead){
			Debug.Log("Enemy is at 0HP");
			isDead = true;
		}

	}
}
