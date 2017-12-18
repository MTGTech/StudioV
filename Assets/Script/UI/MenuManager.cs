using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddHmdLessAvatarEventArgs : EventArgs{
	public string newAvatarName;
	public string skeletonName;
}

public class MenuManager : MonoBehaviour {
	public static MenuManager Instance { get; internal set;}

	public delegate void OpenCloseMenu();
	public static event OpenCloseMenu OnOpenCloseMenu;

	public delegate void AddHmdLessAvatarButton(object source, AddHmdLessAvatarEventArgs args);
	public static event AddHmdLessAvatarButton OnAddHmdLessAvatarButton;

	void Awake(){
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy (this);
		}
	}

	[SerializeField]GameObject mainPanelObject;
	[SerializeField]GameObject activeAvatarUIElement;
	[SerializeField]GameObject avatarResourceUIElement;
	[SerializeField]Transform avatarResourcesUIParent;
	[SerializeField]Transform activeAvatarsUIParent;
	public AvatarMenu am;

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			mainPanelObject.SetActive (!mainPanelObject.activeSelf);
		}
	}
	public void AddAvatarResource(string avatarName){
		GameObject go = Instantiate (avatarResourceUIElement, avatarResourcesUIParent) as GameObject;
		go.GetComponent<AvatarResourceElement> ().am = am;
		go.GetComponent<AvatarResourceElement> ().SetupElement(avatarName);
	}
	public void AddActiveAvatar(AvatarManager avatar, string skeletonName, Sprite image, bool canBeRemoved){
		GameObject go = Instantiate (activeAvatarUIElement, activeAvatarsUIParent) as GameObject;
		go.GetComponent<ActiveAvatarElement> ().am = am;
		go.GetComponent<ActiveAvatarElement> ().SetupElement (avatar, skeletonName, image, canBeRemoved);
	}
	public void PressAddAvatarButton(AddHmdLessAvatarElement input){
		if(OnAddHmdLessAvatarButton != null){
			if(!String.IsNullOrEmpty(input.skeletonName) && !String.IsNullOrEmpty(input.am.newAvatarName)){
				OnAddHmdLessAvatarButton (this, new AddHmdLessAvatarEventArgs(){skeletonName = input.skeletonName, newAvatarName = input.am.newAvatarName});
				Debug.Log ("Addinghmdless from menu");
			}
		}
	}
}
