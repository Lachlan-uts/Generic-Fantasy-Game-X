using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjectScript : MonoBehaviour {

	[SerializeField]
	private GameObject child;

	// Use this for initialization
	void Start () {
		//gameObject.transform.Translate (Vector3.left);
		//gameObject.transform.Translate(child.transform.position); // Moves the child's amount of translation
		//gameObject.transform.Translate(child.transform.position, Space.World); // Moves to said child
		//gameObject.transform.Rotate(new Vector3(0.0f, 180.0f, 0.0f)); // Flips the parent 180 degrees on y axis
		//gameObject.transform.Translate (-child.transform.position, Space.World); // Moves away from said child

		gameObject.transform.Rotate(new Vector3(0.0f, 180.0f, 0.0f)); 
		gameObject.transform.Translate (-child.transform.position, Space.World); 
		// Flips and then moves away
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
