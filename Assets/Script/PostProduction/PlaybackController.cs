using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaybackController : MonoBehaviour {

	public bool is360 = false;
	[SerializeField]
	SphericalPanoramicCameraController cameraController;
	// Use this for initialization
	void Start () {
		
	}

	void StartRecording(){
	
	}

	void StopRecording(){
	
	}

	public void SetCameraController(SphericalPanoramicCameraController controller){
		cameraController = controller;
	}

	public void GoToNextCam(){
		Debug.Log ("newCamSet");
		if(cameraController!= null)
		cameraController.GoToNextCamera ();
	}


}
 