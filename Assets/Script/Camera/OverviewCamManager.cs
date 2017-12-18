using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class OverviewCamManager : Photon.MonoBehaviour {

	public bool isCameraClient;
	Camera curOverviewCamera;

	void Start(){
		if (isCameraClient) {
			GameManager.Instance.cameraClients.Add (gameObject);
			GetAndApplySceneCamera();
		}

	}

	private void OnEnable(){
		GameManager.OnSceneAsyncDone += OnSceneLoaded;
	}

	private void OnDisable(){
		GameManager.OnSceneAsyncDone -= OnSceneLoaded;

	}

	[PunRPC]
	void RPC_AddToCameraManager(){
		if (GameManager.Instance.cameraManager == null) {
			return;
		}
		CameraManager cm = GameManager.Instance.cameraManager.GetComponent<CameraManager> ();
		cm.AddCameraToManager(gameObject);
		Debug.Log ("CameraClient - " + isCameraClient + " added to camera manager");
	}

	public void OnSceneLoaded(){
		Debug.Log ("loadedscene");
		GetAndApplySceneCamera ();
	}

	void GetAndApplySceneCamera(){
		Camera cameraToPickup;
		if (SceneManager.GetActiveScene ().name == "Launcher") {
			return;
		}
		Debug.Log ("Applying camera to client");
		if (curOverviewCamera != null) {
			curOverviewCamera.gameObject.SetActive (false);
			Destroy (curOverviewCamera.gameObject);
		}
		try {
			cameraToPickup = GameObject.FindGameObjectWithTag ("CameraClientPickup").GetComponent<Camera> ();
		} catch {
			Debug.LogError ("Stopping camera pickup, no overview camera with the tag: CameraClientPickup could be found in the scene.");
			return;
		}
		curOverviewCamera = Instantiate (cameraToPickup, Vector3.zero, Quaternion.identity).GetComponent<Camera> ();
		Debug.Log ("ApplyCam " + curOverviewCamera.name);
		curOverviewCamera.tag = "PickedUpCam";
		curOverviewCamera.transform.SetParent (transform);
		curOverviewCamera.transform.localPosition = Vector3.zero;
		curOverviewCamera.transform.localRotation = Quaternion.identity;
		if (photonView != null) {
			photonView.RPC ("RPC_AddToCameraManager", PhotonTargets.All, new object[]{ });
		}
		if (photonView.isMine) {
			if (PhotonNetwork.isMasterClient) {
				if (Camera.allCameras.Length < 1) {
					curOverviewCamera.enabled = true;
				}
			} else {
				curOverviewCamera.enabled = true;
			}
		}
	}
}