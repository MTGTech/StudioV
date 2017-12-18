using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeVelocity : MonoBehaviour {
	Rigidbody rb;
	Transform initialPosition;
	Transform initialParent;
	public float stiffness = 3000;
	public bool isAvatar;

	public Collider[] collidersToIgnore;

	// Use this for initialization
	void Awake () {
		rb = GetComponent<Rigidbody> ();
		prevPosition = transform.position;

		initialParent = transform.parent;
		initialPosition = new GameObject ().transform;
		initialPosition.position = transform.position;
		initialPosition.rotation = transform.rotation;
		initialPosition.parent = initialParent;
		if (!isAvatar) {
			transform.parent = null;
		}


		rb.isKinematic = false;
		rb.mass = 100;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
	}
	void Start(){
		GameObject[] _sceneColliders = GameObject.FindGameObjectsWithTag ("SceneColliders");
		List<Collider> sceneColliders = new List<Collider> ();
		foreach (GameObject g in _sceneColliders) {
			if (g.GetComponent<Collider> () != null) {
				sceneColliders.Add (g.GetComponent<Collider>());
			}
		}
		IgnoreColliders (sceneColliders);
	}
	Vector3 prevPosition;
	Vector3 velocity;

	void Update () {
		
		rb.velocity = (initialPosition.position - transform.position) * Time.deltaTime * stiffness;
		rb.rotation = initialPosition.rotation;

		prevPosition = transform.position;

		if(Input.GetKeyDown(KeyCode.F3)){
			IgnoreColliders();
		}
	}
	public void IgnoreColliders(List<Collider> colliders){
		foreach(Collider col in colliders){
			Physics.IgnoreCollision (col, GetComponent<Collider> ());
			if (!isAvatar) {
				foreach(Transform t in transform){
					if (t.GetComponent<Collider> () != null) {
						Physics.IgnoreCollision (col, t.GetComponent<Collider> ());
					}
				}
			}
		}
	}
	public void IgnoreColliders(){
		foreach(Collider col in collidersToIgnore){
			Physics.IgnoreCollision (col, GetComponent<Collider> ());
			if (!isAvatar) {
				foreach(Transform t in transform){
					if (t.GetComponent<Collider> () != null) {
						Physics.IgnoreCollision (col, t.GetComponent<Collider> ());
					}
				}
			}
		}
	}
//	void OnCollisionEnter(Collision coll){
//		Rigidbody rb = coll.gameObject.GetComponent<Rigidbody> ();
//		if (rb != null) {
//			rb.velocity += this.velocity;
//		}
//	}
}
