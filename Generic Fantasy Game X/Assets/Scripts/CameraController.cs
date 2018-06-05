using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//to be removed after working this out - or maybe not
using UnityEngine.AI;

public class CameraController : MonoBehaviour {

	public enum relativePositions {
		Right,
		Left,
		forward,
		backward,
		diagForwardRight,
		diagForwardLeft,
		diagBackwardRight,
		diagBackwardLeft
	}

	[SerializeField]
	private int screenEdgePixelSize = 20;

	[SerializeField]
	GameObject cameraTarget;
	public float rotateSpeed;
	float rotate;
	public float offsetDistance;
	public float offsetHeight;
	public float smoothing;
	Vector3 offset;
	[SerializeField]
	bool following = false;
	Vector3 lastPosition;

	private Vector3 playerMoveInput, rotationTarget;
	private float playerScrollInput;
	private Vector2 boxPosOrg, boxPosEnd = Vector2.zero;
	private Rect metalBawx;
	//The texture of the bawx
	public Texture bawxTexture;
	//The list of hero dudes.
	[SerializeField]
	private List<GameObject> heros, selectedHeros;

	//list of destinations to help work out destination pathing.
	private List<Vector3> destinationGizmos = new List<Vector3>();

	//A series of needed vector3 lists
	private List<Vector3> heroDestinations, lineFormation, initialPoint;

	void Start() {
        Time.timeScale = 1;
        cameraTarget = GameObject.FindGameObjectWithTag("Hero");
		lastPosition = new Vector3(cameraTarget.transform.position.x, cameraTarget.transform.position.y + offsetHeight, cameraTarget.transform.position.z - offsetDistance);
		offset = new Vector3(cameraTarget.transform.position.x, cameraTarget.transform.position.y + offsetHeight, cameraTarget.transform.position.z - offsetDistance);
		cameraTarget = null;
		heros.AddRange (GameObject.FindGameObjectsWithTag ("Hero"));
		selectedHeros = new List<GameObject> ();

		//setup the formations here
		heroDestinations = new List<Vector3>();
		lineFormation = new List<Vector3> () { Vector3.right, Vector3.left, Vector3.back };
		initialPoint = new List<Vector3> () { Vector3.zero };
	}

	void Update() {
		if (Input.GetKey (KeyCode.Mouse0)) {
			if (Input.GetKeyDown (KeyCode.Mouse0))
				boxPosOrg = Input.mousePosition;
			else
				boxPosEnd = Input.mousePosition;
		} else {
			if (boxPosOrg != Vector2.zero && boxPosOrg != Vector2.zero) {
				Debug.Log ("Many welps, Handle it!");
//				run a unit selection
				GetDudes ();
			}
			boxPosOrg = Vector2.zero;
			boxPosEnd = Vector2.zero;
		}

		if (Input.GetButtonDown ("Fire1")) {
			GetUnit ();
		}
		if (Input.GetButtonDown ("Fire2")) {
			GetTarget ();
		}
		if (Input.GetKeyDown (KeyCode.G)) {
			Debug.Log ("Attempting to assign loot");
			AssignLootToAcquire ();
		}

		//Getting player inputs
		playerMoveInput = new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical"));
		MouseInput ();

		//Getting the camera target
		CameraTarget ();

		//Rotation adjusted local movement
		Vector3 adjustedVector;
		adjustedVector = Quaternion.Euler (0, this.transform.eulerAngles.y, 0) * playerMoveInput;
		this.transform.Translate (adjustedVector * Time.deltaTime * 5, Space.World);

		//Performing rotation
		if (Input.GetKey (KeyCode.Q)) {
			rotate = 1;
			this.transform.RotateAround (rotationTarget, Vector3.up, 20 * Time.deltaTime);
		} else if (Input.GetKey (KeyCode.E)) {
			rotate = -1;
			this.transform.RotateAround (rotationTarget, Vector3.down, 20 * Time.deltaTime);
		} else {
			rotate = 0;
		}

		//Performing zooming
		CameraZoom ();

		//Following toggle
		if(Input.GetKey(KeyCode.F)) {
			following = !following;
			if (cameraTarget == null) {
				cameraTarget = GameObject.FindGameObjectWithTag("Hero");
			}
		}

		if(following) {
			offset = Quaternion.AngleAxis(rotate * rotateSpeed, Vector3.up) * offset;
			transform.position = cameraTarget.transform.position + offset; 
			transform.position = new Vector3(Mathf.Lerp(lastPosition.x, cameraTarget.transform.position.x + offset.x, smoothing * Time.deltaTime), 
				Mathf.Lerp(lastPosition.y, cameraTarget.transform.position.y + offset.y, smoothing * Time.deltaTime), 
				Mathf.Lerp(lastPosition.z, cameraTarget.transform.position.z + offset.z, smoothing * Time.deltaTime));
			transform.LookAt(cameraTarget.transform.position);
		}
	}

	void LateUpdate() {
		lastPosition = transform.position;
	}

