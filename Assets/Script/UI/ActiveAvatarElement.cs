using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ActiveAvatarElement : MonoBehaviour {

	[HideInInspector]public AvatarMenu am;
	public Text label;
	public Image avatarImage;
	AvatarManager associatedAvatarManager;
	public GameObject replaceButton;
	public GameObject removeButton;

	public void SetupElement(AvatarManager avatarManager, string skeletonName, Sprite image, bool canBeRemoved){
		associatedAvatarManager = avatarManager;
		label.text = skeletonName;
		removeButton.SetActive (canBeRemoved);
		transform.SetSiblingIndex (transform.GetSiblingIndex() - 1);
		associatedAvatarManager.OnAvatarDestroyed += MissingAvatar;
		if(image != null){
			avatarImage.sprite = image;
		}
	}
	void MissingAvatar(){
		associatedAvatarManager.OnAvatarDestroyed -= MissingAvatar;
		Destroy (gameObject);
	}
	public void Replace(){
		associatedAvatarManager.ReplaceAvatar (am.newAvatarName);
//		Destroy (gameObject);
//		Debug.Log ("Replaceing " + label.text + " with " + am.newAvatarName);
	}
	public void SetSkeletonName(Text input){
		if (string.IsNullOrEmpty (input.text)) {
			return;
		}
		associatedAvatarManager.SetSkeletonName(input.text);
		label.text = input.text;
	}
	public void SetScale(Text input){
		if (string.IsNullOrEmpty (input.text)) {
			return;
		}
		associatedAvatarManager.SetToActorScale (float.Parse(input.text));
	}
	public void Remove(){
		associatedAvatarManager.RemoveAvatar ();
//		Destroy (gameObject);
	}
}
