using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfManager : MonoBehaviour {

	public Transform golfball;
	Vector3 ballStartPoint;

	void Start(){
		ballStartPoint = golfball.position;
	}
	// Update is called once per frame
	public void Resetball () {
		golfball.position = ballStartPoint;
		golfball.GetComponent<Rigidbody> ().velocity = Vector3.zero;
		golfball.GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
	}
	void Update(){
		if (Input.GetKeyDown (KeyCode.R)) {
			Resetball ();
		}
	}
}
