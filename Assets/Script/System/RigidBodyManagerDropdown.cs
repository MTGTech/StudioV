using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class RigidBodyManagerDropdown : MonoBehaviour {
	Dropdown dd;
	public GameObject idInput;
	public GameObject newRigidbodyDropdown;
	public GameObject deleteRigidbodyDropdown;
	public Text buttonText;


	void Start(){
		dd = GetComponent<Dropdown> ();
		dd.options.Add (new Dropdown.OptionData (){ text = "Add Rigidbody" });
		dd.options.Add (new Dropdown.OptionData (){ text = "Delete Rigidbody" });
		dd.RefreshShownValue ();
	}
	public void UpdateId(InputField ifield){
		try{
			ifield.text = ManagerMenu.Instance.rigidbodies [dd.value - 1].GetComponent<OptitrackRigidBody>().RigidBodyId.ToString();
			Debug.Log ("Numbeeer: " + ManagerMenu.Instance.rigidbodies [dd.value - 1].GetComponent<OptitrackRigidBody> ().RigidBodyId.ToString ());
		}catch{
			//When player selected to spawn new, or delete a rigidbody
		}
	}
	public void UpdateOptions(){
		if (dd.value == 0) {
			//Add rigidbody
			idInput.SetActive (true);
			newRigidbodyDropdown.SetActive (true);
			deleteRigidbodyDropdown.SetActive (false);
			buttonText.text = "Add Rigidbody";
		} else if (dd.value == 1) {
			//Delete Rigidbody
			idInput.SetActive (false);
			newRigidbodyDropdown.SetActive (false);
			deleteRigidbodyDropdown.SetActive (true);
			buttonText.text = "Delete Rigidbody";
		} else {
			//Replace Rigidbody
			idInput.SetActive (true);
			newRigidbodyDropdown.SetActive (true);
			deleteRigidbodyDropdown.SetActive (false);
			buttonText.text = "Replace Rigidbody";
		}
	}
}
