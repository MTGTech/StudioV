using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SetupAvatarTags : EditorWindow {

	public int blendShapeCount = 10;

	public GameObject avatarParent;
	public GameObject eyeLevel;
	public GameObject headBone;

	public GameObject optitrack_Avatar;
	public GameObject optitrack_Face;
	public GameObject optitrack_EyeL;
	public GameObject optitrack_EyeR;

	public GameObject HMD_Avatar;
	public GameObject neckUpper;
	public GameObject headLower;
	public GameObject hmd_Face;
	public GameObject hmd_EyeL;
	public GameObject hmd_EyeR;

	public List<GameObject> otherMeshes;

	[MenuItem("Window/Assign Avatar Tags Window")]
	public static void ShowWindow(){
		GetWindow <SetupAvatarTags>("Setup Avatar Tags");
	}
	void OnGUI(){

		EditorGUILayout.BeginVertical ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Avatar Parent", GUILayout.Width(130));
		avatarParent = (GameObject)EditorGUILayout.ObjectField(avatarParent, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		ScriptableObject target = this;
		SerializedObject so = new SerializedObject (target);
		EditorGUILayout.LabelField ("", GUILayout.Width(10));
		SerializedProperty otherMeshesProperty = so.FindProperty ("otherMeshes");
		EditorGUILayout.PropertyField (otherMeshesProperty, true);
		so.ApplyModifiedProperties ();
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.LabelField ("", GUILayout.Width(10)); //Just for some space between othermeshes and the rest of the variales

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("", GUILayout.Width(10));
		EditorGUILayout.LabelField ("EyeLevel", GUILayout.Width(130));
		eyeLevel = (GameObject)EditorGUILayout.ObjectField(eyeLevel, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("", GUILayout.Width(10));
		EditorGUILayout.LabelField ("HeadBone", GUILayout.Width(130));
		headBone = (GameObject)EditorGUILayout.ObjectField(headBone, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("", GUILayout.Width(10));
		EditorGUILayout.LabelField ("Optitrack Avatar", GUILayout.Width(130));
		optitrack_Avatar = (GameObject)EditorGUILayout.ObjectField(optitrack_Avatar, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("", GUILayout.Width(15));
		EditorGUILayout.LabelField ("Optitrack Face", GUILayout.Width(130));
		optitrack_Face = (GameObject)EditorGUILayout.ObjectField(optitrack_Face, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("", GUILayout.Width(15));
		EditorGUILayout.LabelField ("Optitrack Eye Left", GUILayout.Width(130));
		optitrack_EyeL = (GameObject)EditorGUILayout.ObjectField(optitrack_EyeL, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("", GUILayout.Width(15));
		EditorGUILayout.LabelField ("Optitrack Eye Right", GUILayout.Width(130));
		optitrack_EyeR = (GameObject)EditorGUILayout.ObjectField(optitrack_EyeR, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("", GUILayout.Width(10));
		EditorGUILayout.LabelField ("HMD Avatar", GUILayout.Width(130));
		HMD_Avatar = (GameObject)EditorGUILayout.ObjectField(HMD_Avatar, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("", GUILayout.Width(15));
		EditorGUILayout.LabelField ("HMD Face", GUILayout.Width(130));
		hmd_Face = (GameObject)EditorGUILayout.ObjectField(hmd_Face, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("", GUILayout.Width(15));
		EditorGUILayout.LabelField ("HMD Eye Left", GUILayout.Width(130));
		hmd_EyeL = (GameObject)EditorGUILayout.ObjectField(hmd_EyeL, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("", GUILayout.Width(15));
		EditorGUILayout.LabelField ("HMD Eye Right", GUILayout.Width(130));
		hmd_EyeR = (GameObject)EditorGUILayout.ObjectField(hmd_EyeR, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("", GUILayout.Width(15));
		EditorGUILayout.LabelField ("NeckUpper", GUILayout.Width(130));
		neckUpper = (GameObject)EditorGUILayout.ObjectField(neckUpper, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("", GUILayout.Width(15));
		EditorGUILayout.LabelField ("HeadLower", GUILayout.Width(130));
		headLower = (GameObject)EditorGUILayout.ObjectField(headLower, typeof(GameObject), true);
		EditorGUILayout.EndHorizontal ();

		if (GUILayout.Button("Assign Tags")) {
			SetTags ();
		}
		if (GUILayout.Button("Try to find GameObjects")) {
			otherMeshes = new List<GameObject> ();
			RecursivelyFindGameObjectsInAvatarToAddTags (avatarParent.transform);
		}

		EditorGUILayout.EndVertical ();
	}
	void RecursivelyFindGameObjectsInAvatarToAddTags(Transform parent){
		foreach(Transform child in parent){
			if(child.name.Contains("titrack") && child.GetComponent<Animator>() != null){
				optitrack_Avatar = child.gameObject;
				RecursivelyFindGameObjectsInOptitrackAvatarToAddTags (child);
			}
			if(child.name.Contains("HMD") && child.GetComponent<Animator>() != null){
				HMD_Avatar = child.gameObject;
				RecursivelyFindGameObjectsInHMDAvatarToAddTags (child);
			}
			if(child.name.Contains("yeLevel")){
				eyeLevel = child.gameObject;
			}
			if(child.name.Contains("mixamorig:Head")){
				headBone = child.gameObject;
			}
		}
		RecursivelyFindGameObjectsInHeadBoneAvatarToAddTags (headBone.transform);
	}
	void RecursivelyFindGameObjectsInOptitrackAvatarToAddTags (Transform parent){
		foreach(Transform child in parent){
			if(child.GetComponent<SkinnedMeshRenderer>() != null){
				if(!child.name.Contains("Eyes") && child.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount > blendShapeCount){
					optitrack_Face = child.gameObject;
				}else if(child.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount < blendShapeCount && child.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount > 0){
					otherMeshes.Add(child.gameObject);
				}
			}
			if(child.name.Contains("ye_L") || (child.name.Contains("EyeL") && !child.name.Contains("EyeLevel"))){
				optitrack_EyeL = child.gameObject;
			}else if(child.name.Contains("ye_R") || child.name.Contains("EyeR")){
				optitrack_EyeR = child.gameObject;
			}
			RecursivelyFindGameObjectsInOptitrackAvatarToAddTags (child);
		}
	}
	void RecursivelyFindGameObjectsInHMDAvatarToAddTags (Transform parent){
		foreach(Transform child in parent){
			if(child.GetComponent<SkinnedMeshRenderer>() != null){
				if(!child.name.Contains("Eyes") && child.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount > blendShapeCount){
					hmd_Face = child.gameObject;
				}else if(child.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount < blendShapeCount && child.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount > 0){
					otherMeshes.Add(child.gameObject);
				}
			}
			if (child.name.Contains ("eckUpper")) {
				neckUpper = child.gameObject;
			}
			RecursivelyFindGameObjectsInHMDAvatarToAddTags (child);
		}
	}
	void RecursivelyFindGameObjectsInHeadBoneAvatarToAddTags (Transform parent){
		foreach(Transform child in parent){
			if(child.name.Contains("ye_L") || (child.name.Contains("EyeL") && !child.name.Contains("EyeLevel"))){
				hmd_EyeL = child.gameObject;
			}else if(child.name.Contains("ye_R") || child.name.Contains("EyeR")){
				hmd_EyeR = child.gameObject;
			}
			if (child.name.Contains ("eadLower")) {
				headLower = child.gameObject;
			}
		}
	}
	void SetTags(){
		optitrack_Avatar.tag = "AAS_Optitrack_Avatar";
		HMD_Avatar.tag = "AAS_HMD_Avatar";
		hmd_Face.tag = "AAS_HMD_Face";
		optitrack_Face.tag = "AAS_Optitrack_Face";
		hmd_EyeL.tag = "AAS_HMD_EyeL";
		hmd_EyeR.tag = "AAS_HMD_EyeR";
		optitrack_EyeL.tag = "AAS_Optitrack_EyeL";
		optitrack_EyeR.tag = "AAS_Optitrack_EyeR";
		neckUpper.tag = "AAS_NeckUpper";
		headLower.tag = "AAS_HeadLower";
		headBone.tag = "AAS_HeadBone";
		eyeLevel.tag = "AAS_EyeLevel";
		foreach(GameObject g in otherMeshes){
			g.tag = "AAS_OtherMeshes";
		}
	}
}
