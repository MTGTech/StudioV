using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AlienController : EyeRaycasterTarget {

	NavMeshAgent agent;
	Animator anim;

	void Start () {
		agent = GetComponent<NavMeshAgent> ();
		agent.SetDestination (Vector3.zero);
		anim = GetComponentInChildren<Animator> ();
	}
	public void Stop(){
		photonView.RPC ("RPC_AlienStopWalking", PhotonTargets.All, new object[]{ });
		agent.enabled = false;
	}
	public override void LookedAt(){
		photonView.RPC ("RPC_AlienStopWalking", PhotonTargets.All, new object[]{ });
	}
	public override void LookExit(){
		photonView.RPC ("RPC_AlienStartWalking", PhotonTargets.All, new object[]{ });
	} 
	[PunRPC]
	void RPC_AlienStopWalking(){
		anim.SetBool ("walking", false);
		if (agent.enabled) {
			agent.SetDestination (transform.position);
		}
	}
	[PunRPC]
	void RPC_AlienStartWalking(){
		if (agent.enabled) {
			anim.SetBool ("walking", true);
			agent.SetDestination (Vector3.zero);
		}
	}
	void OnTriggerEnter(Collider col){
		Debug.Log ("STAHP");
		if(col.gameObject.tag == "Alien"){
			StartCoroutine (StopDelay ());
		}
	}
	IEnumerator StopDelay(){
		yield return new WaitForSeconds (3);
		Stop ();
	}
}
