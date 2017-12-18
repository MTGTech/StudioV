using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeBallPhysics : MonoBehaviour {
	Rigidbody rb;
	public float radius;
	public GameObject contactVisualiser;
	bool grounded;
	public Vector3 angularVelocity;

	void Awake(){
		rb = GetComponent<Rigidbody> ();
	}
	void OnCollisionEnter(Collision col){
		UpdateAngularVelocity ();
		if (col.gameObject.tag == "SceneCollider") {
			grounded = true;
		} else {
			
		}
		StartCoroutine ("VisualiseContact");
	}
	void Update(){
		angularVelocity = rb.angularVelocity;

		if (Input.GetKeyDown (KeyCode.R)) {
//			rb.angularVelocity = new Vector3 (100,0,0);
			rb.AddTorque(1000,0,0);
		}
	}
	void UpdateAngularVelocity(){
//		rb.angularVelocity = rb.velocity / radius;
		Debug.Log ("1: " + rb.angularVelocity + " V: " + rb.velocity);
		rb.angularVelocity = rb.velocity * radius;
		Debug.Log ("2: " + rb.angularVelocity + " V: " + rb.velocity);
	}
	void OnCollisionExit(Collision col){
		if (col.gameObject.tag == "SceneCollider") {
			grounded = false;
		}
	}
	IEnumerator VisualiseContact(){
		GameObject go = Instantiate (contactVisualiser, transform.position, Quaternion.identity) as GameObject;
		yield return new WaitForSeconds (2);
		Destroy (go);
	}
}
