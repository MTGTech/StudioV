using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Diagnostics;
using MicroLibrary;
using UnityEngine;

public class RecordingCameraClient : Photon.MonoBehaviour{
	[HideInInspector]public CameraPosRecordData recording;
//	public string filePath;

	void FixedUpdate(){
		if (recording != null) {
			recording.position = transform.position;
			recording.rotation = transform.rotation.eulerAngles;
		}
	}
	public void StartRecording(){
		photonView.RPC ("RPC_StartRecording", PhotonTargets.AllBuffered);
	}
	public void StopRecording(){
		photonView.RPC ("RPC_StopRecording", PhotonTargets.AllBuffered);
	}
	[PunRPC]
	public void RPC_StartRecording(){
		if (photonView.isMine) {
			recording = new CameraPosRecordData ();
			string path = "DataRecording/";
			string _cameraFilename = gameObject.name + "_fps" + 60;

			string nameBase = String.Format ("{0}_{1:yyyy-MM-dd_HH-mm-ss}", _cameraFilename, DateTime.Now);

			_cameraFilename = path + "CameraClientData_" + nameBase + ".txt";

			recording.sw_cameraPos = new StreamWriter (_cameraFilename);
			recording.Start ();

			GameManager.Instance.GetComponent<WorldTimer>().StartTimer();
		}
	}
	[PunRPC]
	public void RPC_StopRecording(){
		GameManager.Instance.GetComponent<WorldTimer>().StopTimer();
		GameManager.Instance.GetComponent<WorldTimer> ().ResetTimer ();

		if (photonView.isMine) {
			if (recording == null)
				return;
			if (recording.sw_cameraPos != null){
				recording.sw_cameraPos.Close();
			}
			recording.Abort();
			recording.MicroTimer.Enabled = false;
			recording.MicroTimer.Stop();
			recording.MicroTimer.Abort();
		}
	}
}

public class CameraPosRecordData : ThreadedJob {
	

	public StreamWriter sw_cameraPos;
	public Vector3 position;
	public Vector3 rotation;

	public int FrameCount;
	public int ThreadSleepTime = Mathf.FloorToInt(1000 / 60);
	public System.DateTime StartRecordingTime;

	public MicroTimer MicroTimer;


	protected override void ThreadFunction()
	{
		StartRecordingTime = System.DateTime.Now;

		MicroTimer = new MicroTimer();
		MicroTimer.MicroTimerElapsed += OnTimedEvent;

		MicroTimer.Interval = ThreadSleepTime * 1000; // Call micro timer every 1000us

		// Can choose to ignore event if late by Xµs (by default will try to catch up)
		MicroTimer.IgnoreEventIfLateBy = 50; // 50µs (0.05ms)

		MicroTimer.Enabled = true; // Starts timer

	}

	void OnTimedEvent(object sender,MicroTimerEventArgs timerEventArgs)
	{
		FrameCount = timerEventArgs.TimerCount;

		if (sw_cameraPos == null)
		{
			UnityEngine.Debug.Log(timerEventArgs.TimerCount + "," + (System.DateTime.Now - StartRecordingTime) + ",0,0,0");
		}
		else
		{
			sw_cameraPos.WriteLine(timerEventArgs.TimerCount + "," + (System.DateTime.Now - StartRecordingTime) + "," + position.x + "," + position.y + "," + position.z + "," + rotation.x + "," + rotation.y + "," + rotation.z);

//			for (int i = 0; i < BlendShapeNumber; i++)
//			{
//				sw_cameraPos.WriteLine(timerEventArgs.TimerCount + "," + (System.DateTime.Now - StartRecordingTime) + "," +
//					BlendShapeNames[i] + "," + BlendShapeValues[i]);
//			}

		}

		//        UnityEngine.Debug.Log(string.Format(
		//            "Count = {0:#,0}  Timer = {1:#,0} µs, " +
		//            "LateBy = {2:#,0} µs, ExecutionTime = {3:#,0} µs",
		//            timerEventArgs.TimerCount, timerEventArgs.ElapsedMicroseconds,
		//            timerEventArgs.TimerLateBy, timerEventArgs.CallbackFunctionExecutionTime));
	}
}
