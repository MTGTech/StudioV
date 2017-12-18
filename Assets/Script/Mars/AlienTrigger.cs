using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AlienTrigger : MonoBehaviour {

	void OnTriggerEnter(Collider col){
		if (col.GetComponent<AlienController> () != null) {
			col.GetComponent<AlienController> ().Stop ();
		}
	}
}
