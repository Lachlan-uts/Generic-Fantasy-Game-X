﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    private Vector3 dragOrigin;
    private float screenHeight;
    private float screenWidth;
    private float horizontalMove;
    private float verticalMove;

	//camera
	[SerializeField]
	private Camera MCamera;
    [SerializeField]
    private float dragSpeed;
    [SerializeField]
    private float movementSpeed;

    //trying to make it so you can click to select a unit.
    public GameObject selectedObject { private set; get; }

    // Use this for initialization
    void Start () {
		MCamera = Camera.main;
        screenHeight = Screen.height;
        screenWidth = Screen.width;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("Fire1")) {
			GetUnit (MCamera);
		}
		if (Input.GetButtonDown ("Fire2")) {
			MoveCommand (MCamera);
		}

        /*if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Vector3 playerPos = GameObject.FindGameObjectWithTag("Hero").transform.position;
            this.transform.position = new Vector3(playerPos.x, 15, playerPos.z - 15);
            //transform.rotation.Set(40, 0, 0, 0);
        }*/
        //DragCamControl();

        KeyMovement();
    }

    private void DragCamControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(0)) return;

        Vector3 pos = Camera.main.ScreenToViewportPoint(dragOrigin - Input.mousePosition);
        Vector3 move = new Vector3(pos.x * dragSpeed, 0, pos.y * dragSpeed);

        transform.Translate(move, Space.World);

    }

	//method to try and get a unit on the unit layer.
	private void GetUnit(Camera camera) {
		int layerMask = 1 << 8;
		RaycastHit hit;
		Ray cameraRay = camera.ScreenPointToRay (Input.mousePosition);

		if (Physics.Raycast (cameraRay, out hit, 200f, layerMask)) {
			selectedObject = hit.collider.gameObject;
//			Debug.Log (selectedObject.gameObject.name);
		}
	}

	//telling a unit where to go.
	private void MoveCommand(Camera camera) {
		if (!selectedObject)
			return;
		int layerMask = 1 << 9;
		RaycastHit hit;
		Ray cameraRay = camera.ScreenPointToRay (Input.mousePosition);

		if (Physics.Raycast (cameraRay, out hit, 200f)) {
			selectedObject.GetComponent<EntityNavigationScript> ().SetDestination (hit.point,this.gameObject);
//			Debug.Log ("sending a move command");
		}
	}

    private void KeyMovement()
    {
        horizontalMove = Input.GetAxis("Horizontal");
        verticalMove = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalMove, 0, verticalMove).normalized;
        transform.Translate(movement * Time.deltaTime * movementSpeed, Space.World);
        //Debug.Log(movement);

    }
}
