using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    private Vector3 dragOrigin;
    private float screenHeight;
    private float screenWidth;
    private float horizontalMove;
    private float verticalMove;
    private Vector3 moveDirection;

	//camera
	[SerializeField]
	private Camera MCamera;
    [SerializeField]
    private float dragSpeed;
    [SerializeField]
    private float movementSpeed;
    [SerializeField]
    private float rotationSpeed;

    //Used for EdgeHovering
    [SerializeField]
    private float denominator;
    [SerializeField]
    private float minHeight;
    [SerializeField]
    private float maxHeight;

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
        //DragCamControl(Camera camera);

        
    }

    void FixedUpdate()
    {
        horizontalMove = Input.GetAxis("Horizontal");
        verticalMove = Input.GetAxis("Vertical");

        XZMovement(MCamera, horizontalMove, verticalMove);
        EdgeMovement(MCamera);

        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
        {
            int direction = 1;
            if (Input.GetKey(KeyCode.Q))
            {
                direction = -1;
                CameraRotation(MCamera, direction);
            }
            else CameraRotation(MCamera, direction);
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            CameraZoom(MCamera, Input.GetAxis("Mouse ScrollWheel"));
        }
    }

    private void DragCamControl(Camera camera)
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(0)) return;

        Vector3 pos = camera.ScreenToViewportPoint(dragOrigin - Input.mousePosition);
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

    private void CameraZoom(Camera camera, float z)
    {
        Vector3 movement = new Vector3(0, 0, z).normalized;

        //Limit Zoom between minHeight and maxHeight
        if (z <= 0f && camera.transform.position.y <= maxHeight)
        {   
            camera.transform.Translate(movement * Time.deltaTime * rotationSpeed, Space.Self);
        }
        if (z >= 0f && camera.transform.position.y >= minHeight)
        {
            camera.transform.Translate(movement * Time.deltaTime * rotationSpeed, Space.Self);
        }
    }

    private void CameraRotation(Camera camera, int dir)
    {
        float rotation = Time.deltaTime * rotationSpeed * dir;
        this.transform.Rotate(0, rotation, 0, Space.World);
    }

    private void XZMovement(Camera camera, float x, float z)
    {
        moveDirection = (transform.forward * z) + (transform.right * x);
        moveDirection = moveDirection.normalized;
        this.transform.Translate(moveDirection * Time.deltaTime * movementSpeed, Space.World);
    }

    private void EdgeMovement(Camera camera)
    {
        Vector3 mousePos = Input.mousePosition;
        float z = ((2 * mousePos.y) - screenHeight) / screenHeight;
        if (mousePos.y < screenHeight && mousePos.y > 0)
        {
            if (mousePos.x >= screenWidth - (screenWidth / denominator) && mousePos.x <= screenWidth)
            {
                float x = mousePos.x / screenWidth;
                Debug.Log(x);
                XZMovement(camera, x, z);
            }
            else if (mousePos.x <= (screenWidth / denominator) && mousePos.x >= 0)
            {
                float x = ((screenWidth / denominator) - mousePos.x) / (screenWidth / denominator);
                XZMovement(camera, -x, z);
            }
            else if (mousePos.y > screenHeight - screenHeight / denominator 
                || mousePos.y < screenHeight / denominator)
            {
                float x = (mousePos.x - screenWidth/2) / (screenWidth/2);
                XZMovement(camera, x, z);
            }
        }
    }
}
