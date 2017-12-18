using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//calls event when activated, this is used for timeline function activation.
public class CameraSwitchTrigger : MonoBehaviour {
	PlaybackController pc;

	void Awake(){
		pc = GetComponentInParent<PlaybackController> ();
	}

	void OnEnable () {
		pc.GoToNextCam ();
	}
}
