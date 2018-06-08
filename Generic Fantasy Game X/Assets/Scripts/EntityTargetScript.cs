﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;


public class EntityTargetScript : MonoBehaviour {

	//various values that need tracking
	private float targetProximity = Mathf.Infinity;

	//settable, important fields
	[SerializeField]
	private List<string> targetableTags = new List<string> () {"Hero", "Enemy"}; //<--- Make sure this always has atleast 2 elements or bad times will occour

    SceneController sceneController;

//	private string[] targetableTags = new string[] {"Hero", "Enemy"};

	/*
	 * Fancy corountine better entity tracking stuff
	 */
	[SerializeField]
	private GameObject targetedEntity_;
	public GameObject targetedEntity {
		get {
			return targetedEntity_;
		}
		set {
			targetedEntity_ = value;
			StartCoroutine (CheckTargetEntity ());
		}
	}
	//The move system for this entity
	private EntityNavigationScript entityNavigationScript;

	//The sword
	private WeaponScript weaponScript;

	//to trigger the attack
	private Animator anim;

	// Use this for initialization
	void Start () {

        sceneController = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<SceneController>();
        Time.timeScale = 1;
        weaponScript = GetComponentInChildren<WeaponScript> ();

		//remove own tag from list of targetable tags
		targetableTags.Remove(this.gameObject.tag);

		entityNavigationScript = GetComponent<EntityNavigationScript> ();
		anim = GetComponent<Animator> ();
		targetedEntity = null;
		StartCoroutine ("WatchForTarget");

	}

	private IEnumerator FindNearbyEnemy() {
		if (LevelGenerationScript.entityLists [targetableTags [0]].Count <= 0)
			yield break;
		GameObject enemy = LevelGenerationScript.entityLists [targetableTags [0]] [0].gameObject;
//		GameObject enemy = enemies [0].gameObject;
		float enemyDistance = Vector3.Distance (this.transform.position, enemy.transform.position);

		targetedEntity = enemy;
		yield return new WaitForEndOfFrame ();

		for (int i = 1; i < LevelGenerationScript.entityLists [targetableTags [0]].Count; i++) {
			float testDist = Vector3.Distance (this.transform.position, LevelGenerationScript.entityLists [targetableTags [0]] [i].position);
			if (testDist < enemyDistance) {
				enemyDistance = testDist;
				enemy = LevelGenerationScript.entityLists [targetableTags [0]] [i].gameObject;
			}
			targetedEntity = enemy;
			yield return new WaitForEndOfFrame ();
		}
		yield return null;
	}

	private IEnumerator CheckTargetEntity() {
		//check if it's not a null
		if (targetedEntity_ == null) {
			yield break;
//			yield return StartCoroutine (FindNearbyEnemy ());
		}
		
		yield return new WaitUntil (() => !targetedEntity_.GetComponent<EntityTargetScript>().enabled);
		targetedEntity = null;
		yield return null;
	}

	/*
	 * In future I'd like this to be a field of view sort of system
	 */
	private IEnumerator AquireEnemy() {
		if (targetedEntity_ != null) {
			yield return new WaitUntil (() => targetedEntity_ == null);
		}
		yield return StartCoroutine (FindNearbyEnemy ());	
//		targetedEntity = GameObject.FindWithTag (targetableTags [0]);
//		yield return null;
	}
		
	private bool CheckSight() {
		RaycastHit hit;
		Ray entityRay = new Ray(transform.position, targetedEntity.transform.position - transform.position);
		Debug.DrawRay (transform.position, targetedEntity.transform.position - transform.position, Color.black, 1.0f, true);
		if (Physics.Raycast (entityRay, out hit, 20f)) {
			if (hit.collider.gameObject == targetedEntity_) {
				return true;
			}
			if (hit.collider.CompareTag (targetableTags[0])) {
				//will need to add further checks here for the order and response systems.
				targetedEntity_ = hit.collider.gameObject;
				return true;
			}
		}
		return false;
	}

