using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MessageWindow : MonoBehaviour {
	public Text messageText;
	public Button retry;
	public Button close;

	public delegate void RetryHandler();
	public static event RetryHandler OnRetry;

	public void Close(){
		Destroy (gameObject);
	}
	public void Retry(){
		OnRetry ();
	}
}
