using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Teleporter : EyeRaycaster {
	public Transform player;
	public Transform hand;
	public Transform optitrackSpace;

	public float distance = 0.1f;
	public float teleportationWaitTime;
//	public float maxTeleportDistance = 100;
	public GameObject teleportationMarker;
	public Image fillIndicator;

	bool isTeleporting = false;

	RaycastHit hit;

	// Use this for initialization
	void Start () {
		
	}


	// Update is called once per frame
	void FixedUpdate () {
		CheckHit ();
		//Depending on the distance between the hand and the head a teleportation sequence will be initiated
		if (Vector3.Distance (hand.position, eyes.position) < distance) {
			teleportationMarker.SetActive (true);
			if (eyeHit.collider != null) {
				Debug.Log (eyeHit.collider.gameObject);
				if (eyeHit.collider.gameObject != teleportationMarker) {
					if (!isTeleporting) {
						teleportationMarker.transform.position = eyeHit.point;
						isTeleporting = true;
						StartCoroutine ("Teleporting");
					} else {
						isTeleporting = false;
						StopCoroutine ("Teleporting");
					}
				} else {
					
				}
			}
		} else {
			isTeleporting = false;
			teleportationMarker.SetActive (false);
		}

	}
	float t = 0;
	void Update(){
		if (isTeleporting) {
			fillIndicator.fillAmount = Mathf.Lerp (0, 1, t);
			if (t < 1) {
				t += Time.deltaTime / teleportationWaitTime;
			}
		} else {
			fillIndicator.fillAmount = 0;
			t = 0;
		}
		fillIndicator.transform.LookAt (eyes.position);
	}
	IEnumerator Teleporting(){
//		StartCoroutine ("FillIndicator");
		yield return new WaitForSeconds (teleportationWaitTime);
		if (isTeleporting) {
			Vector3 difference = teleportationMarker.transform.position - eyes.position;
			optitrackSpace.position += new Vector3(difference.x, 0, difference.z);
			player.position += new Vector3(difference.x, 0, difference.z);
		}
	}
}
