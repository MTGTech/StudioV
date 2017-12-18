using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticAvatarSetup : MonoBehaviour {


	PlayerManager pm;
	FacialController fc;
	PhotonView pv;
	ScaleAdjust sa;
	SetHeadPos shp;

	SyncAvatarToHMD satHMD;

	void StartBasicSetup(GameObject _avatar){
		pm = _avatar.AddComponent (typeof(PlayerManager)) as PlayerManager;
		pm.FindCameraRig ();
		sa = _avatar.AddComponent (typeof(ScaleAdjust)) as ScaleAdjust;
		pv = _avatar.transform.parent.parent.GetComponent<PhotonView>();
		pm.photonView = pv;
		pv.ObservedComponents = new List<Component> ();
		pv.ObservedComponents.Add (pm);

	}
	public void StartHMDSetup(GameObject _avatar, string skeletonName, string faceExpressions){
		StartBasicSetup (_avatar);
		fc = _avatar.AddComponent (typeof(FacialController)) as FacialController;
		fc.photonView = pv;
		fc.OtherMeshes = new Transform[0];
		IterateAndAddComponentsToThroughChildren (_avatar.transform, true);
		IterateAndAssignValuesDependenciesInChildren (_avatar.transform);
		pv.ObservedComponents.Add (fc);
		pv.ObservedComponents.Add (shp);
		pm.localOptitrackAnimator.SkeletonAssetName = skeletonName;
		pm.remoteOptitrackAnimator.SkeletonAssetName = skeletonName;
		pm.localOptitrackAnimator.enabled = true;
		pm.remoteOptitrackAnimator.enabled = true;

		fc.expressionTargets = new string[faceExpressions.Split ('\n').Length - 1];
		fc.expressionWeights = new float[fc.expressionTargets.Length];
		for (int i = 0; i < faceExpressions.Split ('\n').Length -1; i++) {
			string[] entries = GetLine (faceExpressions, i).Split(',');
			fc.expressionTargets [i] = entries [0];
			fc.expressionWeights [i] = float.Parse(entries [1]);
		}
	}
	public void StartHMDLessSetup(GameObject _avatar, string skeletonName, string faceExpressions){
		StartBasicSetup (_avatar);
		IterateAndAddComponentsToThroughChildren (_avatar.transform, false);

		pm.remoteOptitrackAnimator.SkeletonAssetName = skeletonName;
		pm.remoteOptitrackAnimator.enabled = true;

		pm.isHmdLess = true;
	}
	string GetLine(string text, int lineNr){
		string[] lines = text.Replace ("\r", "").Split ('\n');
		return lines.Length >= lineNr ? lines [lineNr] : null;
	}

	void IterateAndAddComponentsToThroughChildren(Transform parent, bool withHmd){
		foreach(Transform child in parent){
			switch(child.tag){
			case "AAS_HMD_Avatar":
				if (withHmd) {
					pm.localOptitrackAnimator = child.gameObject.AddComponent (typeof(OptitrackSkeletonAnimator)) as OptitrackSkeletonAnimator;
					pm.localOptitrackAnimator.DestinationAvatar = child.GetComponent<Animator> ().avatar;
					satHMD = child.gameObject.AddComponent (typeof(SyncAvatarToHMD)) as SyncAvatarToHMD;
				} else {
					Destroy (child.gameObject);
				}
				break;
			case "AAS_Optitrack_Avatar":
				pm.remoteOptitrackAnimator = child.gameObject.AddComponent (typeof(OptitrackSkeletonAnimator)) as OptitrackSkeletonAnimator;
				pm.remoteOptitrackAnimator.DestinationAvatar = child.GetComponent<Animator> ().avatar;
				break;
			case "AAS_HeadBone":
				if (withHmd) {
					shp = child.gameObject.AddComponent (typeof(SetHeadPos)) as SetHeadPos;
					shp.lerpRate = 15f;
					shp._playerPhotonView = pv;
				} else {
					Destroy (child.gameObject);
				}
				break;
			case "AAS_EyeLevel":
				sa.eyeLevel = child;
				break;
			case "AAS_OtherMeshes":
				if (withHmd) {
					Transform[] t = new Transform[fc.OtherMeshes.Length + 1];
					for (int i = 0; i < fc.OtherMeshes.Length; i++) {
						t [i] = fc.OtherMeshes [i];
					}
					t [fc.OtherMeshes.Length] = child;
					fc.OtherMeshes = t;
				}
				break;
			}
			IterateAndAddComponentsToThroughChildren (child, withHmd);
		}
	}
	void IterateAndAssignValuesDependenciesInChildren(Transform parent){
		foreach(Transform child in parent){
			switch(child.tag){
			case "AAS_HMD_Face":
				fc.HmdFace = child.GetComponent<SkinnedMeshRenderer> ();
				break;
			case "AAS_Optitrack_Face":
				fc.OptitrackFace = child.GetComponent<SkinnedMeshRenderer> ();
				break;
			case "AAS_HMD_EyeL":
				fc.eye_L = child;
				break;
			case "AAS_HMD_EyeR":
				fc.eye_R = child;
				break;
			case "AAS_Optitrack_EyeL":
				fc.r_eye_L = child; 
				break;
			case "AAS_Optitrack_EyeR":
				fc.r_eye_R = child;
				break;
			case "AAS_NeckUpper":
				satHMD.neckUpper = child;
				break;
			case "AAS_HeadLower":
				satHMD.headLower = child;
				break;
			
			}
			IterateAndAssignValuesDependenciesInChildren (child);
		}
	}
}
