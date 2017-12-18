using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPositionSmoothening : Photon.MonoBehaviour {
	public float lerpRate = 1.5f;

	bool isMine;
	protected Vector3 newPos;
	protected Quaternion newRot;

	public virtual void Update () {
		if (!isMine) {
			transform.position = Vector3.Lerp (transform.position, newPos, Time.deltaTime * lerpRate);
			transform.rotation = Quaternion.Lerp (transform.rotation, newRot, Time.deltaTime * lerpRate);
		}
	}
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
		if (stream.isWriting ){
            isMine = true;
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
		}
		else{
            isMine = false;
			newPos = (Vector3)stream.ReceiveNext();
			newRot = (Quaternion)stream.ReceiveNext();
		}
	}
}
