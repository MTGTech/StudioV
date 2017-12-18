using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarMenu : MonoBehaviour {

	public string newAvatarName;
	public string activeAvatar;

	public void SetSelectedNewAvatar(string newAvatarName){
		this.newAvatarName = newAvatarName;
	}
}
