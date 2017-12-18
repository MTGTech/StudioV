using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

// TODO: playback in slow speed
[CustomEditor(typeof(FacialPlayback))]
public class FacialEditor : Editor
{
    FacialPlayback playback;

    void OnEnable()
    {
        playback = (FacialPlayback)target;
    }

    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();

        for (int i = 0; i < playback.OptitrackFace.sharedMesh.blendShapeCount; i++)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(playback.OptitrackFace.sharedMesh.GetBlendShapeName(i), GUILayout.MinWidth(120));
            playback.OptitrackFace.SetBlendShapeWeight(i,
                EditorGUILayout.Slider("", playback.OptitrackFace.GetBlendShapeWeight(i), 0, 100,
                    GUILayout.Width(200)));

            GUILayout.EndHorizontal();
        }

    }

}
