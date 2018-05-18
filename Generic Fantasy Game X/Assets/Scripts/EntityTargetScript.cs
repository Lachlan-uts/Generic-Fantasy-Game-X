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
        Time.timeScale = 1;
        weaponScript = GetComponentInChildren<WeaponScript> ();

		//remove own tag from list of targetable tags
		targetableTags.Remove(this.gameObject.tag);

		entityNavigationScript = GetComponent<EntityNavigationScript> ();
		anim = GetComponent<Animator> ();
		targetedEntity = null;
		StartCoroutine ("WatchForTarget");

	}

	/*
	 * In future I'd like this to be a field of view sort of system
	 */
	private IEnumerator AquireEnemy() {
		if (targetedEntity_) {
			yield return new WaitUntil (() => !targetedEntity_);
		}
		targetedEntity = GameObject.FindWithTag (targetableTags [0]);
		yield return null;
	}

	private bool SightCheck() {
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
		yield return new WaitUntil (() => targetProximity <= 1.4f);
		Debug.Log (this.gameObject.name + " is within striking distance!");
		targetProximity = Mathf.Infinity;
		anim.SetTrigger ("Attacking");
		entityNavigationScript.PauseMovement ("stop");
		yield return new WaitForFixedUpdate ();
		Debug.Log (entityNavigationScript.GetAgentIsStopped ());
		yield return new WaitUntil (() => entityNavigationScript.GetAgentIsStopped() == false);
		if (targetedEntity_) {
			yield return StartCoroutine ("Attack");
		} else {
			yield return null;
		}
	}

	private IEnumerator EntityChecker() {
		Debug.Log ("In the ent checker");
		if (targetedEntity_) {
			Debug.Log ("null no longer");
			yield return WatchForTarget ();
		} yield return null;
	}

	private IEnumerator WatchForTarget() {
		Debug.Log ("attempting to watch for target");
		if (!targetedEntity_) {
			Debug.Log ("The target is null now");
			yield return StartCoroutine (AquireEnemy());
			yield return new WaitUntil (() => targetedEntity_);
			Debug.Log ("Have a target again!");
		}
//		yield break;
		yield return StartCoroutine (SightCheckTarget ());
	}

	private IEnumerator SightCheckTarget() {
		StartCoroutine ("Attack");
		while (targetedEntity_) {
			if (SightCheck ()) {
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
	public void ToggleWeaponCollider(string state) {
		if (state.Contains ("true")) {
			weaponScript.ToggleCollider (true);
		} else {
			weaponScript.ToggleCollider (false);
		}
	}


	public void Die() {
        
		if (this.gameObject.CompareTag("Hero")) {
			Invoke("PlayerDeath", 3.0f);
		}
        targetedEntity_ = null;
		StopAllCoroutines ();
		anim.enabled = false;
		entityNavigationScript.enabled = false;
		GetComponent<NavMeshAgent>().enabled = false;
		GetComponentInChildren<WeaponScript> ().enabled = false;
		this.enabled = false;


    }

	private void PlayerDeath() {
		SceneManager.LoadScene(2); //Temp
	}
}
