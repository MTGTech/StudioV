using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ScaleAdjust))]
public class ScaleAdjustEditor : Editor
{
    // 1f is the orginal local scale after scale the avatar prefab
    //private static float headScale = 1f;

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();

		ScaleAdjust script = (ScaleAdjust)target;
	    if (GUILayout.Button("Set Special Height"))
	    {
	        script.SetSpecialHeight();
	    }
        //headScale = EditorGUILayout.Slider("Set Head Scale", headScale, 0, 5);

        // if the game is not playing, don't change in editor mode
     //   if (! Application.isPlaying)
	    //{
     //       return;
	    //}

        //script.SetHeadScale(headScale);
    }

}
#endif