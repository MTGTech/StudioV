using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollyTargetAssignment : MonoBehaviour {

	[SerializeField]
	Cinemachine.CinemachineVirtualCamera [] cmVcamTargets;
	// Use this for initialization
	void Start () {
		AssignCameraTarget ();
	}
	void OnEnable(){
		GameManager.OnAvatarjoined += AssignCameraTarget;
	}
	void OnDisable(){
		GameManager.OnAvatarjoined -= AssignCameraTarget;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.A)) {
			AssignCameraTarget ();
		}
	}

	void AssignCameraTarget(){

		GameObject[] avatars = GameObject.FindGameObjectsWithTag ("CameraLockPos");
		Debug.Log ("AssignCameraTarget: " + avatars.Length);
		if (avatars.GetLength (0) == 1) {
			foreach (Cinemachine.CinemachineVirtualCamera cmv in cmVcamTargets) {
				cmv.LookAt = avatars [0].transform;
			}
		} else {
			for (int i = 0; i < avatars.GetLength (0); i++) {
				cmVcamTargets [i].LookAt = avatars [i].transform;
			}
		}
	}
}
