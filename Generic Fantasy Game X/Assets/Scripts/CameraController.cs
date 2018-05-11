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
	bool following = true;
	Vector3 lastPosition;

	private float horizontalMove, verticalMove;

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


		Debug.Log ("the forward of the camera" + this.transform.forward);
		//Debug.Log ("the intended force to apply" + new Vector3 (horizontalMove, 0, verticalMove));

		//test solution
		Debug.Log ("Trying to get a local vector in global space" + new Vector3 (this.transform.forward.x, 0, 0));
		//Debug.Log (this.transform.rotation.eulerAngles);

		Vector3 adjustedVector;
		adjustedVector = Quaternion.Euler (0, this.transform.eulerAngles.y, 0) * new Vector3 (horizontalMove, 0, verticalMove);
		Debug.Log (adjustedVector);

		//Debug.Log (Vector3.RotateTowards (new Vector3 (verticalMove, 0, horizontalMove), new Vector3 (this.transform.forward.x, 0, this.transform.forward.z), 100f, 1f));


		this.transform.Translate (adjustedVector * Time.deltaTime * 5, Space.World);



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
//		if(Input.GetKey(KeyCode.Q))
//		{
//			rotate = -1;
//		} 
//		else if(Input.GetKey(KeyCode.E))
//		{
//			rotate = 1;
//		} 
//		else
//		{
//			rotate = 0;
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
