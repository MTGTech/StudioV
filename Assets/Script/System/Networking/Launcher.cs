using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com.MTGTech.MyGame{
	
	public class Launcher : Photon.PunBehaviour {
		#region Public Variables
		/// <summary>
		/// The PUN loglevel. 
		/// </summary>
		public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;
		[Tooltip("The Ui Panel to let the user enter name, connect and play")]
		public GameObject controlPanel;

		[Tooltip("The Ui Text to inform the user about the connection progress")]
		public GameObject progressLable;

		public Text roomName;
		#endregion

		#region Private Variables
		/// <summary>
		/// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon, 
		/// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
		/// Typically this is used for the OnConnectedToMaster() callback.
		/// </summary>
		bool isConnecting;
		public Text spawnAsText;
		public Text sceneToLoadText;

		/// <summary>
		/// This client's version number. Users are separated from each other by gameversion (which allows you to make breaking changes).
		/// </summary>
		string _gameVersion = "1";
		#endregion


		#region MonoBehaviour CallBacks
		void Awake() {
            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.automaticallySyncScene = false;

            PhotonNetwork.autoJoinLobby = false;
			PhotonNetwork.logLevel = Loglevel;
		}

		void Start () {
			controlPanel.SetActive(true);
			progressLable.SetActive (false);
		}
		#endregion

		#region Public Methods		
		/// <summary>
		/// Start the connection process. 
		/// - If already connected, we attempt joining a random room
		/// - if not yet connected, Connect this application instance to Photon Cloud Network
		/// </summary>
		public void Connect(){
			isConnecting = true;
			// we check if we are connected or not, we join if we are , else we initiate the connection to the server.
			if (PhotonNetwork.connected) {
			    ManageMasterOnJoin();
			} else {
				PhotonNetwork.ConnectUsingSettings (_gameVersion);
			}
		}
		#endregion


		#region Photon.PunBehavior CallBacks
        // OnConnectedToMaster Server
		public override void OnConnectedToMaster (){
			if (isConnecting) {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
                ManageMasterOnJoin();
			}
		}

		public override void OnJoinedRoom (){
			if (PhotonNetwork.isMasterClient) {
				GameManager.Instance.LoadNewScene (sceneToLoadText.text);
			} else {
				GameManager.Instance.LoadCurrentScene ();
			}
		}
        #endregion

	    void ManageMasterOnJoin(){
			if (spawnAsText.text == "Master"){
				if (string.IsNullOrEmpty (roomName.text)) {
					Debug.Log ("Creating room studiov");
					PhotonNetwork.CreateRoom ("studiov");
				} else {
					Debug.Log("Creating room " + roomName.text);
					PhotonNetwork.CreateRoom (roomName.text);
				}
	        }
	        else{
				if (string.IsNullOrEmpty (roomName.text)) {
					PhotonNetwork.JoinRoom ("studiov");
				} else {
					PhotonNetwork.JoinRoom (roomName.text);

				}
	        }
        }

		[Header("Tab-able Inputfields")]
		public InputField skeletonName;
		public InputField actorHeight;
		public InputField recordedFrameRate;

		void Update(){
			if (spawnAsText.text == "Avatar" && Input.GetKeyDown (KeyCode.Tab)) {
				if (skeletonName.isFocused) {
					EventSystem.current.SetSelectedGameObject (actorHeight.gameObject, null);
					actorHeight.OnPointerClick (new PointerEventData (EventSystem.current));
				}else if (actorHeight.isFocused) {
					EventSystem.current.SetSelectedGameObject (recordedFrameRate.gameObject, null);
					recordedFrameRate.OnPointerClick (new PointerEventData (EventSystem.current));
				}else if (recordedFrameRate.isFocused) {
					EventSystem.current.SetSelectedGameObject (skeletonName.gameObject, null);
					skeletonName.OnPointerClick (new PointerEventData (EventSystem.current));
				}
			}
		}
			
		public Dropdown spawnAsDropdown;

		public void SetResourceName(){
			PhotonNetwork.playerName = spawnAsDropdown.options [spawnAsDropdown.value].text;
			if (PhotonNetwork.playerName == "Avatar") {
				Fallback.Instance.readyDefaultResource = Fallback.ReadyDefaultResource.Avatar;
			} else {
				Fallback.Instance.readyDefaultResource = Fallback.ReadyDefaultResource.PlayerType;
			}
			Debug.Log ("playerName: " + PhotonNetwork.playerName);
		}

		public void SetAvatarName(Text value){
			if(spawnAsDropdown.options [spawnAsDropdown.value].text == "Avatar"){
				PhotonNetwork.playerName = value.text;
			}
		}
		public void SetSkeletonName(Text value){
			PlayerPrefs.SetString ("SkeletonName", value.text);
		}
		public void SetActorHeight(Text value){
			if (!string.IsNullOrEmpty (value.text)) {
				PlayerPrefs.SetFloat ("ActorHeight", float.Parse (value.text));
			} else {
				PlayerPrefs.SetFloat ("ActorHeight", 1.6f);
			}
		}

		public void SetRecordingFPS(Text value)
		{
			if (!string.IsNullOrEmpty(value.text))
			{
				PlayerPrefs.SetInt("RecordingFPS", Int32.Parse(value.text));
				FindObjectOfType<RecordingManager>().RecordingFPS = Int32.Parse(value.text);
				FindObjectOfType<WorldTimer>().SetThreadSleepTime(Int32.Parse(value.text));
			}
		}
	}
}
