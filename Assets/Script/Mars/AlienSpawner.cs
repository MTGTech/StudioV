using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienSpawner : Photon.MonoBehaviour {


	public GameObject alien;
	public Transform[] spawnPoints;
	public float spawnRate;
	bool started = false;
	int alienCounter;
	public int maxAlienCount = 30;

	// Use this for initialization
	void Start () {
		
	}
	void Update(){
		if (Input.GetKeyDown (KeyCode.F12) && !started) {
			StartCoroutine ("SpawnAliens");
		}
	}
	IEnumerator SpawnAliens(){
		while (alienCounter < maxAlienCount) {
			yield return new WaitForSeconds (spawnRate);
			int i = Random.Range (0, spawnPoints.Length);
			photonView.RPC ("RPC_SpawnAlien", PhotonTargets.MasterClient, new object[]{ i });
		}

	}
	[PunRPC]
	void RPC_SpawnAlien(int spawnID){
		PhotonNetwork.Instantiate ("Other/Alien", spawnPoints [spawnID].position, Quaternion.identity, 0);
		alienCounter++;
	}
}
