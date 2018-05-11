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

	private float horizontalMove, verticalMove;

	private Vector3 playerMoveInput, rotationTarget;
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
		horizontalMove = Input.GetAxis("Horizontal");
		verticalMove = Input.GetAxis("Vertical");

		playerMoveInput = new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical"));

		Debug.DrawRay (this.transform.position, this.transform.forward*100, Color.green, 5f);
		if (!following && playerMoveInput.magnitude > 0f) {
			RaycastHit hitInfo;
			Ray cameraRay = new Ray (this.transform.position, this.transform.forward);
			if (Physics.Raycast (cameraRay, out hitInfo, 200f, layerMask)) {
				rotationTarget = hitInfo.point;
				Debug.Log (hitInfo.point);
			}
		}

		//Rotation adjusted local movement
		Vector3 adjustedVector;
		adjustedVector = Quaternion.Euler (0, this.transform.eulerAngles.y, 0) * new Vector3 (horizontalMove, 0, verticalMove);
		this.transform.Translate (adjustedVector * Time.deltaTime * 5, Space.World);


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

		if (Input.GetAxis("Mouse ScrollWheel") != 0f) {
			Debug.Log (Input.GetAxis ("Mouse ScrollWheel"));
			this.transform.Translate (Vector3.forward * Input.GetAxis ("Mouse ScrollWheel") * Time.deltaTime * 200, Space.Self);
			//this.transform.forward * Input.GetAxis("Mouse ScrollWheel");
		}


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

	void LateUpdate()
	{
		lastPosition = transform.position;
	}
}
