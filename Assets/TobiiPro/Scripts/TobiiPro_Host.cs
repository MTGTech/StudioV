using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Research;

public class TobiiPro_Host : MonoBehaviour {

    public static IEyeTracker EyeTrackerInstance;
	public bool IsCalibrating = false; // TODO
    public static bool isEyeTrackerConnected;
    public Camera eyetrackerCam;

    /// <summary>
    /// The transform used to change from eye tracking coordinate to world space. 
    /// Default is Camera.main.transform
    /// </summary>
    public Transform LocalToWorldTransform { get { return _localToWorldTransform; } }
    private Transform _localToWorldTransform;

	private static TobiiPro_Host _instance;

    // Use this for initialization
    void Awake () {

        if (_instance == null) {

            _instance = this;

            bool result = false;
            var trackers = EyeTrackingOperations.FindAllEyeTrackers();
            EyeTrackerInstance = trackers.FirstOrDefault(s => (s.DeviceCapabilities & Capabilities.HasHMDGazeData) != 0);
            if (EyeTrackerInstance == null)
            {
                result = false;

                Debug.Log("No HMD eye tracker detected!");
            }
            else
            {
                result = true;
                isEyeTrackerConnected = true;
                Debug.Log("Selected eye tracker with serial number {0}" + EyeTrackerInstance.SerialNumber);
            }


            if (result == false)
            {
                Debug.LogError("TobiiPro: Failed to create tracker.");
            }
        }
            

    }

    private void Start()
    {
        if (Instance.eyetrackerCam.gameObject.activeSelf == false)
        {
            Debug.LogWarning("TobiiPro: Camera(eye) is not enabled. Pick the main camera instead.");
            Instance.SetCameraUsedToRender(Camera.main);
        }
        else
        {
            Instance.SetCameraUsedToRender(eyetrackerCam);
        }

        if (EyeTrackerInstance != null) {
            EyeTrackerInstance.ConnectionLost += EyeTracker_ConnectionLost;
            EyeTrackerInstance.ConnectionRestored += EyeTracker_ConnectionRestored;
        }
        

    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    public void Init(Camera camera = null)
    {
        if (camera == null) return;

        SetCameraUsedToRender(camera);
    }

    /// <summary>
    /// SteamVR renders the left and right eye 15mm behind the HMD origin and
    /// eye tracking origin is between the lenses it has to be offseted.
    /// </summary>
    /// <param name="newCamera">Should be considered the "main" camera that renders the scene.</param>
    public void SetCameraUsedToRender(Camera newCamera)
    {
        if (newCamera == null) return;

        if (LocalToWorldTransform != null)
        {
            Destroy(LocalToWorldTransform.gameObject);
        }

        // Default offset.
        var zOffs = 0.015f;

        var error = -1;

        // Reflection is used to remove the dependency to have SteamVR in the package.
        var steamVR = Type.GetType("SteamVR");
        if (steamVR != null)
        {
            var instance = steamVR.GetProperty("instance").GetValue(null, null);
            if (instance != null)
            {
                var steamVRActive = steamVR.GetProperty("active").GetValue(null, null);
                if (steamVRActive != null && (bool)steamVRActive == true)
                {
                    var hmd = instance.GetType().GetProperty("hmd").GetValue(instance, null);
                    var method = hmd.GetType().GetMethod("GetFloatTrackedDeviceProperty");
                    var props = method.GetParameters()[1].ParameterType;

                    var arguments = new object[] { (uint)0, Enum.Parse(props, "Prop_UserHeadToEyeDepthMeters_Float"), error };

                    var hmdHeadToEyeDepthMeters = (float)method.Invoke(hmd, arguments);
                    error = (int)arguments[2];

                    if (error == 0)
                    {
                        zOffs = hmdHeadToEyeDepthMeters;
                    }
                }
            }
        }

        if (error != 0)
        {
            Debug.LogWarning("Failed to get the offset from head to eye. Setting default " + zOffs + "m");
        }

        var eyeTrackerOrigin = new GameObject("TobiiPro_Origin");
        eyeTrackerOrigin.transform.parent = newCamera.transform;
        eyeTrackerOrigin.transform.localScale = Vector3.one;
        eyeTrackerOrigin.transform.localPosition = new Vector3(0, 0, zOffs);
        eyeTrackerOrigin.transform.localRotation = Quaternion.identity;

        _localToWorldTransform = eyeTrackerOrigin.transform;
    }

	public static TobiiPro_Host Instance
	{
		get
		{
			if (_instance != null)
			{
				return _instance;
			}
				
			var newGameObject = new GameObject("TobiiPro_Host");
			DontDestroyOnLoad(newGameObject);
			_instance = newGameObject.AddComponent<TobiiPro_Host>();

			return _instance;
		}
	}

    private void OnDisable()
    {
        if (EyeTrackerInstance != null) {
            EyeTrackerInstance.ConnectionLost -= EyeTracker_ConnectionLost;
            EyeTrackerInstance.ConnectionRestored -= EyeTracker_ConnectionRestored;
        }
        
    }

    private static void EyeTracker_ConnectionLost(object sender, ConnectionLostEventArgs e)
    {
        isEyeTrackerConnected = false;
        Debug.Log(string.Format("Connection to the eye tracker was lost at time stamp {0}.", e.SystemTimeStamp));
    }

    private static void EyeTracker_ConnectionRestored(object sender, ConnectionRestoredEventArgs e)
    {
        isEyeTrackerConnected = true;
        Debug.Log(string.Format("Connection to the eye tracker was restored at time stamp {0}.", e.SystemTimeStamp));
    }
}
