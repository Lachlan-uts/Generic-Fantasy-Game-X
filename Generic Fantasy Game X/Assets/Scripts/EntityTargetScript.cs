using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EntityTargetScript : MonoBehaviour {

	//The chosen attack target
	[SerializeField]
	private GameObject targetEntity;

	//Current objective location
	private Vector3 lastKnownLocation;

	//The move system for this entity
	private EntityNavigationScript entityNavigationScript;

	// Use this for initialization
	void Start () {
		targetEntity = GameObject.FindGameObjectWithTag ("Hero");
		entityNavigationScript = GetComponent<EntityNavigationScript> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (SightCheck ()) {
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


	private void FindEntity() {
		
	}
}
