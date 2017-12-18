using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeLookController : Photon.MonoBehaviour {

	//movement related varuables
	[Header("Movement settings")]
	[SerializeField] float stickDeadZone = 0.1f;
	[SerializeField] float movementSpeed = 2;
	[SerializeField] float rotationSpeed = 100;
	[SerializeField] float zSpeed = .5f;
	[SerializeField] float zoomSpeed = 1;

	[Header("Mouse settings")]
	[SerializeField] float mouseSensitivity = 30;
	[SerializeField] int smoothing = 10;
	[SerializeField] float maximumZoomFov;
	[SerializeField] float minimumZoomFov; 
	
	Camera curOverviewCam;

	//mouse related varuables
	float ymove;
	float xmove;
	int iteration = 0;
	float xaggregate = 0;
	float yaggregate = 0;
	bool lockRotation;

	void FixedUpdate () {
		if (!lockRotation) {
			MouseMovement ();
		}
	}
	void Start(){
		if (!GetComponent<PhotonView> ().isMine) {
			this.enabled = false;
		}
		ManagerMenu.OnOpenCloseMenu += SwitchLockRotation;
	}
	void Update(){
		CheckForInput ();
	}
	void CheckForInput(){
		if (Input.GetKeyDown (KeyCode.F8) && !ManagerMenu.Instance.menuCanvas.activeInHierarchy) {
			SetRotationLock (!lockRotation);
		}
		if (Input.GetAxis ("Horizontal") > 0.01f || Input.GetAxis ("Horizontal") < -0.01f || Input.GetAxis ("Vertical") > 0.01f || Input.GetAxis ("Vertical") < -0.01f) {
			transform.Translate (new Vector3 (Input.GetAxis ("Horizontal"), 0, Input.GetAxis ("Vertical")) * Time.deltaTime * movementSpeed);
		}if (Input.GetAxis ("Depth") < -stickDeadZone) {
			transform.Translate (new Vector3(0, -Time.deltaTime, 0) * movementSpeed);
		}if (Input.GetAxis ("Depth") > stickDeadZone) {
			transform.Translate (new Vector3(0, Time.deltaTime, 0) * movementSpeed);
		}if ((Input.GetAxis ("UpAxis") > -1 || Input.GetAxis ("DownAxis") > -1)) {
			transform.Translate (new Vector3 (0, (-(Input.GetAxis ("UpAxis") + 1) + (Input.GetAxis ("DownAxis") + 1)) * Time.deltaTime, 0) * zSpeed);
		}if (Input.GetAxis ("Pitch") > stickDeadZone || Input.GetAxis ("Pitch") < -stickDeadZone || Input.GetAxis("Jaw") > stickDeadZone || Input.GetAxis("Jaw") < -stickDeadZone) {
			transform.Rotate (new Vector3 (Input.GetAxis ("Pitch"),0, 0) * Time.deltaTime * rotationSpeed, Space.Self);
			transform.Rotate (new Vector3(0,Input.GetAxis("Jaw"),0) * Time.deltaTime * rotationSpeed,Space.World);
		}if (Input.GetAxis ("Roll") > stickDeadZone || Input.GetAxis("Roll") < -stickDeadZone) {
			transform.Rotate (new Vector3 (0,0,Input.GetAxis("Roll")) * Time.deltaTime * rotationSpeed, Space.Self);
		}if (Input.GetAxis ("ResetRoll") > stickDeadZone) {
			transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0);
		}if (Input.GetAxis ("Zoom") != 0) {
			Zoom (Input.GetAxis ("Zoom") * Time.deltaTime * zoomSpeed);
		}
	}

	void Zoom(float zoomPower){
		if(curOverviewCam == null){
			try{
				curOverviewCam = GetComponentInChildren<Camera>();
			}catch{
				Debug.LogError ("Could not find a camera to manipulate, aborting zoom");
				return;
			}
		}
		curOverviewCam.fieldOfView += zoomPower;
		curOverviewCam.fieldOfView = Mathf.Clamp (curOverviewCam.fieldOfView, minimumZoomFov, maximumZoomFov);
	}

	void SwitchLockRotation(){
		lockRotation = ManagerMenu.Instance.menuCanvas.activeInHierarchy;
	}

	public void SetRotationLock(bool b){
		lockRotation = b;
	}
		
	void MouseMovement(){
		float[] x = new float[smoothing];
		float[] y = new float[smoothing];

		// reset the aggregate move values

		xaggregate = 0;
		yaggregate = 0;

		// receive the mouse inputs

		ymove = Input.GetAxis("Mouse Y");
		xmove = Input.GetAxis("Mouse X");

		// cycle through the float arrays and lop off the oldest value, replacing with the latest

		y[iteration % smoothing] = ymove;
		x[iteration % smoothing] = xmove;

		iteration++;

		// determine the aggregates and implement sensitivity

		foreach (float xmov in x)
		{
			xaggregate += xmov;
		}

		xaggregate = xaggregate / smoothing * mouseSensitivity;

		foreach (float ymov in y)
		{
			yaggregate += ymov;
		}

		yaggregate = yaggregate / smoothing * mouseSensitivity;

		// turn the x start orientation to non-zero for clamp

		Vector3 newOrientation = transform.eulerAngles + new Vector3(-yaggregate, xaggregate, 0);


		// rotate the object based on axis input (note the negative y axis)

		transform.eulerAngles = newOrientation;
	}
}
