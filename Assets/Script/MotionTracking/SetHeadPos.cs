using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHeadPos : NetworkedPositionSmoothening {
	[HideInInspector] public PhotonView _playerPhotonView;

	private Transform CameraEye;
//	public Transform remoteCameraEye;
//	public float lerpRate = 42.0f;

    private Vector3 headToEyeVector;
	public Vector3 origHeadToEyeVector;
    private Vector3 origRotation;
	// Use this for initialization
	void Awake()
	{
		
	}
	void Start () {
		if (_playerPhotonView.isMine) {
			CameraEye = transform.parent.GetComponent<PlayerManager> ()._camera.transform;
			//origHeadToEyeVector = -transform.position;
			origHeadToEyeVector = new Vector3 (0, 0.118f, 0.15f);
			headToEyeVector = origHeadToEyeVector;
			origRotation = transform.localEulerAngles;

			//Scale HMD Head to 0 on local Avatars
			transform.localScale = new Vector3(0, 0, 0);
			foreach(Transform t in transform){
				t.gameObject.SetActive (false);
			}
		}

		//Why was this code commented out?
		/* 
		if (GameManager.Instance.localAvatar == null || GameManager.Instance.localAvatar == _playerPhotonView.gameObject) {
			// scale hmd head to 0 for every client except the Avatars with a remoteAvatar
			transform.localScale = new Vector3(0, 0, 0);
			foreach(Transform t in transform){
				t.gameObject.SetActive (false);
			}
		}*/
	}

	public override void Update () {
		if (_playerPhotonView.isMine) {
//			Vector3 newPos;
			Vector3 rot = CameraEye.localEulerAngles;
			headToEyeVector = Quaternion.Euler (rot.x, rot.y, rot.z) * headToEyeVector;

			newPos = CameraEye.position - headToEyeVector;

			transform.position = Vector3.Lerp (transform.position, newPos, Time.deltaTime * lerpRate);

			transform.localEulerAngles = origRotation;
			transform.Rotate (CameraEye.localEulerAngles);

			headToEyeVector = origHeadToEyeVector;
		} else {
			transform.position = Vector3.Lerp (transform.position, newPos, Time.deltaTime * lerpRate);
			transform.rotation = Quaternion.Lerp (transform.rotation, newRot, Time.deltaTime * lerpRate);
		}
	}



//	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
//	{
//		if (stream.isWriting )
//		{
//			stream.SendNext(transform.position);
//			stream.SendNext(transform.rotation);
//		}
//		else
//		{
//			newPos = (Vector3)stream.ReceiveNext();
//			newRot = (Quaternion)stream.ReceiveNext();
//		}
//	}
}
