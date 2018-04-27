using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatchScript : MonoBehaviour {

	// private variables
	private float hatchRange = 6.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ExitLevel() {
		GameObject[] heroes = GameObject.FindGameObjectsWithTag ("Hero");
		bool canLeave = true;
		foreach (GameObject hero in heroes) {
			if (Vector3.Distance (hero.transform.position, gameObject.transform.position) > hatchRange) {
				canLeave = false;
			}
		}

		if (canLeave) {
			Debug.Log ("Level Complete!");
		}
	}
}
