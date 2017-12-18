using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fallback : MonoBehaviour {
	public enum ReadyDefaultResource{
		Avatar,
		Rigidbody,
		PlayerType
	}
	[HideInInspector]public ReadyDefaultResource readyDefaultResource;

	public GameObject defaultAvatar;
	public GameObject defaultRigidbody;
	public GameObject defaultPlayerType;

	[HideInInspector] public static Fallback Instance;
	public delegate void RefreshInUpdate();
	public static event RefreshInUpdate OnRefreshedUpdate;
	public delegate void RefreshInIntervall();
	public static event RefreshInIntervall OnRefreshedIntervall;

	public GameObject messageWindow;

	public float intervall = 2f;
	int m_messageCount = 5;

	void Awake(){
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy (gameObject);
		}
		StartCoroutine (Intervall());
	}
	public GameObject InstantiateDefaultResource(){
		switch(readyDefaultResource){
		case ReadyDefaultResource.Avatar:
//			return Instantiate (defaultAvatar, Vector3.zero, Quaternion.identity) as GameObject;
			break;
		case ReadyDefaultResource.PlayerType:
			return Instantiate (defaultPlayerType, Vector3.zero, Quaternion.identity) as GameObject;
			break;
		case ReadyDefaultResource.Rigidbody:
			return Instantiate (defaultRigidbody, Vector3.zero, Quaternion.identity) as GameObject;
			break;
		}
		return null;
	}
	void Update(){
		if (OnRefreshedUpdate != null) {
			OnRefreshedUpdate ();
		}
	}
	IEnumerator Intervall(){
		while (true) {
			yield return new WaitForSeconds (intervall);
			if (OnRefreshedIntervall != null) {
				OnRefreshedIntervall ();
			}
		}
	}
	public MessageWindow SendErrorMessage(string _message){
		GameObject _window = Instantiate (messageWindow, Vector3.zero, Quaternion.identity, transform)as GameObject;
		MessageWindow _messageWindow = _window.GetComponent<MessageWindow> ();
		_messageWindow.messageText.text = _message;
		_messageWindow.GetComponent<Canvas> ().sortingOrder = m_messageCount;
		m_messageCount++;
		return _messageWindow;
	}
}
