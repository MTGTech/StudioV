using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCalibrationsManager : MonoBehaviour {
	public GameObject visionBlocker;
	public Transform calibrationCanvas;
	GameObject avatar;

	void Update () {
		//Toggle on the visionblocker
		if (Input.GetKeyDown (KeyCode.F5)) {
            if (avatar == null) {
                AssignLocalAvatarForVisionBlocker();
            }           
            
			visionBlocker.SetActive (!visionBlocker.activeInHierarchy);
			calibrationCanvas.gameObject.SetActive (visionBlocker.activeInHierarchy);
			
		}
	}

    /// <summary>
    /// Assign calibration objects as children of hmd camera, 
    /// to make vision blocker follow avatars' heads.
    /// Should be triggered on backpacks
    /// </summary>
    void AssignLocalAvatarForVisionBlocker()
    {

        this.avatar = GameManager.Instance.localAvatar;
        Transform cameraTransform;
        cameraTransform = avatar.GetComponent<PlayerManager>()._camera.transform;
        transform.parent = cameraTransform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
}
