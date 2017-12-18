using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DollyController : MonoBehaviour {

	CinemachineVirtualCamera cvc;

	// Use this for initialization
	void Awake () {
		cvc = GetComponent<CinemachineVirtualCamera> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
