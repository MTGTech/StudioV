using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Research.Unity.CodeExamples;

public class EyeRaycaster : MonoBehaviour {

	public float m_RayLength;
	public RaycastHit eyeHit;
	public Transform eyes;
	EyeRaycasterTarget target;

	void FixedUpdate () {
		var ray = new Ray(eyes.position, SubscribingToHMDGazeData.SubscribingInstance.GazeDirection);
		RaycastHit info;
		Physics.Raycast (ray, out info, m_RayLength);
		eyeHit = info;

		CheckHit ();
	}
	protected void CheckHit(){
		if (target != null) {
			if ( eyeHit.collider == null || target.gameObject != eyeHit.collider.gameObject) {
				RemoveTarget ();
				if (eyeHit.collider.GetComponent<EyeRaycasterTarget>() != null) {
					SetTarget (eyeHit.collider.GetComponent<EyeRaycasterTarget>());
				}
			}
		} else {
			if (eyeHit.collider != null && eyeHit.collider.GetComponent<EyeRaycasterTarget>() != null) {
				SetTarget (eyeHit.collider.GetComponent<EyeRaycasterTarget>());
			}
		}
	}
	void RemoveTarget(){
		target.LookExit ();
		target = null;
	}
	void SetTarget(EyeRaycasterTarget target){
		this.target = target;
	}
}
