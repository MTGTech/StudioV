using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncAvatarToHMD : Photon.PunBehaviour {

	public Transform headLower;
	public Transform neckUpper;
    public float speed = 40f;

    private float startTime;
	private float journeyLength;

	// Lerp between headLower and neckUpper;



    // Use this for initialization
	void Start () {

		startTime = Time.time;

	}
	
	// Update is called once per frame
	void Update () {

	    journeyLength = (headLower.position - neckUpper.position).magnitude;

		if ( journeyLength > 0.0001f) {

			float distCovered = (Time.time - startTime) * speed;
			float fracJourney = distCovered / journeyLength;

			Vector3 diffVector = headLower.position - neckUpper.position;
		    transform.position = Vector3.Lerp (transform.position, transform.position + diffVector, fracJourney);
		}

	}	
    	
}
