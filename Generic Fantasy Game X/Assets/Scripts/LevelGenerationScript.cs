using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelGenerationScript : MonoBehaviour {

	//manage all active entities in a given level.
	public static List<Transform> heros;
	public static List<Transform> enemies;

	public static Dictionary<string,List<Transform>> entityLists = new Dictionary<string, List<Transform>> ()
	{
		{ "Hero", new List<Transform>() },
		{ "Enemy", new List<Transform>() }
	};

	// Public Variables
	public int floorNumber = 0;

	// Serialized Private Variables
	[SerializeField]
	private GameObject[] validEntrances;
	[SerializeField]
	private GameObject[] validRooms;
	[SerializeField]
	private GameObject[] validLoot;
	[SerializeField]
	private GameObject[] validEnemiesLight;
	[SerializeField]
	private GameObject[] validEnemiesModerate;
	[SerializeField]
	private GameObject[] validEnemiesBrute;
	[SerializeField]
	private GameObject exitStairs;
	[SerializeField]
	private GameObject roomDoor;
	[SerializeField]
	private GameObject roomRubble;
	[SerializeField]
	private int numRooms = 3;
	[SerializeField]
	private int numEnemies = 5;

	// Private Variables
	private GameObject entranceRoom; // Used later for comparison such that the starting room cannot contain enemies
	private int genStage;
	private int curRooms;
	private int curEnemies;
	private List<GameObject> rooms; // List of rooms with still usable nodes
	private List<GameObject> completedRooms; // List of rooms with nodes that are all used
	private List<GameObject> doorNodes; // List of nodes at which to place doors
	private List<GameObject> rubbleNodes; // List of nodes at which to place rubble
	private List<GameObject> enemySpawnPoints; // List of enemy spawn points
	private GameObject[] enemyList; // Used to disable enemies prior to 'spawning' in heroes and back again
	private List<GameObject> heroSpawnList; // List of hero spawn locations, dependant on spawn locations within the starting room

	// Use this for initialization
	void Start () {
		rooms = new List<GameObject> ();
		completedRooms = new List<GameObject> ();
		doorNodes = new List<GameObject> ();
		genStage = -2;
	}
	
	// Update is called once per frame
	void Update () {
        if (genStage == -2)
        {
            curRooms = 0;
            generateInitialRoom();
            curEnemies = 0;
            genStage = -1;
            heroSpawnList = rooms[0].GetComponent<RoomValueStore>().enemyLocations;
        }
        else if (genStage == -1)
        {
            if (curRooms < numRooms)
            {
                generateAdditionalRoom();
            }
            else
            {
                genStage = 0; // Temporarily "skip" a "generation stage" during which obstacles could be spawned
                curRooms = 0; // Reset the curRooms so as to iterate through each room for furniture later
            }
        }
        else if (genStage == 0)
        {
            // Generate List of Rubble Nodes
            rubbleNodes = GetRubbleLocations();
            genStage = 1;
        }
        else if (genStage == 1)
        {
            // Spawn Rubble
            if (rubbleNodes.Count > 0)
            {
                SpawnRubble();
            }
            else
            {
                genStage = 2;
            }
        }
        else if (genStage == 2)
        {
            // Spawn Furniture
            if (curRooms < rooms.Count)
            {
                rooms[curRooms].GetComponent<RoomValueStore>().spawnFurniture();
                curRooms++;
            }
            else if (curRooms < (rooms.Count + completedRooms.Count))
            {
                completedRooms[curRooms - rooms.Count].GetComponent<RoomValueStore>().spawnFurniture();
                curRooms++;
            }
            else
            {
                genStage = 3;
            }
        }
        else if (genStage == 3)
        {
            // Generate NavMesh
            GenerateNavMesh();
            genStage = 4;
        }
        else if (genStage == 4)
        {
            // Spawn Doors
            if (doorNodes.Count > 0)
            {
                SpawnDoor();
            }
            else
            {
                genStage = 5;
            }
        }
        else if (genStage == 5)
        {
            // Generate List of enemy spawn locations
            enemySpawnPoints = GetEnemySpawns();
            genStage = 6;
        }
        else if (genStage == 6)
        {
            // Spawn Enemies
            if (curEnemies < numEnemies)
            {
                SpawnEnemy();
            }
            else
            {
                genStage = 7;
            }
        }
        else if (genStage == 7)
        {
            // Spawn the exit hatch
            SpawnExit();
            genStage = 8;
        }
        else if (genStage == 8)
        {
            // Disable enemy units so no premature action occurs
            //			enemyList = GameObject.FindGameObjectsWithTag ("Enemy");
            //			foreach (GameObject enemy in enemyList) {
            //				enemy.SetActive (false);
            //			}
            genStage = 9;
        }
        else if (genStage == 9)
        {
            // Spawn in heroes (in reality, relocate them from wherever to spawn points within the starting room)
            //			GameObject[] heroes = GameObject.FindGameObjectsWithTag("Hero");
            //			int iCount = 0;
            //			while (iCount < heroes.Length) {
            //				heroes [iCount].transform.position = heroSpawnList [iCount].transform.position + 
            //                    new Vector3(0,1,0);
            //				iCount++;
            //			}
            genStage = 10;
        }
        else if (genStage == 10)
        {
            // Re-enable enemies within the floor. It's altercation time!
            //			foreach (GameObject enemy in enemyList) {
            //				enemy.SetActive (true);
            //			}
            genStage = 11;
        }
    }

	void FixedUpdate () {
		
	}

	void generateInitialRoom() {
		GameObject newRoom = validEntrances [Random.Range (0, validEntrances.Length)];
		entranceRoom = Instantiate (newRoom, new Vector3 (0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
		entranceRoom.name = "firstRoom"; // Debug purposes
		rooms.Add (entranceRoom);

		curRooms++;
	}

	void generateAdditionalRoom() {
		// Select an existing room and node within said room
		GameObject room = rooms [Random.Range (0, rooms.Count)];
		GameObject selectedNodeA = room.GetComponent<RoomValueStore> ().roomInternodes [Random.Range (0, room.GetComponent<RoomValueStore> ().roomInternodes.Count)];

		// Select a random room to attempt to build, as well as connecting node
		GameObject newRoom = validRooms [Random.Range (0, validRooms.Length)];
		GameObject selectedNodeB = newRoom.GetComponent<RoomValueStore> ().roomInternodes [Random.Range (0, newRoom.GetComponent<RoomValueStore> ().roomInternodes.Count)];

		// Offset the would-be position by matching position to the node with rotation


		// Offset again by the distance between node and room origin, along with rotation


		gameObject.transform.rotation = Quaternion.Euler(new Vector3 (0.0f, room.transform.rotation.eulerAngles.y + selectedNodeA.transform.localRotation.eulerAngles.y, 0.0f));
		gameObject.transform.rotation = Quaternion.Euler(new Vector3(0.0f, (gameObject.transform.rotation.eulerAngles.y - selectedNodeB.transform.localRotation.eulerAngles.y + 180), 0.0f));

		gameObject.transform.position = selectedNodeA.transform.position; // Old Method

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


	

		// Check for intersection by another pre-existing room
		// Raycast forward, backward, left and right to determine collisions
		bool possible = true;

		// Assemble a bounds for the floor plan of the to be generated room at the calculated co-ords and rotation.
		// Warning: Make sure you reset the rotation and position of the room itself, otherwise catastrophe awaits
		Vector3 origCoords = newRoom.transform.position;
		Vector3 origRotate = newRoom.transform.rotation.eulerAngles;

		newRoom.transform.position = gameObject.transform.position;
		newRoom.transform.rotation = gameObject.transform.rotation;

		List<Bounds> newFloorPlan = new List<Bounds> ();
		foreach (GameObject floorPiece in newRoom.GetComponent<RoomValueStore>().floorPlanObjects) {
			newFloorPlan.Add (floorPiece.GetComponent<MeshRenderer> ().bounds);
		}

		newRoom.transform.position = origCoords;
		newRoom.transform.rotation = Quaternion.Euler(origRotate);

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

		// Check for bounds intersection. If any bounds intersection occurs, not possible to place the room.
		foreach (Bounds floorPiece in newFloorPlan) {
			foreach (Bounds oldFloorPiece in oldFloorPlan) {
				if (oldFloorPiece.Intersects (floorPiece)) {
					if (oldFloorPiece.ClosestPoint(floorPiece.center).x.ToString("F2") == floorPiece.ClosestPoint(oldFloorPiece.center).x.ToString("F2")
						&& oldFloorPiece.ClosestPoint(floorPiece.center).y.ToString("F2") == floorPiece.ClosestPoint(oldFloorPiece.center).y.ToString("F2")
						&& oldFloorPiece.ClosestPoint(floorPiece.center).z.ToString("F2") == floorPiece.ClosestPoint(oldFloorPiece.center).z.ToString("F2")
						&& oldFloorPiece.ClosestPoint(floorPiece.center) != floorPiece.center) {
					} else {
						possible = false;
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
			doorNodes.Add(selectedNodeA); // Add the first of the paired nodes to the list of door nodes
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
							== preActiveNodes [countA].transform.position.z.ToString("F2")) { // If there is another node belonging to another room at these co-ords
							preActiveNodes [countA].gameObject.GetComponentInParent<RoomValueStore> ().removeNodeAvailability (preActiveNodes [countA]);
							preActiveNodes.Remove (preActiveNodes [countA]);
							doorNodes.Add(preActiveNodes[countA]); // Add the paired nodes to the list of door nodes where doors can be spawned
							instantRoom.GetComponent<RoomValueStore> ().removeNodeAvailability (instantRoom.GetComponent<RoomValueStore> ().roomInternodes [countB]);
							countB--;
							countA--;
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
		}
	}

	void SpawnDoor() {
		GameObject newDoor = Instantiate (roomDoor, doorNodes [0].transform.position, Quaternion.Euler (doorNodes [0].transform.rotation.eulerAngles)) as GameObject;
		doorNodes.Remove (doorNodes [0]);
	}

	List<GameObject> GetRubbleLocations() { // Generate and return a list of nodes where rubble can be placed
		List<GameObject> rubbleLocations = new List<GameObject> ();
		foreach (GameObject room in rooms) {
			foreach (GameObject node in room.GetComponent<RoomValueStore>().roomInternodes) {
				rubbleLocations.Add (node);
			}
		}
		return rubbleLocations;
	}

	void SpawnRubble() {
		GameObject newRubble = Instantiate (roomRubble, rubbleNodes [0].transform.position, Quaternion.Euler (rubbleNodes [0].transform.rotation.eulerAngles)) as GameObject;
		rubbleNodes.Remove (rubbleNodes [0]);
	}

	void GenerateNavMesh() {
		if (rooms.Count > 0) {
			rooms [0].GetComponent<RoomValueStore> ().floorPlanObjects [0].GetComponent<NavMeshSurface> ().BuildNavMesh ();
		} else {
			completedRooms [0].GetComponent<RoomValueStore> ().floorPlanObjects [0].GetComponent<NavMeshSurface> ().BuildNavMesh ();
		}
	}

	List<GameObject> GetEnemySpawns() {
		List<GameObject> enemySpawnPoints = new List<GameObject> ();
		foreach (GameObject room in completedRooms) {
			if (room.GetComponent<RoomValueStore> ().enemyLocations.Count > 0 && !room.Equals(entranceRoom)) {
				foreach (GameObject enemySpawn in room.GetComponent<RoomValueStore>().enemyLocations) {
					enemySpawnPoints.Add (enemySpawn);
				}
			}
		}

		foreach (GameObject room in rooms) {
			if (room.GetComponent<RoomValueStore> ().enemyLocations.Count > 0 && !room.Equals(entranceRoom)) {
				foreach (GameObject enemySpawn in room.GetComponent<RoomValueStore>().enemyLocations) {
					enemySpawnPoints.Add (enemySpawn);
				}
			}
		}
		return enemySpawnPoints;
	}

	void SpawnEnemy() {
		

		// Add conditions for spawning an enemy in a valid position, possibly using Physics.OverlapBox centered on the chosen enemy configuration
		int enemySpawnNumber = Random.Range(0, enemySpawnPoints.Count);
		int chanceLow, chanceMid, chanceBrt; // Determines the % chance of a particular enemy class spawning
		int enemyCategory = 0;
		// i.e. > 100 means that class cannot spawn. Useful when the lists are empty as they tend to be at this early stage
		// Note: should later allow chances to be augmented by party count, floor, etc.
		if (validEnemiesModerate.Length > 0 && validEnemiesBrute.Length == 0) {
			chanceLow = 70;
			chanceMid = 100;
			chanceBrt = 101;
		} else if (validEnemiesBrute.Length > 0) {
			chanceLow = 50;
			chanceMid = 80;
			chanceBrt = 100;
		} else {
			chanceLow = 100;
			chanceMid = 101;
			chanceBrt = 101;
		}

		int enemyClassInt = Mathf.RoundToInt(Random.Range (0.0f, 100.0f));
		GameObject enemyToSpawn;
		if (enemyClassInt < chanceLow) {
			enemyCategory = 0;
			enemyToSpawn = validEnemiesLight [Random.Range (0, validEnemiesLight.Length)];
		} else if (enemyClassInt < chanceMid) {
			enemyCategory = 1;
			enemyToSpawn = validEnemiesLight [Random.Range (0, validEnemiesLight.Length)];
			//enemyToSpawn = validEnemiesModerate [Random.Range (0, validEnemiesModerate.GetLength ())];
		} else {
			enemyCategory = 2;
			enemyToSpawn = validEnemiesLight [Random.Range (0, validEnemiesLight.Length)];
			//enemyToSpawn = validEnemiesBrute [Random.Range (0, validEnemiesBrute.GetLength ())];
		}

		Vector3 spawnPos = enemySpawnPoints [enemySpawnNumber].transform.position;
		Vector3 spawnRot = enemySpawnPoints [enemySpawnNumber].transform.rotation.eulerAngles;

		Vector3 spawnPosTest = new Vector3 (spawnPos.x, spawnPos.y + 0.2f, spawnPos.z);

		if (Physics.OverlapBox (spawnPosTest, 
			    new Vector3 (enemyToSpawn.transform.localScale.x/2, 0.05f, enemyToSpawn.transform.localScale.z/2), 
			    Quaternion.Euler (spawnRot)).Length == 0) {
			GameObject newEnemy = Instantiate (enemyToSpawn, spawnPos, Quaternion.Euler (spawnRot)) as GameObject;
			//enemies.Add (newEnemy.transform);
			// Note: Here is where code would go to properly initialize enemy resources
			//newEnemy.GetComponent<EntityStatisticsScript>().GenerateStats(floorNumber, enemyCategory);
			// Remove SpawnPoint from the list
			enemySpawnPoints.Remove(enemySpawnPoints[enemySpawnNumber]);

			curEnemies++;
		}
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
		GameObject exitLocation = Instantiate(exitStairs, exitPos, Quaternion.Euler(exitRot)) as GameObject;
	}

    public void Restart()
    {
        genStage = -2;
    }

	public int GetEnemyCount() {
		return numEnemies;
	}
}