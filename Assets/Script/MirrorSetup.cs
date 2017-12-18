using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorSetup : MonoBehaviour {
	public Vector3 startPosition;
	public Vector3 startRotation;
	// Use this for initialization
	void Start () {
		if (GameManager.Instance.localAvatar == null) {
			gameObject.SetActive (false);
			return;
		}
		GameManager.Instance.localAvatar.GetComponent<PlayerManager> ().remoteOptitrackAnimator.gameObject.SetActive (true);
		if (GameManager.Instance.remoteAvatar != null) {
			GameManager.Instance.remoteAvatar.GetComponent<PlayerManager> ().remoteOptitrackAnimator.gameObject.SetActive (true);
		}
		transform.position = startPosition;
		transform.rotation = Quaternion.Euler (startRotation);
	}
}
