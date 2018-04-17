using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelGenerationScript : MonoBehaviour {

	// Serialized Private Variables
	[SerializeField]
	private GameObject[] validEntrances;
	[SerializeField]
	private GameObject[] validRooms;
	[SerializeField]
	private GameObject[] validLoot;
	[SerializeField]
	private GameObject[] validEnemies;
	[SerializeField]
	private GameObject exitStairs;
	[SerializeField]
	private int numRooms = 3;
	[SerializeField]
	private int numEnemies = 5;

	// Private Variables
	private int genStage;
	private int curRooms;
	private int curEnemies;
	private List<GameObject> rooms;
	private List<GameObject> completedRooms;
	private List<GameObject> enemySpawnPoints;

	// Use this for initialization
	void Start () {
		rooms = new List<GameObject> ();
		completedRooms = new List<GameObject> ();
		curRooms = 0;
		generateInitialRoom ();
		genStage = 0;
		curEnemies = 0;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate () {
		if (genStage == 0) {
			if (curRooms < numRooms) {
				generateAdditionalRoom ();
			} else {
				genStage = 1; // Temporarily "skip" a "generation stage" during which obstacles could be spawned
			}
		} else if (genStage == 1) {
			// Future Support for generation of "chests", should we decide to add those in
			genStage = 2;
		} else if (genStage == 2) {
			GenerateNavMesh ();
			genStage = 3;
		} else if (genStage == 3) {
			enemySpawnPoints = GetEnemySpawns ();
			genStage = 4;
		} else if (genStage == 4) {
			if (curEnemies < numEnemies) {
				SpawnEnemy ();
			}
			genStage = 5;
		} else if (genStage == 5) {
			SpawnExit ();
			genStage = 6;
		}
	}

	void generateInitialRoom() {
		GameObject newRoom = validEntrances [Random.Range (0, validEntrances.Length)];
		GameObject instantRoom = Instantiate (newRoom, new Vector3 (0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
		instantRoom.name = "firstRoom"; // Debug purposes
		rooms.Add (instantRoom);

		curRooms++;
	}

	void generateAdditionalRoom() {
		// Select an existing room and node within said room
		GameObject room = rooms [Random.Range (0, rooms.Count)];
		Debug.Log ("Room Selected: " + room.name);
		GameObject selectedNodeA = room.GetComponent<RoomValueStore> ().roomInternodes [Random.Range (0, room.GetComponent<RoomValueStore> ().roomInternodes.Count)];
		Debug.Log (room.name + " Node A: " + selectedNodeA.name);

		// Select a random room to attempt to build, as well as connecting node
		GameObject newRoom = validRooms [Random.Range (0, validRooms.Length)];
		GameObject selectedNodeB = newRoom.GetComponent<RoomValueStore> ().roomInternodes [Random.Range (0, newRoom.GetComponent<RoomValueStore> ().roomInternodes.Count)];
		Debug.Log (newRoom.name + " Node B: " + selectedNodeB.name);

		// Offset the would-be position by matching position to the node with rotation

		//gameObject.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f));

		//Debug.Log ("Position x/y/z: " + gameObject.transform.position.x + "/" + gameObject.transform.position.y + "/" + gameObject.transform.position.z
		//	+ "\nRotation x/y/z: " + gameObject.transform.rotation.x + "/" + gameObject.transform.rotation.y + "/" + gameObject.transform.rotation.z);
		


		Debug.Log ("Position x/y/z: " + gameObject.transform.position.x + "/" + gameObject.transform.position.y + "/" + gameObject.transform.position.z
			+ "\nRotation x/y/z: " + gameObject.transform.rotation.x + "/" + gameObject.transform.rotation.y + "/" + gameObject.transform.rotation.z);


		// Offset again by the distance between node and room origin, along with rotation

		//Debug.Log ("as euler: " + selectedNodeB.transform.rotation.eulerAngles.y);
		//Debug.Log ("roation is " + selectedNodeB.transform.rotation);
		//Debug.Log ("local roation is " + selectedNodeB.transform.localRotation);

		//Debug.Log ("Step 1: Position x/y/z: " + gameObject.transform.position.x + "/" + gameObject.transform.position.y + "/" + gameObject.transform.position.z
		//	+ "\nRotation x/y/z: " + gameObject.transform.rotation.x + "/" + gameObject.transform.rotation.y + "/" + gameObject.transform.rotation.z);
		gameObject.transform.rotation = Quaternion.Euler(new Vector3 (0.0f, room.transform.rotation.eulerAngles.y + selectedNodeA.transform.localRotation.eulerAngles.y, 0.0f));
		// Old Method
		gameObject.transform.rotation = Quaternion.Euler(new Vector3(0.0f, (gameObject.transform.rotation.eulerAngles.y - selectedNodeB.transform.localRotation.eulerAngles.y + 180), 0.0f));
		// Old Method

		//Vector3 vRotation = new Vector3(0.0f, 0.0f, 0.0f);
		//vRotation.y = 180 + selectedNodeB.transform.rotation.eulerAngles.y - selectedNodeA.transform.rotation.eulerAngles.y;
		//Debug.Log ("SNA LEuler: " + selectedNodeA.transform.localRotation.eulerAngles.y + " : SNA Euler: " + selectedNodeA.transform.rotation.eulerAngles.y + 
		//	"\nSNB LEuler: " + selectedNodeB.transform.localRotation.eulerAngles.y + " : SNB Euler: " + selectedNodeB.transform.rotation.eulerAngles.y);
		//gameObject.transform.Rotate (vRotation, Space.World);
		//gameObject.transform.Translate (selectedNodeA.transform.position - selectedNodeB.transform.position, Space.World); // New Method
		//gameObject.transform.position = Vector3.zero; // New Method

		gameObject.transform.position = selectedNodeA.transform.position; // Old Method
		//gameObject.transform.position = (gameObject.transform.position + selectedNodeB.transform.localPosition); // Old Method

		// Possible new methodology
		/* 
		 * Go to the position and rotation of the master room
		 * Translate to node a, rotate to match node a in the world
		 * flip to match node b's facing angle, back translate to new room's position and rotation
		 * 
		 */

		gameObject.transform.Translate (-selectedNodeB.transform.position, Space.Self);
		gameObject.transform.position = new Vector3 ((Mathf.Round(gameObject.transform.position.x * 100.0f) / 100.0f),
			(Mathf.Round(gameObject.transform.position.y * 100.0f) / 100.0f),
			(Mathf.Round(gameObject.transform.position.z * 100.0f) / 100.0f));

		//gameObject.transform.position = new Vector3 (0.0f, 0.0f, 0.0f);
		//Vector3 correctiveTranslation = selectedNodeA.transform.position - selectedNodeB.transform.position;
		//gameObject.transform.position += correctiveTranslation;


		Debug.Log ("Result: Position x/y/z: " + gameObject.transform.position.x + "/" + gameObject.transform.position.y + "/" + gameObject.transform.position.z
			+ "\nRotation x/y/z: " + gameObject.transform.rotation.x + "/" + gameObject.transform.rotation.y + "/" + gameObject.transform.rotation.z);
		//gameObject.transform.rotation = Quaternion.Euler(new Vector3(0.0f, (gameObject.transform.rotation.y - selectedNodeB.transform.localRotation.y + 180), 0.0f));
		//Debug.Log ("Position x/y/z: " + gameObject.transform.position.x + "/" + gameObject.transform.position.y + "/" + gameObject.transform.position.z
		//	+ "\nRotation x/y/z: " + gameObject.transform.rotation.x + "/" + gameObject.transform.rotation.y + "/" + gameObject.transform.rotation.z);

		// Check for intersection by another pre-existing room
		// Raycast forward, backward, left and right to determine collisions
		bool possible = true;

		// Assemble a bounds for the floor plan of the to be generated room at the calculated co-ords and rotation.
		// Warning: Make sure you reset the rotation and position of the room itself, otherwise catastrophe awaits
		Vector3 origCoords = newRoom.transform.position;
		Vector3 origRotate = newRoom.transform.rotation.eulerAngles;
		Debug.Log ("Room Params Noted");

		newRoom.transform.position = gameObject.transform.position;
		newRoom.transform.rotation = gameObject.transform.rotation;
		Debug.Log ("Room Params Modified");

		List<Bounds> newFloorPlan = new List<Bounds> ();
		foreach (GameObject floorPiece in newRoom.GetComponent<RoomValueStore>().floorPlanObjects) {
			newFloorPlan.Add (floorPiece.GetComponent<MeshRenderer> ().bounds);
			Debug.Log ("Item Added: " + floorPiece.name);
		}
		Debug.Log ("Bounds Data Created For New Room");

		/*
		foreach (Bounds floorPiece in newFloorPlan) {
			Debug.Log ("Old Extents: " + floorPiece.extents);
			floorPiece.Expand (-0.1f); // Cut a small amount off the bounds for each new floor piece a tiny bit to avoid detecting "touches"
			Debug.Log ("New Extents: " + floorPiece.extents); 
		}
		*/

		string debugString = "";
		foreach (Bounds floorPiece in newFloorPlan) {
			debugString = debugString + "/Center:/" + floorPiece.center + "/Min:/" + floorPiece.min + "/Max:/" + floorPiece.max + "\n";
		}
		Debug.Log (debugString);

		newRoom.transform.position = origCoords;
		newRoom.transform.rotation = Quaternion.Euler(origRotate);
		Debug.Log ("Room Params Reset");

		// Assemble a bounds for the floor plan of every room in existance to check against. This is the point where it may have been a good idea to write a co-routine for this.

		List<Bounds> oldFloorPlan = new List<Bounds> ();
		if (completedRooms.Count > 1) {
			foreach (GameObject oldRoom in completedRooms) {
				foreach (GameObject oldFloorPiece in oldRoom.GetComponent<RoomValueStore>().floorPlanObjects) {
					oldFloorPlan.Add (oldFloorPiece.GetComponent<MeshRenderer> ().bounds);
				}
			}
		}

		foreach (GameObject oldRoom in rooms) {
			foreach (GameObject oldFloorPiece in oldRoom.GetComponent<RoomValueStore>().floorPlanObjects) {
				oldFloorPlan.Add (oldFloorPiece.GetComponent<MeshRenderer> ().bounds);
			}
		}
		Debug.Log ("Bounds Data Recreated for Old Rooms");

		debugString = "";
		foreach (Bounds floorPiece in oldFloorPlan) {
			debugString = debugString + "/Center:/" + floorPiece.center + "/Min:/" + floorPiece.min + "/Max:/" + floorPiece.max + "\n";
		}
		Debug.Log (debugString);

		// Check for bounds intersection. If any bounds intersection occurs, not possible to place the room.
		foreach (Bounds floorPiece in newFloorPlan) {
			foreach (Bounds oldFloorPiece in oldFloorPlan) {
				if (oldFloorPiece.Intersects (floorPiece)) {
					//Debug.Log ("OFPCP: " + oldFloorPiece.ClosestPoint(floorPiece.center) + "\nNFPCP: " + floorPiece.ClosestPoint(oldFloorPiece.center));
					//Debug.Log ("Z outcome: " + (oldFloorPiece.ClosestPoint(floorPiece.center).z.ToString("F2") == floorPiece.ClosestPoint(oldFloorPiece.center).z.ToString("F2")));
					if (oldFloorPiece.ClosestPoint(floorPiece.center).x.ToString("F2") == floorPiece.ClosestPoint(oldFloorPiece.center).x.ToString("F2")
						&& oldFloorPiece.ClosestPoint(floorPiece.center).y.ToString("F2") == floorPiece.ClosestPoint(oldFloorPiece.center).y.ToString("F2")
						&& oldFloorPiece.ClosestPoint(floorPiece.center).z.ToString("F2") == floorPiece.ClosestPoint(oldFloorPiece.center).z.ToString("F2")
						&& oldFloorPiece.ClosestPoint(floorPiece.center) != floorPiece.center) {
						Debug.Log ("Test Concluded: Floor pieces \"touching\"");
					} else {
						possible = false;
						Debug.Log ("Test Concluded: No Placement Possible");
						//curRooms = numRooms;
						break;
					}
				}
			}
		}

		if (possible) {
			GameObject instantRoom = Instantiate (newRoom, gameObject.transform.position, gameObject.transform.rotation) as GameObject;
			// Instantiate the new room
			instantRoom.name = "newRoom" + curRooms;
			// Rename it for debug purposes
			instantRoom.GetComponent<RoomValueStore> ().removeNodeAvailability (newRoom.GetComponent<RoomValueStore>().roomInternodes.IndexOf(selectedNodeB));
			// Remove selectedNodeB within the new instance of the room
			room.GetComponent<RoomValueStore> ().removeNodeAvailability (selectedNodeA);
			// Remove selectedNodeA within the original target room

			// Compile a list of all pre-existing nodes, for the purpose of comparing them against the nodes in the new room, if the new room has had more than 1 node
			if (instantRoom.GetComponent<RoomValueStore> ().roomInternodes.Count >= 1) {
				List<GameObject> preActiveNodes = new List<GameObject> ();
				foreach (GameObject preRoom in rooms) {
					if (preRoom.GetComponent<RoomValueStore> ().roomInternodes.Count >= 1) {
						foreach (GameObject node in preRoom.GetComponent<RoomValueStore>().roomInternodes) {
							preActiveNodes.Add (node);
						}
					}
				}

				int countA = 0;
				int countB = 0;
				//bool countDown = true;
				while (countA < preActiveNodes.Count) {
					//countDown = true;
					while (countB < instantRoom.GetComponent<RoomValueStore>().roomInternodes.Count) {
						if (instantRoom.GetComponent<RoomValueStore> ().roomInternodes [countB].transform.position.x.ToString("F2")
							== preActiveNodes [countA].transform.position.x.ToString("F2")
							&& instantRoom.GetComponent<RoomValueStore> ().roomInternodes [countB].transform.position.y.ToString("F2")
							== preActiveNodes [countA].transform.position.y.ToString("F2")
							&& instantRoom.GetComponent<RoomValueStore> ().roomInternodes [countB].transform.position.z.ToString("F2")
							== preActiveNodes [countA].transform.position.z.ToString("F2")) {
							preActiveNodes [countA].gameObject.GetComponentInParent<RoomValueStore> ().removeNodeAvailability (preActiveNodes [countA]);
							preActiveNodes.Remove (preActiveNodes [countA]);
							instantRoom.GetComponent<RoomValueStore> ().removeNodeAvailability (instantRoom.GetComponent<RoomValueStore> ().roomInternodes [countB]);
							countB--;
							countA--;
							Debug.Log ("Nodes Cancelled");
							if (instantRoom.GetComponent<RoomValueStore> ().roomInternodes.Count < 1) {
								break;
							}
						}
						countB++;
					}

					countA++;
				}
			}

			rooms.Add (instantRoom);
			// Add the instantiated room to the list of rooms.
			curRooms++;
			// Increment number of rooms made

			if (room.GetComponent<RoomValueStore> ().roomInternodes.Count == 0) { // If no more nodes in the original room, the room is "complete"
				completedRooms.Add (room);
				rooms.Remove (room);
			}

			if (instantRoom.GetComponent<RoomValueStore> ().roomInternodes.Count == 0) { // If no more nodes in the instantiated room, the room is "complete"
				completedRooms.Add (instantRoom);
				rooms.Remove (instantRoom);
			}

			Debug.Log (instantRoom.name + " Done!");
		}
	}

	void GenerateNavMesh() {
		

		foreach (GameObject room in rooms) {
			foreach (GameObject floorPiece in room.GetComponent<RoomValueStore>().floorPlanObjects) {
				floorPiece.GetComponent<NavMeshSurface> ().BuildNavMesh ();
			}
		}

		foreach (GameObject room in completedRooms) {
			foreach (GameObject floorPiece in room.GetComponent<RoomValueStore>().floorPlanObjects) {
				floorPiece.GetComponent<NavMeshSurface> ().BuildNavMesh ();
			}
		}
	}

	List<GameObject> GetEnemySpawns() {
		List<GameObject> enemySpawnPoints = new List<GameObject> ();
		foreach (GameObject room in completedRooms) {
			if (room.GetComponent<RoomValueStore> ().enemyLocations.Count > 0) {
				foreach (GameObject enemySpawn in room.GetComponent<RoomValueStore>().enemyLocations) {
					enemySpawnPoints.Add (enemySpawn);
				}
			}
		}

		foreach (GameObject room in rooms) {
			if (room.GetComponent<RoomValueStore> ().enemyLocations.Count > 0) {
				foreach (GameObject enemySpawn in room.GetComponent<RoomValueStore>().enemyLocations) {
					enemySpawnPoints.Add (enemySpawn);
				}
			}
		}

		return enemySpawnPoints;
	}

	void SpawnEnemy() {
		// Add conditions for spawning an enemy in a valid position, possibly using Physics.OverlapBox centered on the chosen enemy configuration
		curEnemies++;
	}

	void SpawnExit() {
		List<GameObject> potentialExits = new List<GameObject> ();

		foreach (GameObject room in completedRooms) {
			if (room.GetComponent<RoomValueStore> ().exitLocations.Count > 0) {
				foreach (GameObject exitSpawn in room.GetComponent<RoomValueStore>().exitLocations) {
					potentialExits.Add (exitSpawn);
				}
			}
		}

		foreach (GameObject room in rooms) {
			if (room.GetComponent<RoomValueStore> ().exitLocations.Count > 0) {
				foreach (GameObject exitSpawn in room.GetComponent<RoomValueStore>().exitLocations) {
					potentialExits.Add (exitSpawn);
				}
			}
		}

		int exitNumber = Random.Range (0, potentialExits.Count);
		Vector3 exitPos = potentialExits [exitNumber].transform.position;
		Vector3 exitRot = potentialExits [exitNumber].transform.rotation.eulerAngles;

		// Spawn the Exit Stairs
		//GameObject exitLocation = Instantiate(exitStairs, exitPos, Quaternion.Euler(exitRot)) as GameObject;
		Debug.Log("Exit Spawned.");
	}

}
