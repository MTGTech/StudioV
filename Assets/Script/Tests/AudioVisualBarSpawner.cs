using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVisualBarSpawner : MonoBehaviour {

	AudioVisualizer av;
	public GameObject visualBarPrefab;
	public int resolution;
	public float barDistance;
	public Transform [,] matris;
	public float timeDelay;
	// Use this for initialization
	void Start () {
		matris = new Transform[resolution, resolution];
		av = GetComponent<AudioVisualizer> ();
		for (int x = 0; x < resolution; x++) {

			GameObject go = Instantiate (visualBarPrefab, transform.position + new Vector3 ((barDistance * x), 0.1f, 0), Quaternion.identity)as GameObject;
			av.visualBars.Add (go.transform);

			for (int y = 0; y < resolution; y++) {
				GameObject go2 = Instantiate (visualBarPrefab, transform.position + new Vector3 ((barDistance * y), 0.1f, barDistance * x), Quaternion.identity)as GameObject;
				matris [y, x] = go2.transform;
			}
		}
		StartCoroutine (UpdateDelayedAudioVisuals());
	}
	IEnumerator UpdateDelayedAudioVisuals(){
		while (true) {
			yield return new WaitForEndOfFrame();
			for (int x = 0; x < resolution; x++) {
				StartCoroutine (DelayedAudioVisualsRow(x));
			}
		}
	}
	IEnumerator DelayedAudioVisualsBar(Transform bar, float scale){
		yield return new WaitForSeconds (timeDelay);
		Vector3 previousScale = bar.localScale;
		previousScale.y = scale;
		bar.localScale = previousScale;
	}
	IEnumerator DelayedAudioVisualsRow(int x){
		for (int y = 0; y < resolution; y++) {
			yield return new WaitForSeconds (timeDelay);
			StartCoroutine (DelayedAudioVisualsBar (matris[x,y], av.visualBars[x].localScale.y));
		}
	}
}
