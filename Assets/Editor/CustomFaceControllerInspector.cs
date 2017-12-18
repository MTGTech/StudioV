using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(FacialController))]
public class CustomFaceControllerInspector : Editor {
	FacialController fc;
	void OnEnable(){
		fc = (FacialController)target;
	}

	public override void OnInspectorGUI(){
		DrawDefaultInspector ();

        EditorGUILayout.Space();
	    EditorGUILayout.LabelField("Customize BlendShapes Weights", EditorStyles.boldLabel);

		for(int i = 0; i < fc.expressionTargets.Length; i++){
			GUILayout.BeginHorizontal ();
			// TODO: error message array index out of range
			GUILayout.Label (fc.expressionTargetNames[i], GUILayout.MinWidth(117));
			fc.expressionTargets[i] = GUILayout.TextField(fc.expressionTargets[i],GUILayout.MinWidth(110));
			EditorGUIUtility.labelWidth = 15;
			fc.expressionWeights [i] = EditorGUILayout.FloatField ("X",fc.expressionWeights [i],GUILayout.Width(125));
			GUILayout.EndHorizontal ();
		}
	}
}  
