using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureScript : MonoBehaviour {

	// Public variables
	public float xSize;
	public float zSize;

	// Serialized private variables
	[SerializeField]
	private List<GameObject> subParts; // Storage for spawning decor on the object (e.g. books on a bookshelf)
	[SerializeField]
	private List<GameObject> validParts; // List of parts that could form extra decor

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
