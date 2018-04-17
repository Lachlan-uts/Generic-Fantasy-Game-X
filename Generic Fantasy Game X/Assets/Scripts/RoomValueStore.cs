using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomValueStore : MonoBehaviour {

	public float xSize = 1.0f;
	public float zSize = 1.0f;
	public List<GameObject> roomInternodes;
	public List<GameObject> floorPlanObjects;
	public List<GameObject> exitLocations;
	public List<GameObject> lootLocations;
	public List<GameObject> enemyLocations;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void removeNodeAvailability(GameObject nodeToRemove) {
		roomInternodes.Remove (nodeToRemove);
		//DestroyImmediate (nodeToRemove, false);
		//Destroy (nodeToRemove);
		nodeToRemove.SetActive (false);
	}

	public void removeNodeAvailability(int nodeToRemoveIndex) {
		GameObject nodeToRemove = roomInternodes [nodeToRemoveIndex];
		roomInternodes.Remove (nodeToRemove);
		//DestroyImmediate (nodeToRemove, false);
		//Destroy (nodeToRemove);
		nodeToRemove.SetActive (false);
	}
}
