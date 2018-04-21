using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour {


	// Serialized private variables
	[SerializeField]
	private Camera MCamera;

	// Setting up a theoretical event system based on whether this variable updates
	private GameObject selectedObject;
	public GameObject sObject {
		get { return selectedObject; }
		set {
			if (selectedObject == value) {
				return;
			}
			selectedObject = value;
			if (selectedObject != null) {
				switch (selectedObject.tag) {
				default:
					break;
				}
			}
		}
	}

	// Use this for initialization
	void Start () {
		selectedObject = null;
	}
	
	// Update is called once per frame
	void Update () {


		RaycastHit hit;
		Ray cameraRay = MCamera.ScreenPointToRay (Input.mousePosition);
		//Debug.Log ("hitPoint: " + hit.transform.position);

		if (Physics.Raycast(cameraRay, out hit) && !selectedObject) {
		} else if (!(selectedObject)) {
		}

		if (Input.GetButtonDown ("Fire1")) {
			if (Physics.Raycast(cameraRay, out hit) && !selectedObject) {
				sObject = hit.collider.gameObject;
			} else {
				sObject = null;
			}
		}

	}

	// private methods



	// public methods

	// get methodology

	public GameObject getSelectedObject() {
		return selectedObject;
	}
}