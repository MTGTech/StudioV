using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    private Transform expressionController;

	public bool isHmdLess;


	[HideInInspector] public PhotonView photonView; //Set from AutomaticAvatarSetup. Component is on AvatarManager GameObject
	[HideInInspector] public GameObject _camera;
	[HideInInspector] public Transform cameraRig;

    [Header("Local")]
    public OptitrackSkeletonAnimator localOptitrackAnimator; //used in gamemanager to set skeleton name on all clients
	private AudioListener listener;

    [Header("Remote")]
    public OptitrackSkeletonAnimator remoteOptitrackAnimator; //used in gamemanager to set skeleton name on all clients
//    public Transform OptitrackHead;

    void Awake() {
		gameObject.name += photonView.viewID.ToString ();

		remoteOptitrackAnimator.gameObject.SetActive (true);

		expressionController = FindObjectOfType<FaceExpressionController>().transform;
		GameManager.Instance.AddAvatarToList(this);

//        SetParent(transform); <- AvatarManager sets parent instead

        // If it's local player and not a hmdless avatar
        // Set local avatar and get avatar camera and audio listener
		if (photonView.isMine && !PhotonNetwork.isMasterClient) {
			localOptitrackAnimator.gameObject.SetActive (true);

			GameManager.Instance.localAvatar = gameObject;
			Debug.Log ("Activating camerarig");

			if (cameraRig == null) {
				FindCameraRig ();
			}

			try {
				_camera = cameraRig.Find ("Camera (head)").Find ("Camera (eye)").gameObject;
				listener = cameraRig.Find ("Camera (head)").Find ("Camera (ears)").GetComponent<AudioListener> ();
			} catch (System.Exception) {
				_camera = cameraRig.Find ("Camera (eye)").gameObject;
				listener = cameraRig.Find ("Camera (eye)").Find ("Camera (ears)").GetComponent<AudioListener> ();
			}

			_camera.GetComponent<SteamVR_Camera> ().enabled = true;
			_camera.GetComponent<Camera> ().enabled = true;
			listener.enabled = true;

//			localOptitrackAnimator.enabled = false;
//			remoteOptitrackAnimator.enabled = false;

			// Set optitrack avatar to layer "Remote"
			// Not necessary if remoteavatar is already set to layer Remote in the prefab..
			SetLayerTo (this, "Remote");

		} else if (!photonView.isMine && !PhotonNetwork.isMasterClient) {
			GameManager.Instance.remoteAvatar = gameObject;
		} else if (photonView.isMine && PhotonNetwork.isMasterClient) {
		} else if (!PhotonNetwork.isMasterClient) {
			
		}
    }
	public void FindCameraRig(){
		cameraRig = GameObject.FindGameObjectWithTag ("HMD").transform;
	}


    /// <Summary>
    /// This method is only called by HMD Avatars
    /// Set remote avatar layer
    /// </Summary>
    public void SetLayerTo(PlayerManager playerManager, string layer){
        //playerManager.localOptitrackAnimator.gameObject.SetActive(true);
        //playerManager.remoteOptitrackAnimator.gameObject.SetActive(false);
        foreach (Transform t in playerManager.remoteOptitrackAnimator.transform){
            if (t.gameObject.activeInHierarchy){
                t.gameObject.layer = LayerMask.NameToLayer(layer);
            }
        }
    }

    /// <summary>
    /// Used for setup avatar onJoinedRoom() in GameManager
    /// </summary>
    public void SetSkeletonName(){
        if (photonView.isMine){
            photonView.RPC("RPC_SetSkeletonName", PhotonTargets.AllBuffered, new object[]{
                gameObject.name,
                PlayerPrefs.GetString("SkeletonName")
            });
        }
    }

    /// <summary>
    /// Used for replace avatar during game play
    /// </summary>
    public void SetSkeletonName(string skeletonName){
        photonView.RPC("RPC_SetSkeletonName", PhotonTargets.AllBuffered, new object[]{
            gameObject.name,
            skeletonName
        });
    }

    [PunRPC]
    public void RPC_SetSkeletonName(string gameObjectName, string skeletonName){
        PlayerManager pm = GameObject.Find(gameObjectName).GetComponent<PlayerManager>();
        pm.localOptitrackAnimator.SkeletonAssetName = skeletonName;
        pm.remoteOptitrackAnimator.SkeletonAssetName = skeletonName;
        //Enable the components in case setting the skeletonName was too late
        pm.localOptitrackAnimator.enabled = true;
        pm.remoteOptitrackAnimator.enabled = true;
    }


    void SetParent(Transform player){      
        player.parent = expressionController.transform;     
    }


    // TODO: should be called in setup avatar
    public void SetToActorScale(Vector3 localScale){
        photonView.RPC("RPC_SetToActorScale", PhotonTargets.OthersBuffered, new object[] {gameObject.name, localScale});
    }

    //TODO: hmd is no longer a child of avatar
    [PunRPC]
    public void RPC_SetToActorScale(string playerName, Vector3 localScale){
		Debug.Log("Setting scale of " + playerName + " on all clients to: " + localScale);

        // Get original global scale of hmd
        Transform hmd = GameObject.Find("[CameraRig]").transform;
        Vector3 scl = hmd.lossyScale;

        // Scale down avatar prefab
        GameObject.Find(playerName).transform.localScale = localScale;

        // Resize hmd to glocal scale (1,1,1)
        hmd.localScale *= scl.x / hmd.lossyScale.x;

    }

    public void SetSpecialScale(Vector3 localScale){
        photonView.RPC("RPC_SetSpecialScale", PhotonTargets.OthersBuffered, new object[] { gameObject.name, localScale });
    }

    [PunRPC]
    public void RPC_SetSpecialScale(string playerName, Vector3 localScale){
        Debug.Log("Setting scale of " + playerName + " on all clients to: " + localScale);
        GameObject.Find(playerName).transform.localScale = localScale;
        
        // set head offset for special scale
        GameObject.Find(playerName).GetComponentInChildren<SetHeadPos>().origHeadToEyeVector = new Vector3(0,0.118f,0.15f) * localScale.x;
    }

    /// <summary>
    /// Destroy HMD head for hmdless avatars
    /// </summary>
	public void DestroyHMDHead(){
		Destroy(GetComponentInChildren<SetHeadPos> ().gameObject);
	}



    //TODO: remake scale script
    //public void SetHeadScale(float scale){
    //    photonView.RPC("RPC_SetHeadScale", PhotonTargets.AllBuffered, new object[] {gameObject.name, scale});
    //}
    //[PunRPC]
    //public void RPC_SetHeadScale(string playerName, float scale){
    //    // Scale Optitrack head
    //    GameObject.Find(playerName).GetComponent<PlayerManager>().OptitrackHead.localScale =* scale;
    //    // Scale HMD head
    //    GameObject.Find(playerName).GetComponentInChildren<SetHeadPos>().transform.localScale = scale;
    //}

}
