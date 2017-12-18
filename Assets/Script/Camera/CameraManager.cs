using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonView))]
public class CameraManager : Photon.MonoBehaviour {

	[SerializeField] List<GameObject> cameras;	
	[SerializeField] Camera activeCamera;
	[SerializeField] GameObject activeCamParent;

	KeyCode[] keyCodes = {
		KeyCode.Alpha0,
		KeyCode.Alpha1,
		KeyCode.Alpha2,
		KeyCode.Alpha3,
		KeyCode.Alpha4,
		KeyCode.Alpha5,
		KeyCode.Alpha6,
		KeyCode.Alpha7,
		KeyCode.Alpha8,
		KeyCode.Alpha9
	};
	void Awake () {
		GameManager.Instance.cameraManager = gameObject;
		if (!photonView.isMine) {
			this.enabled = false;
		} 
		// Make sure there is only 1 CameraManager
		PhotonPlayer[] players = PhotonNetwork.playerList;
		int numOfCameraManager = 0;
		foreach (var p in players){
			if (p.NickName == "CameraManager"){
				numOfCameraManager += 1;
			}
		}
		if (numOfCameraManager > 1){
			Fallback.Instance.SendErrorMessage ("There already is a CameraManager, exit the room.");
			GameManager.Instance.LeaveRoom();
		}
	}

	void Update () {
		if (photonView.isMine) {
			//Updates the camera list
			if (Input.GetKeyDown (KeyCode.F4)) {
				AddAllCamerasInTheScene ();
			}
			//Input for camera selection
			for (int i = 0; i < keyCodes.Length; i++) {
				if (Input.GetKeyDown (keyCodes [i])) {
					SetActiveCamera (i -1);
				}
			}
		}
	}
	void SetActiveCamera(int i){
		if (photonView.isMine) {
			Camera cameraComponent;
			cameras.RemoveAll (item => item == null);
			if (i < 0 || i >= cameras.Count) {
				Debug.LogWarning ("Cant switch to camera slot " + (i + 1) + " because it was not found");
				return;
			}
			cameraComponent = cameras [i].GetComponentInChildren<Camera> ();
			if (activeCamera == cameraComponent) {
				Debug.LogWarning ("Camera already selected");
				return;
			}
			cameraComponent.enabled = true;

			if (activeCamera != null) {
				activeCamera.enabled = false;
				ResetCameraMarkerColor ();
				if (activeCamera.GetComponent<AudioListener> () != null) {
					activeCamera.GetComponent<AudioListener> ().enabled = false;
				}
			}
			if (cameras [i].GetComponent<AudioListener> () != null) {
				cameras [i].GetComponent<AudioListener> ().enabled = true;
			}
			activeCamera = cameras [i].GetComponentInChildren<Camera> ();
			activeCamParent = cameras [i];
			SetCameraMarkerColor ();
		}
	}


	public void SetCameraMarkerColor(){
		try {
			activeCamera.transform.parent.GetComponent<CameraMarker> ().SetAsSelected(true);
		} catch {
			activeCamera.GetComponent<CameraMarker> ().SetAsSelected(true);
		}
	}

	public void ResetCameraMarkerColor(){
		try {
			activeCamera.transform.parent.GetComponent<CameraMarker> ().SetAsSelected(false);
		} catch  {
			activeCamera.GetComponent<CameraMarker> ().SetAsSelected(false);
		}
	}

	void AddAllCamerasInTheScene(){
		cameras.RemoveAll (item => item == null);
		List<GameObject> cameraParents = new List<GameObject>();
		cameraParents.AddRange (GameObject.FindGameObjectsWithTag ("VirtualCamera"));
		cameraParents.AddRange (GameObject.FindGameObjectsWithTag ("CameraClient"));
		foreach (GameObject go in cameraParents) {
			if (!cameras.Contains (go)) {
				cameras.Add (go);
			}
		}
		SetActiveCamera (0);
	}
	public void AddCameraToManager(GameObject overviewCamParent){
		if (!cameras.Contains (overviewCamParent)) {
			cameras.Add (overviewCamParent);
			if (cameras.Count == 1) {
				SetActiveCamera (0);

			}
		} else {
			if (overviewCamParent == activeCamParent) {
				ActivateLastActiveCam ();
			}
		}
		cameras.RemoveAll (item => item == null);
	}

	void ActivateLastActiveCam(){
		int index = cameras.FindIndex(a => a == activeCamParent);
		SetActiveCamera (index);

	}
}
