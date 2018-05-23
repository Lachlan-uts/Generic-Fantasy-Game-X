using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySelectedScript : MonoBehaviour {

	private MeshRenderer rend;

	// Use this for initialization
	void Start () {
		rend = GetComponent<MeshRenderer> ();
		rend.enabled = false;
	}

	public void DoAThing() {
		Debug.Log ("THING DID");
	}

	public void SelectionToggle() {
		rend.enabled = !rend.enabled;
	}
}
