using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfHole : MonoBehaviour {
	GolfManager gm;
	void Start(){
		gm = GameObject.Find ("GolfManager").GetComponent<GolfManager> ();
	}
	void OnTriggerEnter(Collider col){
		gm.Resetball ();
	}
}
