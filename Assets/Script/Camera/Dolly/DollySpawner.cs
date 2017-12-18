using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollySpawner : MonoBehaviour {

	PhotonView m_photonView;
	[SerializeField]
	GameObject dollySetupPrefab;
	// Use this for initialization
	void Start () {
		m_photonView = GetComponent<PhotonView> ();
		if (m_photonView.isMine && dollySetupPrefab != null) {
			GameObject go = Instantiate (dollySetupPrefab, Vector3.zero, Quaternion.Euler (Vector3.forward)) as GameObject;
			DontDestroyOnLoad (go);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
