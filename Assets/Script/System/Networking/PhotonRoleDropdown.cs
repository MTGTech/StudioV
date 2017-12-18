using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MTGTech.MyGame{

	[RequireComponent(typeof(Dropdown))]
	public class PhotonRoleDropdown : MonoBehaviour {

		#region Private Variables
		// Store the PlayerPref key to avoid typos
		Dropdown _dropdown;
		public Dropdown sceneDropdown;

		#endregion

		public GameObject avatarName;
		public GameObject skeletonName;
		public GameObject actorHeight;
	    public GameObject recordingFPS;


		#region MonoBehavior CallBacks
		// Use this for initialization
		void Start () {
			_dropdown = GetComponent<Dropdown> ();		
		}

	    public void OnDropdownChange(){
			
			if (_dropdown.options [_dropdown.value].text == "Avatar") {
				avatarName.SetActive (true);
				actorHeight.SetActive (true);
				skeletonName.SetActive (true);
                recordingFPS.SetActive(true);
				sceneDropdown.gameObject.SetActive (false);
			} else if (_dropdown.options [_dropdown.value].text == "Master") {
				
				avatarName.SetActive (false);
				actorHeight.SetActive (false);
				skeletonName.SetActive (false);
			    recordingFPS.SetActive(false);
                sceneDropdown.gameObject.SetActive (true);
			} else {
				avatarName.SetActive (false);
				actorHeight.SetActive (false);
				skeletonName.SetActive (false);
			    recordingFPS.SetActive(false);
                sceneDropdown.gameObject.SetActive (false);
			}

		}

		#endregion
	}
}