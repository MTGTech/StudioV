using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TestAAS : MonoBehaviour {
	

	[MenuItem("Tools/Make Avatar ready for AssetBundle")]
	static void Run(){
		GameObject go = Selection.activeGameObject;


		string assetBundleName = "avatars/" + go.name;
		if (go.tag == "Avatar") {

//			//Duplicate avatar prefab
//			Object objectRoot = PrefabUtility.GetPrefabParent (go);
//			Object newGo = PrefabUtility.InstantiatePrefab (objectRoot);
//			AssetDatabase.Refresh ();

			//Set AssetBundle name of avatar
			string path = AssetDatabase.GetAssetPath (go);
			AssetImporter ai = AssetImporter.GetAtPath (path);
			Debug.Log (ai.assetBundleName);
			ai.assetBundleName = assetBundleName;

			//Write facial weights into text file
			FacialController fc = go.GetComponent<FacialController> ();
			string textPath = path.Replace(".prefab", string.Empty) + "_facialExpressions.txt";
			Debug.Log (textPath);
			StreamWriter sw = new StreamWriter (textPath, true);
			for(int i = 0; i < fc.expressionTargets.Length; i++){
				sw.WriteLine (fc.expressionTargets[i]+","+fc.expressionWeights[i]);
			}
			sw.Close ();
			AssetDatabase.Refresh ();
			AssetImporter textai = AssetImporter.GetAtPath (textPath);
			textai.assetBundleName = assetBundleName;

			//Remove all components
			RecursivelyRemoveComponents(go.transform);
		}
	}
	static void RecursivelyRemoveComponents(Transform parent){
		foreach(Component comp in parent.GetComponents<Component>()){
			if(!(comp is Transform || comp is Animator || comp is SkinnedMeshRenderer || comp is MeshFilter || comp is MeshRenderer)){
//				Debug.Log (comp);
				DestroyImmediate(comp, true);
			}
		}
		foreach(Transform child in parent){
			RecursivelyRemoveComponents (child);
		}
	}


//	[MenuItem("Tools/Execute Avatar Generation")]
//	static void AvatarGeneration(){
//		GameObject optitrackBody = Selection.activeGameObject;
//
//		GameObject newParent = new GameObject ();
//
//		optitrackBody.transform.parent = newParent.transform;
//		GameObject hmdBody = Instantiate (optitrackBody, newParent.transform)as GameObject;
//
//		newParent.tag = "Avatar";
//		newParent.name = optitrackBody;
//		optitrackBody.name += "_Optitrack_Body";
//		hmdBody.name += "_HMD_Body";
//
//		RecursivelySetupOptitrackBody (optitrackBody.transform);
//		RecursivelySetupHMDBody (hmdBody.transform);
//
//	}
//	public void RecursivelySetupOptitrackBody(Transform parent){
//		foreach(Transform t in parent){
//			switch (t.name) {
//			case "mixamorig:Hips":
//				break;
//			}
//		}
//	}
//	public void RecursivelySetupHMDBody(Transform parent){
//		foreach(Transform t in parent){
//			if(t.name == "mixamorig:Hips"){
//
//			}
//		}
//	}
}
