using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class ClapperBoard : Photon.MonoBehaviour {


	[SerializeField] float clapDuration;
	[SerializeField] Text m_Time;
	[SerializeField] Text m_Date;
	[SerializeField] InputField[] Inputfields;

 	long timeOffsetTicks;
    double syncBufferTime;
    Canvas c_Canvas;
	AudioSource m_AudioSource;

	void Start () {
		m_AudioSource = GetComponent<AudioSource> ();
		c_Canvas = GetComponentInChildren<Canvas> ();
	}

	void Update () {
		CheckForInput ();
	}

	void CheckForInput(){
		//Toggle clapper edit mode
		if (Input.GetKeyDown (KeyCode.F1)) {
			ToggleClapperUI ();
		}

		// Clap only when is recording
		if (Input.GetKeyDown (KeyCode.F2))
		{           
			photonView.RPC("RPC_SetTimeToClap", PhotonTargets.All, new object[] { });
		}
	}
	void FixedUpdate(){
		//calculating elepced time
		long elapcedTicks = DateTime.Now.Ticks + timeOffsetTicks;
		DateTime dt = new DateTime(1,01,01);
		DateTime curTime = dt.AddTicks (elapcedTicks);
		m_Time.text = curTime.ToString("HH:mm:ss");
		m_Date.text = curTime.Date.ToString("yyyy/MM/dd");
	}
	public void OnJoinedRoom (){
		photonView.RPC ("RPC_RequestHostDateTime", PhotonTargets.MasterClient, new object[]{});
	}

	void ToggleClapperUI(){
		c_Canvas.enabled = !c_Canvas.enabled;
	}

	//set a timestamp to clap on other clients
	[PunRPC]
	public void RPC_SetTimeToClap(){
		PlayerManager localPlayer = null;

		foreach (var player in GameManager.Instance.avatars)
	    {
			if (player.transform.parent.GetComponent<PhotonView>().isMine)
	            localPlayer = player;
	    }

	    syncBufferTime = 0;
	    foreach (PhotonPlayer player in PhotonNetwork.playerList)
	    {

			var tempBufferTime = (int) player.CustomProperties["Ping"];

			if (tempBufferTime > syncBufferTime)
				syncBufferTime = tempBufferTime;

	    }
		
	    StartCoroutine(ClapAtSetTime (PhotonNetwork.time + syncBufferTime/1000, localPlayer));
    }

	IEnumerator ClapAtSetTime(double mark, PlayerManager localAvatar){
		while (PhotonNetwork.time < mark) {
			yield return new WaitForEndOfFrame ();
		}
		m_AudioSource.Stop ();
		m_AudioSource.Play ();
		c_Canvas.enabled = true;

        // Set a clap in data recording
	    if (localAvatar != null)
	    {
	        localAvatar.GetComponent<FacialController>().ClapInDataRecording();
	    }

        yield return new WaitForSeconds (clapDuration);
		c_Canvas.enabled = false;	    
    }


    public void SetRemoteProductionName(){
		photonView.RPC ("RPC_SetInputField", PhotonTargets.Others, new object[]{0,Inputfields[0].text});
		Debug.Log ("sync field");
	}
	public void SetRemoteSceneName(){
		photonView.RPC ("RPC_SetInputField", PhotonTargets.Others, new object[]{1,Inputfields[1].text});
	}
	public void SetRemoteTakeNr(){
		photonView.RPC ("RPC_SetInputField", PhotonTargets.Others, new object[]{2,Inputfields[2].text});
	}
	[PunRPC]
	public void RPC_RequestHostDateTime(){
		byte[] b = BitConverter.GetBytes (DateTime.Now.Ticks);
		photonView.RPC ("RPC_SendHostDateTimeToAll", PhotonTargets.Others, new object[]{b});
	}
	[PunRPC]
		public void RPC_SendHostDateTimeToAll(byte[] b){
		DateTime hostsDateTimestamp = DateTime.FromBinary(BitConverter.ToInt64(b,0));
		timeOffsetTicks = hostsDateTimestamp.Ticks - DateTime.Now.Ticks;
	}


	[PunRPC]
	public void RPC_SetInputField(int fieldNR, string newString){
		Debug.Log ("are things happening?" + fieldNR + " " + newString);
		Inputfields [fieldNR].text = newString;
	}
}