	//drawing relevent gui stuff
	void OnGUI() {
		if (boxPosOrg != Vector2.zero && boxPosEnd != Vector2.zero) {
//			Rect metalBawx = new Rect (boxPosOrg, new Vector2 (boxPosEnd.x - boxPosOrg.x, boxPosEnd.y - boxPosEnd.y));
			metalBawx = new Rect (boxPosOrg.x, Screen.height - boxPosOrg.y, boxPosEnd.x - boxPosOrg.x, -1 * (boxPosEnd.y - boxPosOrg.y));
			GUI.DrawTexture (metalBawx, bawxTexture);
		}
	}

	private void GetDudes(bool shiftSelect = false) {
		if (!shiftSelect)
			selectedHeros = new List<GameObject> ();
//		metalBawx = new Rect (boxPosOrg.x, Screen.height - boxPosOrg.y, boxPosEnd.x - boxPosOrg.x, -1 * (boxPosEnd.y - boxPosOrg.y));
		foreach (var hero in heros) {
			Vector2 heroPos = Camera.main.WorldToScreenPoint (hero.transform.position);
			Vector2 convertedPos = new Vector2(heroPos.x, Screen.height - heroPos.y);
			if (metalBawx.Contains (convertedPos,true)) {
				if (!selectedHeros.Contains (hero))
//					selectedHeros.Add (hero);
					AddHeroToSelected(hero);
			}
		}
	}

	private void MouseInput() {
		if (Input.mousePosition.x > 0 && Input.mousePosition.x < screenEdgePixelSize) {
			playerMoveInput = playerMoveInput + Vector3.left;
		}
		if (Input.mousePosition.x < Screen.width && Input.mousePosition.x > Screen.width - screenEdgePixelSize) {
			playerMoveInput = playerMoveInput + Vector3.right;
		}

		if (Input.mousePosition.y > 0 && Input.mousePosition.y < screenEdgePixelSize) {
			playerMoveInput = playerMoveInput + Vector3.back;
		}
		if (Input.mousePosition.y < Screen.height && Input.mousePosition.y > Screen.height - screenEdgePixelSize) {
			playerMoveInput = playerMoveInput + Vector3.forward;
		}
	}

	private void CameraTarget() {
		if (following) {
			rotationTarget = cameraTarget.transform.position;
			return;
		}

		//Debug.DrawRay (this.transform.position, this.transform.forward*40, Color.green, 5f);
		int layerMask = 1 << 9;
		if (!following && playerMoveInput.magnitude > 0f) {
			RaycastHit hitInfo;
			Ray cameraRay = new Ray (this.transform.position, this.transform.forward);
			if (Physics.Raycast (cameraRay, out hitInfo, 60f, layerMask)) {
				rotationTarget = hitInfo.point;
			}
		}
	}

	private void CameraZoom() {
		if (Input.GetAxis("Mouse ScrollWheel") != 0f) {
			playerScrollInput = Input.GetAxis ("Mouse ScrollWheel");
			if (this.transform.position.y <= 4 && playerScrollInput > 0) {
				return;
			}
			if (this.transform.position.y >= 14 && playerScrollInput < 0) {
				return;
			}
			this.transform.Translate (Vector3.forward * Input.GetAxis ("Mouse ScrollWheel") * Time.deltaTime * 200, Space.Self);
		}
	}

	//method to try and get a unit on the unit layer.
	private void GetUnit() {
		Debug.Log ("getting unit");
		int layerMask = 1 << 8;
		RaycastHit hit;
		Ray cameraRay = Camera.main.ScreenPointToRay (Input.mousePosition);
		if (Physics.Raycast (cameraRay, out hit, 200f, layerMask)) {
			Debug.Log (hit.collider.name);
			Debug.Log (hit.collider.gameObject.layer);
			if (hit.collider.CompareTag("Hero")) {
//				cameraTarget = hit.collider.gameObject;
				selectedHeros.Add (hit.collider.gameObject);
			}
		}
	}

