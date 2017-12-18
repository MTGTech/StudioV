using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadmintonBallMachine : MonoBehaviour {

	public GameObject badmintonBall;
	public float force;
	public float intervall;

	// Use this for initialization
	void Start () {
		StartCoroutine (SpawnBalls ());
	}
	IEnumerator SpawnBalls(){
		while(true){
			GameObject go = Instantiate (badmintonBall, transform.position + transform.forward, Quaternion.identity) as GameObject;
			go.GetComponent<Rigidbody> ().AddForce (transform.forward * force);
			yield return new WaitForSeconds (intervall);
		}
	}
}
