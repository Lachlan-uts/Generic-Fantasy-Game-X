using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EntityTargetScript : MonoBehaviour {

	//The chosen attack target
	[SerializeField]
	public GameObject targetEntity { get; set; }

	/*
	 * Fancy corountine better entity tracking stuff
	 */
	private GameObject targetedEntity_;
	public GameObject targetedEntity {
		get {
			return targetedEntity_;
		}
		set {
			targetedEntity_ = value;
		}
	}

	//Current objective location
	private Vector3 lastKnownLocation;

	//The move system for this entity
	private EntityNavigationScript entityNavigationScript;

	//to trigger the attack
	private Animator anim;

	// Use this for initialization
	void Start () {
		targetEntity = GameObject.FindGameObjectWithTag ("Hero");
		entityNavigationScript = GetComponent<EntityNavigationScript> ();
		anim = GetComponent<Animator> ();
		targetedEntity = null;
		StartCoroutine ("WatchForTarget");
	}
	
	// Update is called once per frame
	void Update () {
//		if (SightCheck ()) {
//			ProximityCheck ();
//			entityNavigationScript.SetDestination (targetEntity.transform.position, this.gameObject);
//			entityNavigationScript.StoppedMovementCheck ();
//		}
	}

	private bool SightCheck() {
		RaycastHit hit;
		Ray entityRay = new Ray(transform.position, targetedEntity.transform.position - transform.position);
		Debug.DrawRay (transform.position, targetedEntity.transform.position - transform.position, Color.black, 1.0f, true);
		if (Physics.Raycast (entityRay, out hit, 20f)) {
			if (hit.collider.CompareTag ("Hero") || hit.collider.CompareTag ("Enemy") && !hit.collider.CompareTag(this.gameObject.tag)) {
				return true;
				Debug.Log ("it can see it!");
			}
		}
		return false;
	}

	private void ProximityCheck() {
		float targetProximity = Vector3.Distance (this.gameObject.transform.position, targetedEntity.transform.position);
		if (targetProximity <= 4.0f && targetProximity >= 1.0f) {
			entityNavigationScript.ProximityTrigger ();
			anim.SetTrigger ("Attacking");
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
			yield return new WaitUntil (() => targetedEntity_);
			Debug.Log ("Have a target again!");
		}
//		yield break;
		yield return StartCoroutine (SightCheckTarget ());
	}

	private IEnumerator SightCheckTarget() {
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
}