	//get a target for the controlled unit, either point on ground or enemy.
	private void GetTarget() {
		if (selectedHeros.Count == 0) {
			return;
		}
		int layerMask = (1 << 8) | (1 << 9) | (1 << 10);
		RaycastHit hit;
		Ray cameraRay = Camera.main.ScreenPointToRay (Input.mousePosition);
		if (Physics.Raycast (cameraRay, out hit, 200f, layerMask)) {
			Debug.Log (hit.collider.name);
			Debug.Log (hit.collider.gameObject.layer);

			if (hit.collider.gameObject.layer == 8) {
				foreach (var hero in selectedHeros) {
					hero.GetComponent<EntityTargetScript> ().targetedEntity = hit.collider.gameObject;
					hero.GetComponent<EntityNavigationScript> ().SetDestination (hit.point, this.gameObject);
				}
				destinationGizmos = new List<Vector3> ();
				destinationGizmos.Add (hit.point);
			} else if (hit.collider.gameObject.layer == 10) {
				foreach (var hero in selectedHeros) {
					hero.GetComponent<EntityTargetScript> ().targetedEntity = null;
				}
				selectedHeros [0].GetComponent<EntityStatisticsScript> ().InstigateCommand (EntityStatisticsScript.entityTargetContexts.Item, hit.collider.gameObject);

			} else {
				foreach (var hero in selectedHeros) {
					hero.GetComponent<EntityTargetScript> ().targetedEntity = null;
				}

				Vector3 heading = hit.point - selectedHeros [0].transform.position;
				heading.y = 0;
				float distance = heading.magnitude;
				Vector3 direction = heading / distance;

				Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

				heroDestinations = new List<Vector3> ();
				if (selectedHeros.Count % 2 == 0) {
					heroDestinations.Add (GetNavMeshSpot(hit.point,rotation,initialPoint) + rotation * Vector3.left * (selectedHeros.Count / 2 + 0.5f));
				} else {
					heroDestinations.Add (GetNavMeshSpot(hit.point,rotation,initialPoint) + rotation * Vector3.left * selectedHeros.Count / 2);
				}
				if (selectedHeros.Count >= 2)
					heroDestinations.Add (GetNavMeshSpot(heroDestinations[0], rotation, lineFormation));

				while (heroDestinations.Count < selectedHeros.Count) {
					heroDestinations.Add (GetNavMeshSpot (heroDestinations [heroDestinations.Count - 1], rotation, lineFormation));
				}
				destinationGizmos = heroDestinations;


				for (int i = 0; i < selectedHeros.Count; i++) {
					selectedHeros[i].GetComponent<EntityNavigationScript> ().SetDestination (heroDestinations[i], this.gameObject);
				}
			}
		}
	}

	private void AssignLootToAcquire() {
		if (selectedHeros.Count < 1) {
			return;
		}
		List<GameObject> itemsOnFloor = new List<GameObject> ();
		itemsOnFloor.AddRange(GameObject.FindGameObjectsWithTag("Items"));

		Debug.Log ("SHC: " + selectedHeros.Count + " : IFC: " + itemsOnFloor.Count);

		if (selectedHeros.Count <= itemsOnFloor.Count) {
			foreach (var hero in selectedHeros) {
				Debug.Log("Assigning " + hero.name + " to pick up " + itemsOnFloor[0].name + " at " + itemsOnFloor[0].transform.position);
				hero.GetComponent<EntityStatisticsScript> ().InstigateCommand (EntityStatisticsScript.entityTargetContexts.Item, itemsOnFloor [0]);
				itemsOnFloor.Remove (itemsOnFloor [0]);
			}
		} else {
			int lootCount = 0;
			int lootMax = itemsOnFloor.Count;
			while (lootCount < lootMax) {
				Debug.Log("Assigning " + selectedHeros[lootCount].name + " to pick up " + itemsOnFloor[0].name + " at " + itemsOnFloor[0].transform.position);
				selectedHeros [lootCount].GetComponent<EntityStatisticsScript> ().InstigateCommand (EntityStatisticsScript.entityTargetContexts.Item, itemsOnFloor [0]);
				itemsOnFloor.Remove (itemsOnFloor [0]);

				lootCount++;
			}
		}



	}

	private Vector3 GetNavMeshSpot(Vector3 parentPosition, Quaternion rotation, List<Vector3> targetDirections) {
		NavMeshHit navHit;
		foreach (var dir in targetDirections) {
			if (NavMesh.SamplePosition (parentPosition + rotation * dir, out navHit, 0.5f, NavMesh.AllAreas)) {
				return navHit.position;
			}
		}
		//if this fails, time for random position, so we try a few times to get a random position
		for (int i = 0; i < 10; i++) {
			Vector3 randomPoint = parentPosition + Random.insideUnitSphere * 3.0f;
			if (heroDestinations.TrueForAll (vec => vec.sqrMagnitude - randomPoint.sqrMagnitude > 1.0f)) {
				if (NavMesh.SamplePosition (randomPoint, out navHit, 0.5f, NavMesh.AllAreas)) {
					return navHit.position;
				}
			}
//			foreach (var vec in heroDestinations) {
//				if (vec.sqrMagnitude - randomPoint.sqrMagnitude < 1.0f^2) {
//					break;
//				}
//			}
		}
		//That also failed??! Then all hope is lost, return a zero vector and hope for the best!
		return Vector3.zero;

//		if (NavMesh.SamplePosition (parentPosition + rotation * Vector3.left, out navHit, 0.5f, NavMesh.AllAreas)) {
//			heroDestinations.Add(navHit.position);
//		} else {
//			canPlaceLeft = false;
//		}
	}

//	private static bool SufficientDistance(

	private void AddHeroToSelected(GameObject hero) {
		selectedHeros.Add (hero);
		StartCoroutine ("SelectHero", hero);
	}

	private IEnumerator SelectHero(GameObject hero) {
		hero.GetComponent<EntityStatisticsScript> ().SelectionToggle ();
		yield return new WaitUntil (() => !selectedHeros.Contains (hero));
		hero.GetComponent<EntityStatisticsScript> ().SelectionToggle ();
		yield return null;
	}

	void OnDrawGizmos() {
		if (destinationGizmos.Count > 0) {
			foreach (var point in destinationGizmos) {
				Gizmos.DrawSphere (point, 0.5f);
			}
		}
	}
}
