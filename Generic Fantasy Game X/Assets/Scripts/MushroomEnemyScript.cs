using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomEnemyScript : MonoBehaviour {

	private Rigidbody rb;

	private Animator anim;

	// Use this for initialization
	void Start () {

		Debug.Log("WORKING");
		
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();


	}
	
	// Update is called once per frame
	void FixedUpdate () {

		if(rb.velocity.magnitude > .01){
			anim.SetBool("IsMoving", true);
			Debug.Log("IsMoving");
		} else{
			anim.SetBool("IsMoving", false);
			Debug.Log("IsNOTMoving");
			Debug.Log("velocity magnitued: " + rb.velocity.magnitude);
		}

	}

// 	private IEnumerator IsMoving(){

// 		Vector3 startPos = rb.transform.position;
// 		yield return new WaitForSeconds(0.25f);
// 		Vector3 currentPos = rb.transform.position;

// 		if(startPos.x != currentPos.x || startPos.z != currentPos.z){
// 			anim.SetBool("IsMoving", true);
// 			Debug.Log("IsMoving");
// 		} else {
// 			anim.SetBool("IsMoving", false);
// 			Debug.Log("IsNOTMoving");
// 		}
// 	}
}
