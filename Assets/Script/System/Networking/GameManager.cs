using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Photon.PunBehaviour
{	
	public delegate void AvatarJoinedEventHandeler ();
	public static event AvatarJoinedEventHandeler OnAvatarjoined;
	[HideInInspector]public static GameManager Instance;
	[HideInInspector]public GameObject localAvatar;
	[HideInInspector]public GameObject remoteAvatar;
	[HideInInspector]public GameObject cameraManager;
	[SerializeField]private List<PlayerManager> _avatars;
	public List<PlayerManager> avatars{
		get{ 
			if (_avatars == null) {
				_avatars = new List<PlayerManager> ();
			}
			_avatars.RemoveAll (item => item == null);
			return _avatars;
		}set{ 
			_avatars = value;
		}
	}
	[SerializeField]private List<GameObject> _hmdLessAvatars;
	public List<GameObject> hmdLessAvatars{
		get{ 
			if (_hmdLessAvatars == null) {
				_hmdLessAvatars = new List<GameObject> ();
			}
			_hmdLessAvatars.RemoveAll (item => item == null);
			return _hmdLessAvatars;
		}set{ 
			_hmdLessAvatars = value;
		}
	}
	private List<GameObject> _cameraClients = new List<GameObject>();
	[HideInInspector]public List<GameObject> cameraClients{
		get{ 
			_cameraClients.RemoveAll (item => item == null);
			return _cameraClients;
		}set{ 
			_cameraClients = value;
		}
	}
   
    void Awake(){
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy (gameObject);
		}

		avatars = new List<PlayerManager> ();

        // TBC... probably for post production
		if (SceneManager.GetActiveScene().name != "Launcher") {
			PhotonNetwork.offlineMode = true;
			Debug.LogWarning ("You are now in offline mode");
		}
	}

    void Start(){
        DontDestroyOnLoad(gameObject);
    }
	void OnEnable(){
		MenuManager.OnAddHmdLessAvatarButton += AddHmdLessAvatarFromAssetBundle;
	}
    void OnDisable(){
		MenuManager.OnAddHmdLessAvatarButton -= AddHmdLessAvatarFromAssetBundle;

        var udpsender = GetComponent<RemoteTrigger>().udpSender;
        var ct = GetComponent<RemoteTrigger>().controlThread;

        if (udpsender != null)
            udpsender.Close();

        if (ct != null)
        {
            ct.Abort();
        }
    }

    #region Photon Callbacks
    public override void OnJoinedRoom(){

		FindObjectOfType<OptitrackStreamingClient>().TriggerUpdateDefinitions();

        if (!PhotonNetwork.isMasterClient) {
            GameObject obj = InstantiateResource(PhotonNetwork.playerName);
            if (obj.tag == "Avatar"){
                SetupAvatar(obj);
            }
        }

        // TBC: get ping to sync with...
        // Need to be updated to use this value
        ExitGames.Client.Photon.Hashtable playerCustomProps = new ExitGames.Client.Photon.Hashtable();
        playerCustomProps["Ping"] = PhotonNetwork.GetPing();

        PhotonNetwork.player.SetCustomProperties(playerCustomProps);
    }


    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer){
        // Destroy the skeleton object created by OptitrackSkeletonAnimator
        GameObject obj = GameObject.Find("OptiTrack Skeleton - " + otherPlayer.NickName);
        if (obj != null)
        {
            Destroy(obj);
        }
    }

    public override void OnLeftRoom(){
        SceneManager.LoadScene(0);
    }
    #endregion

	public delegate void SceneAsyncDone();
	public static event SceneAsyncDone OnSceneAsyncDone;

	//Used in Launcher by newly joined clients to load the scene currently active on the host
	public void LoadCurrentScene(){
		photonView.RPC ("RPC_LoadCurrentScene", PhotonTargets.MasterClient);

	}
	//Used by any client to tell the master to switch scene which then talls all other clients to load the new scene
	public void LoadNewScene(string sceneName){
		photonView.RPC ("RPC_LoadScene", PhotonTargets.MasterClient, new object[]{ sceneName});
	}
	[PunRPC]
	void RPC_LoadCurrentScene(){
		photonView.RPC ("RPC_LoadScene", PhotonTargets.All, new object[]{ SceneManager.GetActiveScene ().name});
	}
	[PunRPC]
	void RPC_LoadScene(string sceneName){
		if (SceneManager.GetSceneByName (sceneName) == SceneManager.GetActiveScene ()) {
			return;
		}
		StartCoroutine (CheckLoadSceneAsync (sceneName));	
		if (PhotonNetwork.isMasterClient) {
			photonView.RPC ("RPC_LoadScene", PhotonTargets.Others, new object[]{ sceneName});
		}
	}
	IEnumerator CheckLoadSceneAsync(string sceneName){
		yield return SceneManager.LoadSceneAsync(sceneName);
		if (OnSceneAsyncDone != null) {
			OnSceneAsyncDone ();
		}
	}

	public void LeaveRoom(){
        PhotonNetwork.LeaveRoom();
    }
	   
	public GameObject InstantiateResource(string resourceName){
		GameObject g = PhotonNetwork.Instantiate (resourceName, new Vector3 (0, 0, 0), Quaternion.identity, 0);
		return g;
	}

    /// <summary>
    /// Add avatar to avatar list in Gamemanger
    /// Used in PlayerManager
    /// </summary>
    /// <param name="pm">PlayerManager component that the avatar has</param>
    public void AddAvatarToList(PlayerManager pm)
    {
        avatars.Add(pm);
        if (OnAvatarjoined != null)
        {
            OnAvatarjoined();
        }
    }

    /// <summary>
    /// Set up an avtar in OnJoinedRoom
    /// Including enable avatar UI, 
    /// </summary>
    /// <param name="obj"></param>
	public void SetupAvatar(GameObject obj){
		if (obj.GetPhotonView ().isMine) {
				
			GameObject[] avatarUi = GameObject.FindGameObjectsWithTag ("AvatarUI");
			foreach (GameObject go in avatarUi) {
				go.GetComponent<Canvas> ().enabled = true;
			}
			obj.GetComponent<PlayerManager> ().SetSkeletonName ();
			obj.GetComponent<PlayerManager> ().localOptitrackAnimator.enabled = true;
			obj.GetComponent<PlayerManager> ().remoteOptitrackAnimator.enabled = true;
		}
		
	}



    /// <summary>
    /// Only called on Master client
    /// Master client instantiate hmdless avatar over network
    /// Then set up avatar on all client including master client
    /// </summary>
    /// <param name="newAvatar">Name of the avatar resource</param>
    /// <param name="skeletonName">Skeleton name for OptitrackSkeletonAnimator</param>
	[PunRPC]
	public void RPC_AddHmdLessAvatar(string newAvatar, string skeletonName){
		GameObject hmdLessAvatar = InstantiateResource(newAvatar);
		photonView.RPC ("RPC_SetupHmdLessAvatar", PhotonTargets.AllBuffered, new object[]{hmdLessAvatar.name, skeletonName});
	}
	[PunRPC]
	void SetupHmdLessAvatar(string hmdLessAvatarName, string skeletonName){
		GameObject hmdLessAvatar = GameObject.Find (hmdLessAvatarName);
		hmdLessAvatars.Add (hmdLessAvatar);
		hmdLessAvatar.GetComponent<PlayerManager> ().remoteOptitrackAnimator.SkeletonAssetName = skeletonName;
		hmdLessAvatar.GetComponent<PlayerManager> ().remoteOptitrackAnimator.gameObject.SetActive (true);
		hmdLessAvatar.GetComponent<PlayerManager> ().remoteOptitrackAnimator.enabled = true;
		hmdLessAvatar.GetComponent<PlayerManager> ().isHmdLess = true;
		Transform parent = hmdLessAvatar.GetComponent<PlayerManager> ().remoteOptitrackAnimator.transform;
		ChangeLayerRecursively (parent);
		hmdLessAvatar.GetComponent<PlayerManager> ().DestroyHMDHead ();
		hmdLessAvatar.GetComponent<PlayerManager> ().localOptitrackAnimator.gameObject.SetActive (false);
		Destroy(hmdLessAvatar.GetComponent<PlayerManager> ().localOptitrackAnimator.gameObject);
		Destroy(hmdLessAvatar.GetComponent<PlayerManager> ().cameraRig);
		hmdLessAvatar.GetComponent<FacialController> ().enabled = false;
		DontDestroyOnLoad (hmdLessAvatar);
	}





