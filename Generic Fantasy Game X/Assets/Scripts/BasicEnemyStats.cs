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
	public float experienceDrop = 30.0f;
	public List<GameObject> lootList;
	public GameObject enemy;

	// private variables
	private float timeElapsed;
	private Rigidbody rb;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		timeElapsed = 0.0f; // Used for the 'death' animation
	}
	
	// Update is called once per frame
//	void Update () {
//		
//		if(currentHP <= 0){
//			
//			EnemyDeath();
//			//Destroy(this);
//
//		}
//
//	}

	void Update() {

		if (Input.GetKeyDown (KeyCode.Z)) {
			EnemyDeath (Vector3.zero);
		}

		if (isDead && timeElapsed < 5.0f) {
			if (timeElapsed < 0.3f) {
				gameObject.transform.RotateAround (gameObject.transform.position - new Vector3 (0.0f, 0.5f, 0.0f), gameObject.transform.right, 0.5f);
			}

			timeElapsed += Time.deltaTime;
		}

	}

	public void RecieveDmg(float dmg, Transform attacker){
		if (currentHP >= 0)
			currentHP -= dmg;
		if (currentHP <= 0 ){
			EnemyDeath(attacker.forward);
			//Destroy(this);
		}
	}

	void EnemyDeath(Vector3 attack){

		//TODO needs to have death animation and have some kind of exp gain/loot drop
		if(!isDead){
			Debug.Log("Enemy is at 0HP");
			isDead = true;
			GetComponent<EntityNavigationScript> ().SetState (false);
			rb.isKinematic = false;
			rb.useGravity = true;
			//rb.AddForce (1,6,0, ForceMode.Impulse);
			//GetComponent<EntityNavigationScript> ().SetObstacle ();
			//this.enabled = false;
			gameObject.layer = 10;
		}

	}
}
