using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddHmdLessAvatarElement : MonoBehaviour {
	public AvatarMenu am;
	public string skeletonName;
	public float actorHeight;

	public void SetSkeletonName(Text skeletonNameInput){
		skeletonName = skeletonNameInput.text;
	}
}
