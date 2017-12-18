using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class ManagerMenu : Photon.MonoBehaviour {

	public static ManagerMenu Instance { get; internal set;}


	public delegate void OpenCloseMenu();
	public static event OpenCloseMenu OnOpenCloseMenu;

	private GameManager gameManager;
    private RecordingManager _recordingManager;

	void Awake(){
		Instance = this;
		gameManager = GetComponent<GameManager> ();
	    _recordingManager = FindObjectOfType<RecordingManager>();

	}

	[Header("Menu")]
	public GameObject menuCanvas;
	public Dropdown replaceAvatarDropdown;
	public Dropdown removeAvatarDropdown;
	public Dropdown rigidbodiesDropdown;
	public Dropdown deleteRigidbodiesDropdown;
	public Text newRigidbodyId;
	public Text newOptitrackAvatarSkeletonName;
	[HideInInspector]public GameObject[] rigidbodies;
	private PlayerManager[] avatars;

//	void Update(){
//		if (Input.GetKeyDown (KeyCode.Escape)) {
//			menuCanvas.SetActive (!menuCanvas.activeInHierarchy);
//			RefreshAvatarList ();
//			RefreshRigidbodiesList ();
//			foreach(PlayerManager pm in GameManager.Instance.avatars){
//				if (pm.photonView.isMine) {
//					return;
//				}
//			}
//			if (OnOpenCloseMenu != null) {
//				OnOpenCloseMenu ();
//			}
//			Cursor.visible = menuCanvas.activeInHierarchy;
//			spaceCalibrationCanvas.SetActive (false);
//		}
//	}

	//------------------------------------------AVATARS------------------------------------------
	public void RefreshAvatarList(){
		replaceAvatarDropdown.options.Clear ();
		removeAvatarDropdown.options.Clear ();
		foreach (PlayerManager avatar in GameManager.Instance.avatars) {
			Debug.Log ("Avatars: " + avatar);
			replaceAvatarDropdown.options.Add (new Dropdown.OptionData (){ text = avatar.name });
			if (avatar.isHmdLess) {
				removeAvatarDropdown.options.Add (new Dropdown.OptionData (){ text = avatar.name });
			}
			avatar1.options.Add (new Dropdown.OptionData (){ text = avatar.name });
			avatar2.options.Add (new Dropdown.OptionData (){ text = avatar.name });
		}
		replaceAvatarDropdown.RefreshShownValue ();
		removeAvatarDropdown.RefreshShownValue ();
		avatar1.RefreshShownValue ();
		avatar2.RefreshShownValue ();
	}

	public void ReplaceAvatar(Text newAvatar){
		avatars = new PlayerManager[GameManager.Instance.avatars.Count];
		for(int i = 0; i < GameManager.Instance.avatars.Count; i++){
			avatars [i] = GameManager.Instance.avatars [i];
		}
		PlayerManager oldAvatar = Array.Find (avatars, g => g.name == replaceAvatarDropdown.options [replaceAvatarDropdown.value].text);
		string skeletonName = oldAvatar.localOptitrackAnimator.SkeletonAssetName;
		photonView.RPC ("RPC_ReplaceAvatar", PhotonTargets.AllBuffered, new object[]{ oldAvatar.gameObject.name, newAvatar.text, skeletonName });

	    // Handle the case when avatar change, stop every client
	    if (_recordingManager.IsRecording)
	    {
	        _recordingManager.StopRecording();
	        _recordingManager.IsRecording = true;	        
	    }    

    }
	[PunRPC]
	void RPC_ReplaceAvatar(string oldAvatar, string newAvatar, string skeletonName){
		GameObject oldGO = GameObject.Find (oldAvatar);
		if (oldGO.GetPhotonView ().isMine) {
			if (!oldGO.GetComponent<PlayerManager> ().isHmdLess) {
				try {			    
					gameManager.ReplaceAvatar (oldGO, newAvatar, skeletonName);
				} catch (System.Exception) {
					Debug.LogWarning ("Cant spawn new avatar due to issues with optitrack");
				}
			} else {
				//If is hmdLess and isMine this will always run on MasterClient
				PhotonNetwork.Destroy (oldGO);
				gameManager.photonView.RPC("RPC_AddHmdLessAvatar", PhotonTargets.MasterClient, new object[]{newAvatar, skeletonName});
			}
		}
    }
	//-------------------------------------NO HMD AVATARS------------------------------
	public void AddHmdLessAvatar(Text newAvatar){
		gameManager.photonView.RPC("RPC_AddHmdLessAvatar", PhotonTargets.MasterClient, new object[]{newAvatar.text, newOptitrackAvatarSkeletonName.text});
	}

	public void RemoveAvatar(){
		//Will only destroy pure optitrack avatars since they are the only ones owned by Master
		photonView.RPC ("RPC_RemoveAvatar", PhotonTargets.MasterClient, removeAvatarDropdown.options[removeAvatarDropdown.value].text);
	}
	[PunRPC]
	void RPC_RemoveAvatar(string avatarToRemove){
		GameObject go = GameObject.Find (avatarToRemove);
		PhotonNetwork.Destroy (go);
	}

	//------------------------------------------SCENES------------------------------------------
	public void LoadScene(Text sceneName){
		GameManager.Instance.LoadNewScene (sceneName.text);
	}


	//------------------------------------------RIGIDBODIES------------------------------------------
	public void RefreshRigidbodiesList(){
		rigidbodies = GameObject.FindGameObjectsWithTag ("Rigidbody");
		rigidbodiesDropdown.options.Clear ();
		deleteRigidbodiesDropdown.options.Clear ();
		rigidbodiesDropdown.options.Add (new Dropdown.OptionData (){ text = "Add New Rigidbody" });
		rigidbodiesDropdown.options.Add (new Dropdown.OptionData (){ text = "Delete Rigidbody" });
		foreach (GameObject rigidbody in rigidbodies) {
			rigidbodiesDropdown.options.Add (new Dropdown.OptionData (){ text = rigidbody.name });
			deleteRigidbodiesDropdown.options.Add (new Dropdown.OptionData (){ text = rigidbody.name });
		}
		rigidbodiesDropdown.RefreshShownValue ();
		deleteRigidbodiesDropdown.RefreshShownValue ();
	}
	public void OnRigidBodyButton(Text newRigidbody){
		int _rbId;
		if (string.IsNullOrEmpty (newRigidbodyId.text)) {
			_rbId = 9999;
		} else {
			_rbId = Int32.Parse (newRigidbodyId.text);
		}
		if (rigidbodiesDropdown.value == 0) {
			//Add New Rigidbody
			photonView.RPC ("RPC_SpawnNewRigidbody", PhotonTargets.AllBuffered, new object[] {
				newRigidbody.text,
				_rbId
			});
		} else if(rigidbodiesDropdown.value == 1){
			//Remove rigidbody
			GameObject rigidbodyToRemove = Array.Find(rigidbodies, g => g.name == deleteRigidbodiesDropdown.options [deleteRigidbodiesDropdown.value].text);
			photonView.RPC ("RPC_RemoveRigidbody", PhotonTargets.AllBuffered, new object[] {
				rigidbodyToRemove.name
			});
		}else{
			//Replace Rigidbody
			GameObject oldRigidbody = Array.Find (rigidbodies, g => g.name == rigidbodiesDropdown.options [rigidbodiesDropdown.value].text);
			photonView.RPC ("RPC_ReplaceRigidbody", PhotonTargets.AllBuffered, new object[] {
				oldRigidbody.name,
				newRigidbody.text,
				_rbId
			});
			Debug.Log ("Replacing rigidbody");
		}
	}






	[PunRPC]
	void RPC_ReplaceRigidbody(string oldRigidbody, string newRigidbody, int optitrackId){
		GameObject oldGO = GameObject.Find (oldRigidbody);
		SpawnRigidBody (newRigidbody, optitrackId);
		oldGO.SetActive (false);
		Destroy (oldGO);
	}
	[PunRPC]
	void RPC_RemoveRigidbody(string rigidbodyToRemove){
		GameObject oldGO = GameObject.Find (rigidbodyToRemove);
		oldGO.SetActive (false);
		Destroy (oldGO);
	}
	[PunRPC]
	void RPC_SpawnNewRigidbody(string newRigidbody, int optitrackId){
		SpawnRigidBody (newRigidbody, optitrackId);
	}
	public void SpawnRigidBody(string newRigidbody, int optitrackId){
		Fallback.Instance.readyDefaultResource = Fallback.ReadyDefaultResource.Rigidbody;
		GameObject _rigidbody;
		try{
			_rigidbody = Instantiate (Resources.Load("Rigidbodies/" + newRigidbody, typeof(GameObject)) as GameObject, new Vector3 (0, 0, 0), Quaternion.identity);
		}catch(System.Exception e){
			_rigidbody = Fallback.Instance.InstantiateDefaultResource ();
		}
		_rigidbody.GetComponent<OptitrackRigidBody> ().RigidBodyId = optitrackId;
		RefreshRigidbodiesList ();
		DontDestroyOnLoad (GetComponent<Rigidbody>());
	}





	//------------------------------------------SPACE CALIBRATOR------------------------------------------
	[Header("SpaceCalibrator")]
	public GameObject spaceCalibrationCanvas;
	public Dropdown avatar1;
	public Dropdown avatar2;
	public InputField calibPoint1ID;
	public InputField calibPoint2ID;
	public SpaceCalibrator sc;
	public void StartSync(){
		GameObject _avatar1 = GameObject.Find (avatar1.options [avatar1.value].text);
		GameObject _avatar2 = GameObject.Find (avatar2.options [avatar2.value].text);

		if (_avatar1 == null || _avatar2 == null || _avatar1 == _avatar2) {
			sc.StartSync (_avatar1, Int32.Parse (calibPoint1ID.text));
		} else {
			sc.StartSync (_avatar1, Int32.Parse (calibPoint1ID.text), _avatar2, Int32.Parse (calibPoint2ID.text));
		}

	}
}
