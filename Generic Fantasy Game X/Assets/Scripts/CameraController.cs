using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	GameObject cameraTarget;
	public float rotateSpeed;
	float rotate;
	public float offsetDistance;
	public float offsetHeight;
	public float smoothing;
	Vector3 offset;
	bool following = false;
	Vector3 lastPosition;

	private Vector3 playerMoveInput, rotationTarget, mousePosition, mouseMovement;
	private float playerScrollInput;
	//camera raycast
	private int layerMask = 1 << 9;

	void Start()
	{
		cameraTarget = GameObject.FindGameObjectWithTag("Hero");
		lastPosition = new Vector3(cameraTarget.transform.position.x, cameraTarget.transform.position.y + offsetHeight, cameraTarget.transform.position.z - offsetDistance);
		offset = new Vector3(cameraTarget.transform.position.x, cameraTarget.transform.position.y + offsetHeight, cameraTarget.transform.position.z - offsetDistance);

	}

	void Update()
	{
		playerMoveInput = new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical"));
		mouseMovement = Vector3.zero;

		Debug.DrawRay (this.transform.position, this.transform.forward*40, Color.green, 5f);
		if (!following && playerMoveInput.magnitude > 0f) {
			RaycastHit hitInfo;
			Ray cameraRay = new Ray (this.transform.position, this.transform.forward);
			if (Physics.Raycast (cameraRay, out hitInfo, 60f, layerMask)) {
				rotationTarget = hitInfo.point;
				Debug.Log (hitInfo.point);
			}
		}

		//Rotation adjusted local movement
		Vector3 adjustedVector;
		adjustedVector = Quaternion.Euler (0, this.transform.eulerAngles.y, 0) * playerMoveInput;
		this.transform.Translate (adjustedVector * Time.deltaTime * 5, Space.World);

		//Mouse position
		EdgeMovement ();

		if (Input.GetKey(KeyCode.Q)) {
			rotate = -1;
			this.transform.RotateAround (rotationTarget, Vector3.up, 20 * Time.deltaTime);
		} 
		else if (Input.GetKey(KeyCode.E)) {
			rotate = 1;
			this.transform.RotateAround (rotationTarget, Vector3.down, 20 * Time.deltaTime);
		} 
		else {
			rotate = 0;
		}

		CameraZoom ();


//		if(Input.GetKey(KeyCode.F))
//		{
//			if(following)
//			{
//				following = false;
//			} 
//			else
//			{
//				following = true;
//			}
//		} 

//		if(following)
//		{
//			offset = Quaternion.AngleAxis(rotate * rotateSpeed, Vector3.up) * offset;
//			transform.position = cameraTarget.transform.position + offset; 
//			transform.position = new Vector3(Mathf.Lerp(lastPosition.x, cameraTarget.transform.position.x + offset.x, smoothing * Time.deltaTime), 
//				Mathf.Lerp(lastPosition.y, cameraTarget.transform.position.y + offset.y, smoothing * Time.deltaTime), 
//				Mathf.Lerp(lastPosition.z, cameraTarget.transform.position.z + offset.z, smoothing * Time.deltaTime));
//		} 
//		else
//		{
//			transform.position = lastPosition; 
//		}
//		transform.LookAt(cameraTarget.transform.position);
	}

	private void EdgeMovement() {
		//mousePosition = Camera.main.WorldToViewportPoint (Input.mousePosition);
		Debug.Log (Input.mousePosition);

		//Debug.Log (mousePosition);
		mousePosition = Input.mousePosition;
		Vector3 adjustedVector;

		if (mousePosition.x < 20) {
			mouseMovement = mouseMovement + Vector3.left;
		}
		if (mousePosition.x > Screen.width - 20) {
			mouseMovement = mouseMovement + Vector3.right;
		}

		if (mousePosition.y < 20) {
			mouseMovement = mouseMovement + Vector3.back;
		}
		if (mousePosition.y > Screen.height - 20) {
			mouseMovement = mouseMovement + Vector3.forward;
		}

		adjustedVector = Quaternion.Euler (0, this.transform.eulerAngles.y, 0) * mouseMovement;

		this.transform.Translate (adjustedVector * Time.deltaTime * 5, Space.World);
		//adjustedVector = Quaternion.Euler (0, this.transform.eulerAngles.y, 0) * playerMoveInput;
	}

	private void CameraZoom() {
		if (Input.GetAxis("Mouse ScrollWheel") != 0f) {
			Debug.Log (Input.GetAxis ("Mouse ScrollWheel"));
			playerScrollInput = Input.GetAxis ("Mouse ScrollWheel");
			if (this.transform.position.y <= 4 && playerScrollInput > 0) {
				return;
			}
			if (this.transform.position.y >= 14 && playerScrollInput < 0) {
				return;
			}
			this.transform.Translate (Vector3.forward * Input.GetAxis ("Mouse ScrollWheel") * Time.deltaTime * 200, Space.Self);
			//this.transform.forward * Input.GetAxis("Mouse ScrollWheel");
		}
	}

	void LateUpdate()
	{
		lastPosition = transform.position;
	}
}
