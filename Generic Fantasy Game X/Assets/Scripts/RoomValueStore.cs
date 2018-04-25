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
	public List<GameObject> furnitureLocations;
	public List<GameObject> validFurniture;

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

	public void spawnFurniture() {
		if (validFurniture.Count > 0 && furnitureLocations.Count > 0) {
			foreach (GameObject furniturePos in furnitureLocations) {
				if (Random.Range (0, 30) < 25) {
					// Attempt to spawn a random piece of furniture, ensuring it doesn't collide with pre-existing furniture
					int furnitureToSpawn = Random.Range (0, validFurniture.Count);
					Vector3 augmentedPos = new Vector3 (furniturePos.transform.position.x, furniturePos.transform.position.y + 0.2f, furniturePos.transform.position.z);
					Vector3 augSize = new Vector3 ((validFurniture [furnitureToSpawn].GetComponent<FurnitureScript> ().xSize / 2), 
						                  0.05f, 
						                  (validFurniture [furnitureToSpawn].GetComponent<FurnitureScript> ().zSize / 2));
					if (Physics.OverlapBox (augmentedPos, augSize, Quaternion.Euler (furniturePos.transform.rotation.eulerAngles)).Length == 0) {
						GameObject newFurniture = Instantiate (validFurniture [furnitureToSpawn], furniturePos.transform) as GameObject;
					}
				}
			}
		}
	}
}