	private void ProximityCheck() {
		targetProximity = Vector3.Distance (this.gameObject.transform.position, targetedEntity.transform.position);
		if (targetProximity <= 4.0f && targetProximity >= 1.0f) {
			entityNavigationScript.ProximityTrigger ();
		}
	}

	private IEnumerator Attack() {
		Debug.Log ("Prepared to attack");
		Debug.Log (targetedEntity_.name + " is at " + targetedEntity_.GetComponent<EntityStatisticsScript> ().curHealth);
		if (targetedEntity_.GetComponent<EntityStatisticsScript> ().curHealth == 0) {
			targetedEntity = null;
			yield return null;
		}
		yield return new WaitUntil (() => targetProximity <= 1.4f);
		Debug.Log (this.gameObject.name + " is within striking distance!");
		targetProximity = Mathf.Infinity;
		anim.SetTrigger ("Attacking");
		entityNavigationScript.PauseMovement ("stop");
		yield return new WaitForFixedUpdate ();
		Debug.Log (entityNavigationScript.GetAgentIsStopped ());
		yield return new WaitUntil (() => entityNavigationScript.GetAgentIsStopped() == false);
		if (targetedEntity_ != null) {
			yield return StartCoroutine ("Attack");
		} else {
			yield return null;
		}
		yield return null;
	}

	private IEnumerator WatchForTarget() {
		Debug.Log ("attempting to watch for target");
		if (targetedEntity_ == null) {
			Debug.Log ("The target is null now");
			yield return StartCoroutine (AquireEnemy());
			yield return new WaitUntil (() => targetedEntity_);
//			yield return StartCoroutine (EnemyHealthCheck ());
			Debug.Log ("Have a target again!");
			StartCoroutine ("Attack");
		}
//		yield break;
		yield return StartCoroutine (SightCheckTarget ());
	}

	private IEnumerator SightCheckTarget() {
		while (targetedEntity_ != null) {
			if (CheckSight ()) {
				ProximityCheck ();
				entityNavigationScript.SetDestination (targetedEntity.transform.position, this.gameObject);
				entityNavigationScript.StoppedMovementCheck ();
			}
			yield return new WaitForEndOfFrame ();
		}
		yield return StartCoroutine (WatchForTarget ());
	}

	/*
	 * Putting this here until I can think of a better place to toggle the weapon
	 */
	public void ToggleWeaponColliderOld(string state) {
		if (state.Contains ("true")) {
			weaponScript.ToggleCollider (true);
		} else {
			weaponScript.ToggleCollider (false);
		}
	}

    int deathCount = 0;
	public void Die() {
        
		if (this.gameObject.CompareTag ("Hero")) {
            //			
            deathCount++;
		} else {
			GameObject.Find ("SampleExit(Clone)").GetComponent<HatchScript> ().IncrementEnemyKills ();
		}
        targetedEntity_ = null;
		StopAllCoroutines ();
		anim.enabled = false;
		entityNavigationScript.enabled = false;
		GetComponent<NavMeshAgent>().enabled = false;
		GetComponentInChildren<WeaponScript> ().enabled = false;

		//all heros are dead
		bool allDead = true;

		foreach (var hero in GameObject.FindGameObjectsWithTag("Hero")) {
			if (hero != this.gameObject && !hero.GetComponent<EntityTargetScript>().enabled) {
				
			} else
				allDead = false;
		}

        if (allDead)
        {
            Invoke("PlayerDeath", 3.0f);

            this.enabled = false;
        }

        if(deathCount >= 2)
        {
            Invoke("PlayerDeath", 3.0f);
        }
        Debug.Log("allDead: " + allDead + ", DeathCount: " + deathCount);
    }
		
	/// <summary>
	/// Cleans up and disables this component, getting it ready to be enabled again if needbe.
	/// </summary>
	public void CleanUp() {
		StopAllCoroutines ();
		targetedEntity_ = null;
		this.enabled = false;
	}

    public void PlayerDeath()
    {

        sceneController.GameOver();
    }
}
