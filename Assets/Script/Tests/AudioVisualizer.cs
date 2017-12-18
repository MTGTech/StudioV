using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVisualizer : MonoBehaviour {

	AudioSource source;
	float[] spectrum;
	public float amplify = 20;
	[HideInInspector]public List<Transform> visualBars;


	// Use this for initialization
	void Awake () {
		source = GetComponent<AudioSource> ();
//		spectrum = new float[256];
		visualBars = new List<Transform> ();
//		foreach (Transform t in transform) {
//			visualBars.Add (t);
//		}


	}
	// Update is called once per frame
	void Update () {

		spectrum = AudioListener.GetSpectrumData (1024, 0, FFTWindow.Hamming);

		for (int i = 0; i < visualBars.Count; i++) {
			Vector3 previousScale = visualBars [i].localScale;
			previousScale.y = spectrum [i] * amplify;
			visualBars [i].localScale = previousScale;
		}
	}
}
