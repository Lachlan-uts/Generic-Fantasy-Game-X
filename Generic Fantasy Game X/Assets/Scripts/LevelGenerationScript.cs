using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelGenerationScript : MonoBehaviour {

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
	private int numEnemies = 1;

	// Private Variables
	private GameObject entranceRoom; // Used later for comparison such that the starting room cannot contain enemies
	private int genStage;
	private int curRooms;
	private int curEnemies =0;
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
		InvokeRepeating ("coRoom", 0.0f, 0.1f);
	}

	// Update is called once per frame
	void Update () {

		Debug.Log (curEnemies + "/" + numEnemies);

	}

	void FixedUpdate () {

	}

	void coRoom()
	{
		if (genStage == -2)
		{
			curRooms = 0;
			generateInitialRoom();
			//curEnemies = 0;
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
				Debug.Log("Furniture in 'incomplete' rooms.");
			}
			else if (curRooms < (rooms.Count + completedRooms.Count))
			{
				completedRooms[curRooms - rooms.Count].GetComponent<RoomValueStore>().spawnFurniture();
				curRooms++;
				Debug.Log("Furniture in 'complete' rooms.");
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
			Debug.Log("Spawning Enemy");
			if (curEnemies < numEnemies)
			{
				SpawnEnemy();
				curEnemies++;
			}
			else
			{
				Debug.Log("finish spawning enemies");
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
			CancelInvoke ();
		}

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

	void SpawnDoor() {
		GameObject newDoor = Instantiate (roomDoor, doorNodes [0].transform.position, Quaternion.Euler (doorNodes [0].transform.rotation.eulerAngles)) as GameObject;
		Debug.Log("Door Spawned");
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
		Debug.Log("Rubble Spawned");
		rubbleNodes.Remove (rubbleNodes [0]);
	}

	//void GenerateFurniture() {
	//
	//}

	void GenerateNavMesh() {

		/*
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

		*/
		Debug.Log ("Room Count: " + rooms.Count);
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

		//Debug.Log ("Spawn Position: " + spawnPosTest);
		//Debug.Log ("Spawn Collisions Length: " + Physics.OverlapBox (spawnPosTest, 
		//	new Vector3 (enemyToSpawn.transform.localScale.x / 2, 0.05f, enemyToSpawn.transform.localScale.z / 2), 
		//	Quaternion.Euler (spawnRot)).Length);

		if (Physics.OverlapBox (spawnPosTest, 
			new Vector3 (enemyToSpawn.transform.localScale.x/2, 0.05f, enemyToSpawn.transform.localScale.z/2), 
			Quaternion.Euler (spawnRot)).Length == 0) {
			GameObject newEnemy = Instantiate (enemyToSpawn, spawnPos, Quaternion.Euler (spawnRot)) as GameObject;
			// Note: Here is where code would go to properly initialize enemy resources
			newEnemy.GetComponent<EntityStatisticsScript>().GenerateStats(floorNumber, enemyCategory);
			// Remove SpawnPoint from the list
			enemySpawnPoints.Remove(enemySpawnPoints[enemySpawnNumber]);

			//curEnemies++;
		}


		//curEnemies++;
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
		Debug.Log ("exitNumber: " + exitNumber + ": potentialExitCount: " + potentialExits.Count);
		Vector3 exitPos = potentialExits [exitNumber].transform.position;
		Vector3 exitRot = potentialExits [exitNumber].transform.rotation.eulerAngles;

		// Spawn the Exit Stairs
		GameObject exitLocation = Instantiate(exitStairs, exitPos, Quaternion.Euler(exitRot)) as GameObject;
		Debug.Log("Exit Spawned.");
	}

	public void Restart()
	{
		genStage = -2;
	}
}
