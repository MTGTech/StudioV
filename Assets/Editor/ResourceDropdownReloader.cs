using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ResourceDropdownReloader : MonoBehaviour {

	[MenuItem("Assets/Reload Resources Into Dropdowns")]
	static void ReloadResourcesIntoDropdowns(){
		ResourceDropdown[] _dropdowns = FindObjectsOfTypeAll (typeof(ResourceDropdown)) as ResourceDropdown[];
		foreach(ResourceDropdown _rd in _dropdowns){
			_rd.GenerateResourceDropdowns ();
		}
	}
}
