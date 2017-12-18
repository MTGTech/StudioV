using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AvatarAssetBundle{
	public GameObject avatarAsset;
	public TextAsset avatarTextAsset;
	public Sprite image;
	public AvatarAssetBundle (GameObject avatarAsset, TextAsset avatarTextAsset, Sprite image){
		this.avatarAsset = avatarAsset;
		this.avatarTextAsset = avatarTextAsset;
		this.image = image;
	}
}
public class AssetBundleManager : MonoBehaviour {
	public static AssetBundleManager Instance;
	public List<AvatarAssetBundle> avatarAssetBundles;
	void Awake(){
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy (this);
		}
	}
	void Start () {
		StartCoroutine(LoadAllAvatarAssetBundles());
	}
	//This loads all avatar assets and displays them in the menus
	public IEnumerator LoadAllAvatarAssetBundles(){
		avatarAssetBundles = new List<AvatarAssetBundle> ();
		string[] files = Directory.GetFiles (Application.streamingAssetsPath + "/avatars/", "*.");
		for (int i = 0; i < files.Length; i++) {
			string fileName = files[i].Replace (Application.streamingAssetsPath + "/avatars/", string.Empty);
			var assetBundleRequest = AssetBundle.LoadFromFileAsync (Application.streamingAssetsPath + "/avatars/" + fileName);
			yield return assetBundleRequest;
			var assetBundle = assetBundleRequest.assetBundle;
			GameObject[] gameObjects = assetBundle.LoadAllAssets<GameObject> ();
			yield return gameObjects;
			TextAsset textAsset = assetBundle.LoadAsset<TextAsset> (gameObjects [0].name + "_facialExpressions.txt");
			yield return textAsset;
			Sprite image = assetBundle.LoadAsset<Sprite> (gameObjects [0].name + "_image.png");
			avatarAssetBundles.Add(new AvatarAssetBundle(gameObjects[0], textAsset, image));
			MenuManager.Instance.AddAvatarResource (gameObjects[0].name);
		}
	}
	//Needs further investigations for handling scenes in assetbundles
	IEnumerator LoadAssetBundle(string assetBundleName){
		var assetBundleRequest = AssetBundle.LoadFromFileAsync (Application.streamingAssetsPath + assetBundleName);
		yield return assetBundleRequest;
		var assetBundle = assetBundleRequest.assetBundle;
		if (assetBundle.isStreamedSceneAssetBundle) {
			string[] scenePath = assetBundle.GetAllScenePaths ();

			ResourceDropdown[] _dropdowns = FindObjectsOfTypeAll (typeof(ResourceDropdown)) as ResourceDropdown[];
			foreach(ResourceDropdown _rd in _dropdowns){
				if (_rd.resourceType == ResourceType.Scene) {
					_rd.AddResource (System.IO.Path.GetFileNameWithoutExtension (scenePath [0]));
				}
			}
		}
	}
}
