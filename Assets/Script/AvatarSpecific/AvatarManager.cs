using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AvatarManager : Photon.MonoBehaviour {

	//--------- test
	public GameObject canvas;
	public Text avatarName;
	public Text skeletonNameText;
	public Text actorHeightText;
	public float actorTargetHeight;
	public Dropdown avatarDropdown;

	public void InstantiateViaTestMenu(){
		if (string.IsNullOrEmpty (actorHeightText.text)) {
			return;
		}
		GetComponent<Camera> ().enabled = false;
		Destroy (canvas);
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
		actorTargetHeight = float.Parse (actorHeightText.text);
		InstantiateHMDAvatar (avatarName.text, skeletonNameText.text, actorTargetHeight);
	}
	void Awake(){
		if (photonView.isMine && !PhotonNetwork.isMasterClient) {
			GetComponent<Camera> ().enabled = true;
		} else{
			GetComponent<Camera> ().enabled = false;
			Destroy (canvas);
		}
		gameObject.name = "AvatarManager" + photonView.viewID;
		DontDestroyOnLoad (gameObject);

		Transform parent = GameObject.FindGameObjectWithTag ("ExpressionController").transform;
		transform.parent = parent;

		//Add avatar assebundles to temporary popupmenu
		ResourceDropdown[] _dropdowns = FindObjectsOfTypeAll (typeof(ResourceDropdown)) as ResourceDropdown[];
		foreach (ResourceDropdown _rd in _dropdowns) {
			if(_rd.resourceType == ResourceType.Avatar){
				_rd.ClearResources ();
				foreach (AvatarAssetBundle avatarResource in AssetBundleManager.Instance.avatarAssetBundles) {
					_rd.AddResource (avatarResource.avatarAsset.name);	
				}
			}
		}
		if (!photonView.isMine && avatar == null && !PhotonNetwork.isMasterClient) {
			photonView.RPC ("RPC_M_GetAndApplyActiveAvatar", PhotonTargets.MasterClient, new object[]{gameObject.name});
		}
	}
	//---------
	public AvatarAssetBundle avatarAssetBundle;
	[HideInInspector] public GameObject avatar;
	public string skeletonName;
	public delegate void AvatarDestroyed();
	public event AvatarDestroyed OnAvatarDestroyed;

	void OnDestroy(){
		if (OnAvatarDestroyed != null) {
			OnAvatarDestroyed ();
		}
	}
	//The master tells the recently joined client what avatar parameters to apply to the specified avatar.
	[PunRPC]
	void RPC_M_GetAndApplyActiveAvatar(string avatarManagerName){
		AvatarManager am = GameObject.Find (avatarManagerName).GetComponent<AvatarManager>();
		Debug.Log ("GetAndApplyActiveAvatar: " + am + " tried to find: " + avatarManagerName);
		photonView.RPC ("RPC_UpdateActiveAvatar", PhotonTargets.Others, new object[]{am.avatarAssetBundle.avatarAsset.name, am.skeletonName, am.actorTargetHeight});

	}
	[PunRPC]
	void RPC_UpdateActiveAvatar(string _avatarResourceName, string _skeletonName, float _actorHeihgt){
		if (avatar == null) {
			LoadHMDAvatarFromAssetBundle (_avatarResourceName, _skeletonName, _actorHeihgt);
		}
	}
	//Run from local AvatarManager startup menu
	public void InstantiateHMDAvatar(string _avatarResourceName, string _skeletonName, float _actorHeihgt){
		photonView.RPC ("RPC_InstantiateAvatar", PhotonTargets.All, new object[]{_avatarResourceName, _skeletonName, _actorHeihgt});
	}
	[PunRPC]
	public void RPC_InstantiateAvatar(string _avatarResourceName, string _skeletonName, float _actorHeihgt){
		LoadHMDAvatarFromAssetBundle (_avatarResourceName, _skeletonName, _actorHeihgt);
	}
	public void ReplaceAvatar(string newAvatarName){
		if(string.IsNullOrEmpty(newAvatarName)){
			return;
		}
		photonView.RPC ("RPC_ReplaceAvatar", PhotonTargets.All, new object[]{gameObject.name, newAvatarName});
	}
	[PunRPC]
	void RPC_ReplaceAvatar(string avatarManagerName, string newAvatarName){
		AvatarManager _avatarManager = GameObject.Find (avatarManagerName).GetComponent<AvatarManager>();
		if (avatar.GetComponent<PlayerManager> ().isHmdLess) {
			Destroy (_avatarManager.avatar);
			if (OnAvatarDestroyed != null) {
				OnAvatarDestroyed ();
			}
			LoadHMDLessAvatarFromAssetBundle (newAvatarName, _avatarManager.skeletonName, _avatarManager.actorTargetHeight);

		} else {
			Destroy (_avatarManager.avatar);
			if (OnAvatarDestroyed != null) {
				OnAvatarDestroyed ();
			}
			LoadHMDAvatarFromAssetBundle (newAvatarName, _avatarManager.skeletonName, _avatarManager.actorTargetHeight);

		}
	}
	public void RemoveAvatar(){
		photonView.RPC("RPC_RemoveAvatar", PhotonTargets.All, new object[]{gameObject.name});
	}
	[PunRPC]
	void RPC_RemoveAvatar(string avatarManagerName){
		GameObject _avatar = GameObject.Find (avatarManagerName).GetComponent<AvatarManager> ().avatar;
		if (_avatar.GetComponent<PlayerManager> ().isHmdLess) {
			PhotonNetwork.Destroy (gameObject);
		} else {
			Destroy (GameObject.Find(avatarManagerName).GetComponent<AvatarManager>().avatar);
		}
			
	}
	public void SetSkeletonName(string skeletonName){
		photonView.RPC("RPC_SetSkeletonName", PhotonTargets.AllBuffered, new object[]{
			gameObject.name,
			skeletonName
		});
	}
	[PunRPC]
	public void RPC_SetSkeletonName(string gameObjectName, string skeletonName){
		PlayerManager pm = GameObject.Find(gameObjectName).GetComponent<AvatarManager>().avatar.GetComponent<PlayerManager>();
		if (pm.localOptitrackAnimator != null) {
			pm.localOptitrackAnimator.enabled = false;
			pm.localOptitrackAnimator.SkeletonAssetName = skeletonName;
			pm.localOptitrackAnimator.enabled = true;
		}

		pm.remoteOptitrackAnimator.enabled = false;
		pm.remoteOptitrackAnimator.SkeletonAssetName = skeletonName;
		pm.remoteOptitrackAnimator.enabled = true;
	}
	public void SetToActorScale(float targetHeight){
		photonView.RPC("RPC_SetToActorScale", PhotonTargets.OthersBuffered, new object[] {gameObject.name, avatar.GetComponent<ScaleAdjust> ().SetScale (targetHeight)});
	}
	[PunRPC]
	public void RPC_SetToActorScale(string playerName, Vector3 localScale){
		// Get original global scale of hmd
		Transform hmd = GameObject.Find("[CameraRig]").transform;
		Vector3 scl = hmd.lossyScale;
		// Scale down avatar prefab
		GameObject.Find(playerName).transform.localScale = localScale;
		// Resize hmd to glocal scale (1,1,1)
		hmd.localScale *= scl.x / hmd.lossyScale.x;
	}
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
	public void LoadHMDLessAvatarFromAssetBundle(string avatarResourceName, string skeletonName, float _actorHeight){
		this.skeletonName = skeletonName;
		this.actorTargetHeight = _actorHeight;

		avatarAssetBundle = AssetBundleManager.Instance.avatarAssetBundles.Where(obj => obj.avatarAsset.name == avatarResourceName).SingleOrDefault();

		GameObject tempParent = new GameObject ();
		tempParent.transform.parent = transform;
		tempParent.transform.localPosition = Vector3.zero;
		tempParent.transform.localRotation = Quaternion.Euler(Vector3.zero);
		tempParent.SetActive (false);
		GameObject go = Instantiate (avatarAssetBundle.avatarAsset, Vector3.zero, Quaternion.identity, tempParent.transform)as GameObject;

		if (go.tag == "Avatar") {
			AutomaticAvatarSetup aas = new AutomaticAvatarSetup ();
			aas.StartHMDLessSetup (go, skeletonName, avatarAssetBundle.avatarTextAsset.text);
			ChangeLayerRecursively (go.transform);
			MenuManager.Instance.AddActiveAvatar (this, skeletonName, avatarAssetBundle.image, true);
		}
		go.transform.parent = transform;
		avatar = go;
		Destroy (tempParent);
		go.GetComponent<ScaleAdjust> ().SetScale (_actorHeight);
		//if (photonView.isMine) {
		//	GameObject[] avatarUi = GameObject.FindGameObjectsWithTag ("AvatarUI");
		//	foreach (GameObject aui in avatarUi) {
		//		aui.GetComponent<Canvas> ().enabled = true;
		//	}
		//}
	}
	void LoadHMDAvatarFromAssetBundle(string avatarResourceName, string skeletonName, float _actorHeight){
		this.skeletonName = skeletonName;
		this.actorTargetHeight = _actorHeight;

		avatarAssetBundle = AssetBundleManager.Instance.avatarAssetBundles.Where(obj => obj.avatarAsset.name == avatarResourceName).SingleOrDefault();

		GameObject tempParent = new GameObject ();
		tempParent.transform.parent = transform;
		tempParent.transform.localPosition = Vector3.zero;
		tempParent.transform.localRotation = Quaternion.Euler(Vector3.zero);
		tempParent.SetActive (false);
		GameObject go = Instantiate (avatarAssetBundle.avatarAsset, Vector3.zero, Quaternion.identity, tempParent.transform)as GameObject;

		if (go.tag == "Avatar") {
			AutomaticAvatarSetup aas = new AutomaticAvatarSetup ();
			aas.StartHMDSetup (go, skeletonName, avatarAssetBundle.avatarTextAsset.text);
			MenuManager.Instance.AddActiveAvatar (this, skeletonName, avatarAssetBundle.image, false);
		}
		go.transform.parent = transform;
		avatar = go;
		Destroy (tempParent);
		go.GetComponent<ScaleAdjust> ().SetScale (_actorHeight);
		if (photonView.isMine) {
			GameObject[] avatarUi = GameObject.FindGameObjectsWithTag ("AvatarUI");
			foreach (GameObject aui in avatarUi) {
				aui.GetComponent<Canvas> ().enabled = true;
			}
		}
	}
}
