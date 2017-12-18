using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceCalibrator : MonoBehaviour {

	public GameObject optitrackPointPrefab;
//	public int calibPoint1_optitrackId;
//	public int calibPoint2_optitrackId;
	public GameObject optitrackSpace;
	public Vector3 hmdToOptitrackVector = Vector3.zero; 
	public GameObject calibPoint1;
	public GameObject calibPoint2;
	// Use this for initialization


	Transform hmdForTesting;
	void Start () {
//		optiPoint1 = new Transform ();
//		optiPoint2 = new Transform ();

//		referencePoint = new GameObject().transform;
//		referencePoint.parent = transform;
//		FindPosition ();
//		optiPoint1.parent = optitrackSpace.transform;
//		optiPoint2.parent = optitrackSpace.transform;

		hmdForTesting = GameObject.Find ("Camera (eye)").transform;
//		StartCoroutine (FindScale());
	}

	void OnEnable(){
//		SteamVR_Events.DeviceConnected
	}
	public void StartSync(GameObject avatar1, int id1){
		Debug.Log("started sync");

		calibPoint1.GetComponent<OptitrackRigidBody>().RigidBodyId = id1;
		calibPoint1.transform.SetParent (avatar1.transform);
		SyncPostition(avatar1.transform);

	}
	public void StartSync(GameObject avatar1, int id1, GameObject avatar2, int id2){
		Debug.Log("started sync");

		calibPoint1.GetComponent<OptitrackRigidBody>().RigidBodyId = id1;
		calibPoint2.GetComponent<OptitrackRigidBody>().RigidBodyId = id2;
		calibPoint1.transform.SetParent (avatar1.transform);
		calibPoint2.transform.SetParent (avatar2.transform);

		SyncPostition (avatar1.transform);
		SyncScale (avatar1.transform, avatar2.transform);
	}

	void SyncPostition(Transform avatar){
		Debug.Log ("sync pos");
		Vector3 optitrackPoint = Vector3.zero;
		Vector3 hmdPoint = Vector3.zero;
		foreach (Transform t in avatar) {
			if (t.tag == "OptitrackCalibrationPoint") {
				optitrackPoint = t.position;
				Debug.Log ("found optitrackpoint");
			}  
//			else if (t.tag == "HMDCalibrationPoint") {
//				hmdPoint = t.position;
//				Debug.Log ("foundHMDpoint");
//			}
		}
		hmdPoint = GameObject.FindGameObjectWithTag ("HMD").transform.Find ("Camera (eye)").position;
		if (optitrackPoint == Vector3.zero) {
			Debug.LogError ("could not find all the Optitrack CalibrationPoints on: " + avatar.name + " or the optitrackpoint is in the center");
			return;
		}
		if (hmdPoint == Vector3.zero) {
			Debug.LogError ("could not find all the Vive CalibrationPoints on: " + avatar.name);
			return;
		}

		Vector3 calibrationOffset = optitrackPoint - (hmdPoint + hmdToOptitrackVector);
		optitrackSpace.transform.Translate (new Vector3 (-calibrationOffset.x ,-calibrationOffset.y ,-calibrationOffset.z));







//		optitrackPoint = skeleton1.StreamingClient.GetLatestSkeletonState(skeleton1.SkeletonAssetName) //OptitrackStreamingClient.

//		optitrackSpace.transform.Translate (optitrackPoint - hmdPoint);
	}

	void SyncScale(Transform avatar1, Transform avatar2){
	
	}
	void Update(){
//		FindPosition ();
		Vector3 v3 = calibPoint1.transform.position - hmdForTesting.position;
		//Debug.Log("distance between optitrack and vive: " + Vector3.Distance(calibPoint1.transform.position, hmdForTesting.position + hmdToOptitrackVector) + " vector: " + v3);
		if (Input.GetKeyDown (KeyCode.F7)) {
//			StartSync ();
//			StartCoroutine (FindScale());
		}
	}
	// Update is called once per frame
	IEnumerator FindScale () {
		yield return new WaitForEndOfFrame();
	}
	//void 
}