//	void AddHmdLessAvatarFromAssetBundle(string newAvatarName, string skeletonName){
//		photonView.RPC ("RPC_AddHmdLessAvatarFromAssetBundles", PhotonTargets.MasterClient, new object[]{newAvatarName, skeletonName});
//	}
	void AddHmdLessAvatarFromAssetBundle(object source, AddHmdLessAvatarEventArgs args){
		Debug.Log ("hook: " + args.newAvatarName + " " + args.skeletonName);
		photonView.RPC ("RPC_AddHmdLessAvatarFromAssetBundle", PhotonTargets.MasterClient, new object[]{args.newAvatarName, args.skeletonName});
	}


	/// <summary>
	/// Only called on Master client
	/// Master client instantiate AvatarManager which then instantiates hmdless avatar over network
	/// Then set up avatar on all client including master client
	/// </summary>
	[PunRPC]
	public void RPC_AddHmdLessAvatarFromAssetBundle(string newAvatar, string skeletonName){
		GameObject hmdLessAvatar = InstantiateResource("AvatarFromAssetBundle");
		photonView.RPC ("RPC_SetupHmdLessAvatarFromAssetBundle", PhotonTargets.AllBuffered, new object[]{hmdLessAvatar.name, newAvatar, skeletonName});
	}
	[PunRPC]
	void RPC_SetupHmdLessAvatarFromAssetBundle(string hmdLessAvatarName, string newAvatarName, string skeletonName){

		AvatarManager am = GameObject.Find(hmdLessAvatarName).GetComponent<AvatarManager>();
		am.LoadHMDLessAvatarFromAssetBundle (newAvatarName, skeletonName, 1.7f);
		GameManager.Instance.hmdLessAvatars.Add (gameObject);


		//		hmdLessAvatar.GetComponent<PlayerManager> ().isHmdLess = true;
		//
		//		hmdLessAvatar.GetComponent<PlayerManager> ().DestroyHMDHead ();
		//
		//		Destroy(hmdLessAvatar.GetComponent<PlayerManager> ().cameraRig);

	}






	//VVVVVVVVVVV---- this should be removed from here and only used in avatarmanager
    /// <summary>
    /// Recursively change layer to 0.Default layer
    /// so that hmd less avatar can be seen by all clients
    /// </summary>
	void ChangeLayerRecursively(Transform parent){
		foreach(Transform t in parent){
			t.gameObject.layer = 0;
			ChangeLayerRecursively (t);
		}
	}

    /// <summary>
    /// Used in ManagerMenu
    /// To replace either HMD avatar or HMDless avatar
    /// </summary>
	public void ReplaceAvatar(GameObject oldAvatar, string newAvatarName, string skeletonName){
		oldAvatar.SetActive (false);
		localAvatar = null;
		PhotonNetwork.Destroy (oldAvatar);
		if (oldAvatar != null) {
			Destroy (oldAvatar);
		} 

		GameObject player = PhotonNetwork.Instantiate (newAvatarName, new Vector3 (0, 0, 0), Quaternion.identity, 0);

		if (player.GetPhotonView ().isMine) {
			
			var listener = player.GetComponent<PlayerManager>()._camera.GetComponent<AudioListener> ();
			if (listener != null) {
				listener.enabled = true;
			}
			GameObject[] avatarUi = GameObject.FindGameObjectsWithTag ("AvatarUI");
            // Only enable avatar UI if it is hmd avatar
            if (!player.GetComponent<PlayerManager>().isHmdLess) {
                foreach (GameObject go in avatarUi) {
                    go.GetComponent<Canvas>().enabled = true;
                }
            }
			
			player.GetComponent<PlayerManager> ().SetSkeletonName (skeletonName);
			
		}
	}
}
