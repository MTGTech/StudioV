using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGravity : MonoBehaviour {

	Rigidbody rb;
	[SerializeField] float force;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		rb.AddForce (Physics.gravity * force);
	}

	public void SetForce(float newForce){
		force = newForce;
	}
}
