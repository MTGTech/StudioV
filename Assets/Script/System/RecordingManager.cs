using System;
using System.Diagnostics;
using System.IO;
using System.Security.Authentication;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
// OBS websocket plugin target for .Net 4, will only work on build
#if !UNITY_EDITOR
//using OBSWebsocketDotNet;
#endif
using Debug = UnityEngine.Debug;

public class RecordingManager : Photon.MonoBehaviour {

    private GameObject _recordingPanel;
    private Text _recordedTime;
    private TimeSpan _recordedTimeSpan;
    private RecordData _recordingThread;
	private CameraPosRecordData _cameraRecordingThread;
    private WorldTimer _worldTimer;

    private String _facialFilename;
    private String _eyeFilename;

    [HideInInspector]
    public bool IsRecording = false;

    [HideInInspector]
    public int RecordingFPS;

    [HideInInspector]
    public int FrameOnAvatarChange;

    //#if !UNITY_EDITOR
    //    protected OBSWebsocket _obs;
    // For check obs state in real-time
    //    public string OBSState;
    //#endif
    void Awake() {
        RecordingFPS = 60;
    }
    // Use this for initialization
    void Start () {
        
        _recordingPanel = GameObject.Find("RecordingPanel");
        _recordingPanel.SetActive(false);

        _recordedTime = _recordingPanel.transform.Find("RecordingTimer").GetComponent<Text>();
        _worldTimer = GetComponent<WorldTimer>();

//        if (Process.GetProcessesByName("obs64").Length > 0 || Process.GetProcessesByName("obs32").Length > 0)
//        {
//            _obs = new OBSWebsocket();
//            connectOBS();
//            _obs.OnRecordingStateChange += onRecordingStateChange;
//        }
//        else
//        {
//            Debug.LogError("OBSWebsocket: OBS Studio is not running. Please start OBS.");
//        }
//
//
//        //Start obs recording when Unity start
//        if (_obs != null)
//            StartOBSRecording();

    }

    // Update is called once per frame
    void Update () {

	    if (IsRecording)
	    {
	        _recordedTimeSpan = _worldTimer.ElapsedTimeSinceStart;
	        _recordedTime.text = String.Format("{0:00}:{1:00}:{2:00}",_recordedTimeSpan.Hours, _recordedTimeSpan.Minutes, _recordedTimeSpan.Seconds);
        }

    }

    public void StartRecording()
    {       
        photonView.RPC("RPC_StartRecord", PhotonTargets.All);
        GetComponent<RemoteTrigger>().SendMessageAndStartRecording();
		foreach (GameObject g in GameManager.Instance.cameraClients) {
			g.GetComponent<RecordingCameraClient> ().StartRecording ();
		}
    }


    public void StopRecording()
    {
        photonView.RPC("RPC_StopRecord", PhotonTargets.All);
        GetComponent<RemoteTrigger>().SendMessageAndStopRecording();
		foreach (GameObject g in GameManager.Instance.cameraClients) {
			g.GetComponent<RecordingCameraClient> ().StopRecording ();
		}
    }


    [PunRPC]
    public void RPC_StartRecord()
    {
        GameObject localPlayer = null;
		localPlayer = GameManager.Instance.localAvatar;
        if (localPlayer != null)
        {
            string path = "DataRecording/";
            _facialFilename = localPlayer.name + "_" + PlayerPrefs.GetString("SkeletonName") + "_fps" + RecordingFPS;
            _eyeFilename = localPlayer.name + "_" + PlayerPrefs.GetString("SkeletonName") + "_fps" + RecordingFPS;

            string nameBase = String.Format("{0}_{1:yyyy-MM-dd_HH-mm-ss}", _facialFilename, DateTime.Now);

            _facialFilename = path + "FacialData_" + nameBase + ".txt";
            _eyeFilename = path + "EyeData_" + nameBase + ".txt";

            _recordingThread = localPlayer.GetComponent<FacialController>().NewRecord;
            _recordingThread.sw_facial = new StreamWriter(_facialFilename);
            _recordingThread.sw_eye = new StreamWriter(_eyeFilename);

            // New a thread and start this thread
            _recordingThread.Start();

            IsRecording = true;
            GetComponent<WorldTimer>().StartTimer();


            if (_recordingPanel != null)
            {
                _recordingPanel.SetActive(true);

            }

        }
    }

    [PunRPC]
    public void RPC_StopRecord()
    {
		if (GameManager.Instance.localAvatar != null)
        {
            IsRecording = false;
            GetComponent<WorldTimer>().StopTimer();
			GetComponent<WorldTimer> ().ResetTimer ();

			if (_recordingPanel != null) {
				_recordingPanel.SetActive (false);
			}
            if (_recordingThread == null)
                return;
			if (_recordingThread.sw_eye != null) {
				_recordingThread.sw_eye.Close ();
			}
            if (_recordingThread.sw_facial != null){
                _recordingThread.sw_facial.Close();
            }
            _recordingThread.Abort();
            _recordingThread.MicroTimer.Enabled = false;
            _recordingThread.MicroTimer.Stop();
            _recordingThread.MicroTimer.Abort();
        }

    }


//    private void connectOBS()
//    {
//        var IP = "ws://127.0.0.1:4444";
//        var password = "";
//
//        try
//        {
//            _obs.Connect(IP, password);
//            
//        }
//        catch (AuthenticationException)
//        {
//            Debug.LogError("Authentication failed.");
//            return;
//        }
//        catch (ErrorResponseException ex)
//        {
//            Debug.LogError("Connect failed : " + ex.Message);
//            return;
//        }
//
//    }
//
//    private void onRecordingStateChange(OBSWebsocket sender, OutputState newState)
//    {
//
//        switch (newState)
//        {
//            case OutputState.Starting:
//                Debug.Log("OBS recording starting...");
//                OBSState = "Starting";
//                break;
//
//            case OutputState.Started:
//                Debug.Log("OBS is recording...");
//                OBSState = "Recording";
//                break;
//
//            case OutputState.Stopping:
//                Debug.Log("OBS recording stopping...");
//                OBSState = "Stopping";
//                break;
//
//            case OutputState.Stopped:
//                Debug.Log("OBS recording has stopped");
//                OBSState = "Stopped";
//                break;
//
//            default:
//                OBSState = "";
//                break;
//        }
//
//    }
//
//    public void StartOBSRecording()
//    {
//        if (OBSState == "" || OBSState == "Stopped")
//        {
//            _obs.ToggleRecording();
//        }
//        else
//        {
//            Debug.LogError("RecordingManager: You are trying to start recording when it's recording");
//        }
//        
//    }
//
//    public void StopOBSRecording()
//    {
//        if (OBSState == "Recording")
//        {
//            _obs.ToggleRecording();
//        }
//        else
//        {
//            Debug.LogError("RecordingManager: You are trying to stop recording when it's not recording");
//        }
//    }

    void OnApplicationQuit()
    {
        //Stop data recording when Unity quit if it's still writing
        if (IsRecording)
            StopRecording();
        // Stop OBS recording when Unity quit
//        if (_obs != null)
//        {
//            StopOBSRecording();
//            _obs.Disconnect();
//        }

	
    }
}
