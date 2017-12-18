using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleAdjust : MonoBehaviour {
    public Transform eyeLevel;
    public float manualTargetHeight;


    public PlayerManager _playerManager;
    private PhotonView _photonView; //Not needed when handled by AvatarManager?
    private Transform CameraRig;

    private float targetHeight;    			// Height of actor
    private float originalHeight;           // Original height of avatar
    private float headTipToEye = 0.108f;    // Dimenstion for average head

    void Awake(){
        originalHeight = eyeLevel.position.y;

        if (_photonView == null){            
            _playerManager = GetComponent<PlayerManager>();
			_photonView = _playerManager.photonView;
         
        }
    }

	public Vector3 SetScale(float targetHeight){
		targetHeight = targetHeight - headTipToEye;

		originalHeight = eyeLevel.position.y;

		if (!_playerManager.isHmdLess) {
			CameraRig = _playerManager.cameraRig;
			// Set pillar as child of HMD
			GameObject[] scaleByViveSpace = GameObject.FindGameObjectsWithTag ("ScaledByVivespace");
			foreach (GameObject g in scaleByViveSpace) {
				g.transform.parent = CameraRig.transform;
			}
		}
		// scale down local avatar prefab
		transform.localScale *= targetHeight / originalHeight;
		return transform.localScale;
	}

    public void SetSpecialHeight(){
        Debug.Log("Setting special hight");
        transform.localScale = Vector3.one;
        manualTargetHeight = manualTargetHeight - headTipToEye;
        float scale = manualTargetHeight / originalHeight;

        // Set body size locally	
        transform.localScale *= scale;

        CameraRig.localScale = Vector3.one;           

        // Set Vive area locally
        CameraRig.localScale *= scale;

        // Scale HeadToEyeVector on HMDHead locally
        transform.parent.GetComponentInChildren<SetHeadPos>().origHeadToEyeVector = new Vector3(0, 0.118f, 0.15f) * scale;

        // Scale avatar prefab remotely
        _playerManager.SetSpecialScale(transform.localScale);

    }
}
