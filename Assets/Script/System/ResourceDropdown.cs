using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




public enum ResourceType{
	Scene,
	Avatar,
	Rigidbody,
}

public class ResourceDropdown : MonoBehaviour {

	Dropdown dropdown;
	public string pathFromAssets = "/Resources/Avatars";
	public string alternativePath = "/Resources/";
	//public bool isScene = false;



	public ResourceType resourceType;

	public string[] unfinishedScenes = new string[0];


	// Use this for initialization
	void Awake () {
	}

	public void AddResource(string resourceName){
		dropdown = GetComponent<Dropdown> ();
		dropdown.options.Add (new Dropdown.OptionData (){ text = resourceName });
		dropdown.RefreshShownValue ();
	}
	public void ClearResources(){
		dropdown = GetComponent<Dropdown> ();
		dropdown.options.Clear ();
		dropdown.RefreshShownValue ();
	}

	public void GenerateResourceDropdowns(){
		dropdown = GetComponent<Dropdown> ();
		dropdown.options.Clear ();
		string[] files;
		if (resourceType == ResourceType.Scene) {
			files = Directory.GetFiles (Application.dataPath + pathFromAssets, "*.unity");

//			var _files = Resources.LoadAll ("Scene");
//			Debug.Log (_files.Length);
//			foreach(object s in _files){
//				Debug.Log ("SAdaDSAD " + s.GetType().ToString());
//				Debug.Log ("SAdaDSAD " + s.ToString());
//			}

			unfinishedScenes = null;
		} else if (resourceType == ResourceType.Avatar) {
			files = Directory.GetFiles (Application.dataPath + pathFromAssets, "*.prefab");
		} else {
			files = Directory.GetFiles (Application.dataPath + pathFromAssets, "*.prefab");
		}
		string fileName;
		if (resourceType == ResourceType.Avatar) {
			foreach (string s in files) {
				fileName = s.Replace (Application.dataPath + "/Resources/", string.Empty).ToString ();
				fileName = fileName.Replace ("." + "prefab", string.Empty).ToString ();
				dropdown.options.Add (new Dropdown.OptionData (){ text = fileName });
				dropdown.RefreshShownValue ();
			}

		} else if (resourceType == ResourceType.Scene) {
			foreach (string s in files) {
				fileName = s.Replace (Application.dataPath + pathFromAssets, string.Empty).ToString ();
				fileName = fileName.Replace ("." + "unity", string.Empty).ToString ();
				dropdown.options.Add (new Dropdown.OptionData (){ text = fileName });
				dropdown.RefreshShownValue ();
			}
			if (unfinishedScenes != null) {
				foreach (string s in unfinishedScenes) {
					fileName = s.Replace (Application.dataPath + alternativePath, string.Empty).ToString ();
					fileName = fileName.Replace (".unity", string.Empty).ToString ();
					//				dropdown.options.Add(new Dropdown.OptionData(){text = fileName});

					var texture = new Texture2D (1, 1); // creating texture with 1 pixel
					texture.SetPixel (1, 1, Color.red); // setting to this pixel some color
					texture.Apply (); //applying texture. necessarily
					var item = new Dropdown.OptionData (fileName, Sprite.Create (texture, new Rect (0, 0, texture.width, texture.height), new Vector2 (0, 0))); // creating dropdown item and converting texture to sprite
					dropdown.options.Add (item); // adding this item to dropdown options

					dropdown.RefreshShownValue ();
				}
			}
		} else {

			foreach (string s in files) {
				fileName = s.Replace (Application.dataPath + pathFromAssets, string.Empty).ToString ();
				fileName = fileName.Replace ("." + "prefab", string.Empty).ToString ();
				dropdown.options.Add (new Dropdown.OptionData (){ text = fileName });
				dropdown.RefreshShownValue ();
			}
		}
	}
}
