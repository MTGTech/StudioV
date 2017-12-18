using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarResourceElement : MonoBehaviour {

	public Text avatarNameText;
	public Image avatarImage;
	[HideInInspector] public string avatarName;
	[HideInInspector] public AvatarMenu am;

	public void SetupElement(string avatarName){
		avatarNameText.text = avatarName;
		this.avatarName = avatarName;
	}
	public void OnClick(){
		am.SetSelectedNewAvatar (avatarName);
	}
}
