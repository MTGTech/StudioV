using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(PhotonView))]
public class CameraMarker : Photon.MonoBehaviour {

	[SerializeField]Color colorWhenSelected;
	Color baseColor;
	Renderer m_Renderer;
	int viewId;
	// Use this for initialization
	void Start () {
		if (GetComponent<PhotonView>() != null) {
			viewId = photonView.viewID;
		} else {
			viewId = 0;
		}
		m_Renderer = GetComponent<Renderer> ();
		baseColor = m_Renderer.material.color;
	}
	public void SetAsSelected(bool b){
		photonView.RPC("RPC_SetAsSelected", PhotonTargets.All, new object[]{ b, viewId});
	}
	[PunRPC]
	public void RPC_SetAsSelected(bool b, int id){
		if (viewId == id) {
			if (b) {
				m_Renderer.material.color = colorWhenSelected;
			} else {
				m_Renderer.material.color = baseColor;
			}
		}
	}
}
