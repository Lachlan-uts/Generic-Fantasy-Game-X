using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AssemblyCSharp;

public class EntityTargetScript : MonoBehaviour {

	//The chosen attack target
	[SerializeField]
	public GameObject targetEntity { get; set; }


	/*
	 * Fancy stuff happening here
	 */
	private GameObject targetedEntity_;
	public GameObject targetedEntity {
		get {
			return targetedEntity_;
		}
		set {
			targetedEntity_ = value;
			StartCoroutine ("EntityChecker");
		}
	}

	//Current objective location
	private Vector3 lastKnownLocation;

	//The move system for this entity
	private EntityNavigationScript entityNavigationScript;

	// Use this for initialization
	void Start () {
		targetEntity = GameObject.FindGameObjectWithTag ("Hero");
		entityNavigationScript = GetComponent<EntityNavigationScript> ();
		targetedEntity = null;
	}
	
	// Update is called once per frame
	void Update () {
		if (SightCheck ()) {
			ProximityCheck ();
			entityNavigationScript.SetDestination (targetEntity.transform.position, this.gameObject);
			entityNavigationScript.StoppedMovementCheck ();
		}
	}

	private bool SightCheck() {
		RaycastHit hit;
		Ray entityRay = new Ray(transform.position, targetEntity.transform.position - transform.position);
		Debug.DrawRay (transform.position, targetEntity.transform.position - transform.position, Color.black, 1.0f, true);
		if (Physics.Raycast (entityRay, out hit, 20f)) {
			if (hit.collider.CompareTag ("Hero") || hit.collider.CompareTag ("Enemy") && !hit.collider.CompareTag(this.gameObject.tag)) {
				return true;
				Debug.Log ("it can see it!");
			}
		}
		return false;
	}

	private void ProximityCheck() {
		float targetProximity = Vector3.Distance (this.gameObject.transform.position, targetEntity.transform.position);
		if (targetProximity <= 4.0f && targetProximity >= 1.0f) {
			entityNavigationScript.ProximityTrigger (OrderHierarchy.Low);
		}
	}

	//Not sure what I wanted to use this for.
	private void FindEntity() {
		
	}

	/*
	 * I want this method to do nothing when it has no target, and start checking for it's target when it has one.
	 * 
	 */
	private IEnumerator watchForTarget() {
		Debug.Log ("tried to do the target watch");
		if (!targetedEntity_) {
			Debug.Log("the target entity is null!");
			yield return new WaitUntil (() => targetedEntity_);
			Debug.Log ("the target entity is not null again.");
		}

//		while (targetedEntity_) {
//			yield return new WaitForFixedUpdate ();
//			Debug.Log ("tried to do the target watch");
//		}
		yield break;

//		if (!targetedEntity_) {
//			yield return null;
//		}
//		//yield return new waitfor
//		if (SightCheck ()) {
//			//ProximityCheck ();
//			entityNavigationScript.SetDestination (targetedEntity_.transform.position, this.gameObject);
//			entityNavigationScript.StoppedMovementCheck ();
//		}
//		yield return watchForTarget ();
	}

	private IEnumerator EntityChecker () {
		Debug.Log ("in the entity checker");
		if (targetedEntity_) {
			Debug.Log ("not null nomore!");
			yield return watchForTarget ();
		} yield return null;
	}
}
