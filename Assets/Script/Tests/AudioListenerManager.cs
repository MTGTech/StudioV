using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioListenerManager : Photon.PunBehaviour {

	private PhotonView view;
	private AudioListener listener;

	// Use this for initialization
	void Start () {
		

		view = GetComponent<PhotonView> ();
		if (view.isMine) {
			listener = GameObject.Find("Camera (ears)").GetComponent<AudioListener> ();
			listener.enabled = true;
		}

	}
}
