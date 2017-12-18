using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkedPositionSmoothening))]
[RequireComponent(typeof(FreeLookController))]
public class SphericalPanoramicCameraController : Photon.MonoBehaviour {

	[SerializeField]
	bool showCameraNameInHud = true;

	List<Camera> cameras;
	[HideInInspector]public bool activated;
	Camera activeCamera;
	int activeIndex;
	[SerializeField] Text cameraName;
	bool[] occupiedCameras;


	void Awake(){
		cameras = new List<Camera>();
		foreach(Transform t in transform){
			if (t.GetComponent<Camera> () != null) {
				cameras.Add (t.GetComponent<Camera> ());
				t.GetComponent<Camera> ().enabled = false;
			}
		}
		occupiedCameras = new bool[cameras.Count];
		try{
			GameObject.Find ("360RecorderTimeline").GetComponent<PlaybackController> ().SetCameraController (this);
		}catch{
			Debug.LogWarning ("Unable to find the PlaybackDirector");
		}
		if (!PhotonNetwork.inRoom) {
			Activate360Camera ();
			GetComponent<NetworkedPositionSmoothening> ().enabled = false;
			GetComponent<FreeLookController> ().SetRotationLock (true);
		}
	}
	public void Activate360Camera(){
		cameras [0].enabled = true;
		Debug.Log ("Activated camera " + cameras[0].transform.parent.name + " " + cameras [0].enabled);
		activated = cameras [0].enabled;
		if (cameras [0].enabled) {
			activeCamera = cameras [0];
			activeIndex = 0;
		} else {
			activeIndex = -1;
		}
	}
	void Update(){
		if (activated) {
			if (Input.GetKeyDown (KeyCode.RightArrow)) {
				GoToNextCamera ();
			}else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				if (activeIndex - 1 >= 0) {
					SetActiveCamera (activeIndex - 1);
				} else {
					SetActiveCamera (cameras.Count - 1);
				}
			}
		}
	}
	public void GoToNextCamera(){
		if (activeIndex + 1 < cameras.Count) {
			SetActiveCamera (activeIndex + 1);
		} else {
			SetActiveCamera (0);
		}
	}

	public void SetActiveCamera(int i){
        Coroutine coroutine = null;
        activeCamera.enabled = false;
		activeCamera = cameras [i];
		activeCamera.enabled = true;
		photonView.RPC ("RPC_SetOccupied", PhotonTargets.All, new object[]{ activeIndex, false });
		activeIndex = i;
        if (coroutine != null) {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(ShowCameraName());
		photonView.RPC ("RPC_SetOccupied", PhotonTargets.All, new object[]{ i, true });
	}
	[PunRPC]
	void RPC_SetOccupied(int id, bool occupied){
		occupiedCameras [id] = occupied;
	}
	IEnumerator ShowCameraName(){
		if (!showCameraNameInHud) {
			yield break;
		}
		cameraName.gameObject.SetActive (true);
		if (occupiedCameras [activeIndex]) {
			cameraName.color = Color.red;
		} else {
			cameraName.color = Color.black;
		}
		cameraName.text = activeCamera.gameObject.name;
		yield return new WaitForSeconds (2);
		cameraName.gameObject.SetActive (false);
	}
}
