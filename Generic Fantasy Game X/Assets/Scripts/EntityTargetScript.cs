using System.Collections;
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

	//list of targets
	//Dictionary<GameObject,float> TargetProximities = new Dictionary<GameObject, float> ();

	private List<Transform> enemies = new List<Transform>();

	// Use this for initialization
	void Start () {
        Time.timeScale = 1;
        weaponScript = GetComponentInChildren<WeaponScript> ();

		//remove own tag from list of targetable tags
		targetableTags.Remove(this.gameObject.tag);

		entityNavigationScript = GetComponent<EntityNavigationScript> ();
		anim = GetComponent<Animator> ();
		targetedEntity = null;
		StartCoroutine ("WatchForTarget");

	}

	private IEnumerator PopulateEnemyList () {
		foreach (var enemy in GameObject.FindGameObjectsWithTag(targetableTags [0])) {
			enemies.Add (enemy.transform);
			//TargetProximities.Add (enemy, Vector3.Distance (enemy.transform.position, this.transform.position));
		}
		yield return StartCoroutine ("WatchForTarget");
	}

	private IEnumerator FindNearbyEnemy() {
		GameObject enemy = enemies [0].gameObject;
		float enemyDistance = Vector3.Distance (this.transform.position, enemies [0].position);

		targetedEntity = enemy;
		yield return new WaitForEndOfFrame ();

		for (int i = 1; i < enemies.Count; i++) {
			float testDist = Vector3.Distance (this.transform.position, enemies [i].position);
			if (testDist < enemyDistance) {
				enemyDistance = testDist;
				enemy = enemies [i].gameObject;
			}
			targetedEntity = enemy;
			yield return new WaitForEndOfFrame ();
		}
		yield return null;
	}

	private IEnumerator CheckTargetEntity() {
		//check if it's not a null
		if (targetedEntity_ == null)
			yield break;
		
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
		targetedEntity = GameObject.FindWithTag (targetableTags [0]);
		yield return null;
	}
		
	private bool CheckSight() {
		RaycastHit hit;
		Ray entityRay = new Ray(transform.position, targetedEntity.transform.position - transform.position);
		//Debug.DrawRay (transform.position, targetedEntity.transform.position - transform.position, Color.black, 1.0f, true);
		if (Physics.Raycast (entityRay, out hit, 20f)) {
			if (hit.collider.CompareTag ("Hero") || hit.collider.CompareTag ("Enemy") && !hit.collider.CompareTag(this.gameObject.tag)) {
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

	private IEnumerator EntityChecker() {
		Debug.Log ("In the ent checker");
		if (targetedEntity_ != null) {
			Debug.Log ("null no longer");
			yield return WatchForTarget ();
		} yield return null;
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
			yield return null;
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


	public void Die() {
        
		if (this.gameObject.CompareTag ("Hero")) {
//			Invoke ("PlayerDeath", 3.0f);
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
			Invoke ("PlayerDeath", 3.0f);

		this.enabled = false;


    }

	private void PlayerDeath() {
		SceneManager.LoadScene(2); //Temp
	}
}
