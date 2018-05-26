using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingUIScript : MonoBehaviour {

	private Vector3 camPos;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		camPos = Camera.main.transform.position;
//		transform.LookAt (new Vector3 (-camPos.x, transform.position.y, -camPos.z));
		transform.LookAt (new Vector3 (transform.position.x, transform.position.y, transform.position.z-4));
	}
}
