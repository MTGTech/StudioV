using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddFakeVeleocityComponents : MonoBehaviour {

	public List<Collider> collidersToIgnore = new List<Collider>();
	public PhysicMaterial physicMaterial;
	Collider[] avatarCols;
	// Use this for initialization
	void Awake () {
		avatarCols = GetComponentsInChildren<Collider> ();

		foreach (Collider c in avatarCols) {
			collidersToIgnore.Add (c);
		}
		foreach (Collider c in avatarCols) {
			c.gameObject.AddComponent<FakeVelocity> ();
			c.GetComponent<FakeVelocity>().IgnoreColliders(collidersToIgnore);
			c.GetComponent<FakeVelocity> ().isAvatar = true;
			c.material = physicMaterial;
//			c.GetComponent<MeshRenderer> ().enabled = true;
			c.transform.parent = transform.parent.parent;
		}
		GameManager.OnSceneAsyncDone += FindAndIgnoreSceneColliders ;
		FindAndIgnoreSceneColliders ();
	}
	public void FindAndIgnoreSceneColliders(){
		GameObject[] sceneCols = GameObject.FindGameObjectsWithTag ("SceneColliders");
		Debug.Log ("FindAndIgnoreSceneColliders()");
		foreach (GameObject c in sceneCols){
			Debug.Log ("Ignoring gameobject " + sceneCols);
			if (c.GetComponent<Collider>() != null) {
				Debug.Log ("with collider " + c.GetComponent<Collider>());

				collidersToIgnore.Add (c.GetComponent<Collider> ());
			}
		}
		foreach (Collider c in avatarCols) {
			c.GetComponent<FakeVelocity> ().IgnoreColliders (collidersToIgnore);
		}
	}
}
