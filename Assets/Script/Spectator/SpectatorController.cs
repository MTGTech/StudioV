using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorController : MonoBehaviour {

	[SerializeField]
	Camera cameraToActivateIfMine;
	// Use this for initialization
	void Start () {
		if (GetComponent<PhotonView> ().isMine) {
			cameraToActivateIfMine.enabled = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
