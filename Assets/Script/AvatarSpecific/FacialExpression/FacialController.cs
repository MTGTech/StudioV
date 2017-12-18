using System;
using Tobii.Research.Unity.CodeExamples;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class FacialController : MonoBehaviour {

	[HideInInspector] public PhotonView photonView;

    #region public field
    // Use face mesh to record blendshapes data and 
    // identify blink blendshapes to avoid nesting
    public SkinnedMeshRenderer HmdFace;
    public SkinnedMeshRenderer OptitrackFace;

    [Tooltip("For other meshes that also have blendshapes")]
    public Transform[] OtherMeshes;

    // Lerping rate for facial expression, blink and eye movement
    [Header("Lerp Speed")]
    public float FaceLerpSpeed = 20f;
    public float BlinkBlendSpeed = 4f;
    public float EyeMoveLerpSpeed = 20f;

    // Eye movement controller
    [Header("HMD")]
    public Transform eye_L;
    public Transform eye_R;
    [Header("Optitrack")]
    public Transform r_eye_L;
    public Transform r_eye_R;
    public bool EnableWink = false;

    // Data recording settings  
    private int RecordingFPS;

    [HideInInspector]
    public float[] expressionWeights =
    {
        1.0f, // "JawOpen"s
        1.0f, // "JawLeft"
        1.0f, // "JawRight"
        1.0f, // "JawFwd"
        0.8f, // "LipsUpperUp_L"
        0.8f, // "LipsUpperUp_R"
        0.8f, // "LipsLowerDown_L"
        0.8f, // "LipsLowerDown_R"
        1.0f, // "LipsUpperClose"
        1.0f, // "LipsLowerClose"
        1.0f, // "MouthSmile_L"
        1.0f, // "MouthSmile_R"
        1.8f, // "LipStretch_L"
        1.8f, // "LipStretch_R"
        1.5f, // "MouthFrown_L"
        1.5f, // "MouthFrown_R"
        0.8f, // "LipsPucker"
        0.8f, // "LipsFunnel"
        1.0f, // "MouthLeft"
        1.0f, // "MouthRight"
        1.0f  // "Puff"
    };
    [HideInInspector]
    public string[] expressionTargetNames = {
        ".JawOpen",
        ".JawLeft",
        ".JawRight",
        ".JawFwd",
        ".LipsUpperUp_L",
        ".LipsUpperUp_R",
        ".LipsLowerDown_L",
        ".LipsLowerDown_R",
        ".LipsUpperClose",
        ".LipsLowerClose",
        ".MouthSmile_L",
        ".MouthSmile_R",
        ".LipsStretch_L",
        ".LipsStretch_R",
        ".MouthFrown_L",
        ".MouthFrown_R",
        ".LipsPucker",
        ".LipsFunnel",
        ".MouthLeft",
        ".MouthRight",
        ".Puff"
    };
    [HideInInspector]
    public string[] expressionTargets;

    #endregion

    #region private field

    // Facial Expression controller
    private FaceExpressionController _faceExpressionController;   
    private float[] _blendShapes;           // BlendShapes value   
    private float[] _newBlendShapes;        // BlendShapes value on remote player

    // Eye blink controller
    private float _leftBlinkTimeStamp = 0.0f;
    private float _leftPrevBlinkTimeStamp;
    private float _rightBlinkTimeStamp = 0.0f;
    private float _rightPrevBlinkTimeStamp;

    private float _leftEyeOpenness;
    private float _rightEyeOpenness;
    private float _leftBlinkBlendShape;
    private float _rightBlinkBlendShape;
    private float _newLeftBlinkBlendShape;
    private float _newRightBlinkBlendShape;

    private float t_EyeL;
    private float t_EyeR;
    private int _blinkLeftBlendShapeId = -1;
    private int _blinkRightBlendShapeId = -1;

    // Eye movement controller
    private Vector3 _gazeDirection;
    private Vector3 new_gazeDirection;
    private SubscribingToHMDGazeData _subscribingGazeData;

    // Recording
    private Button _startRecording;
    private Button _stopRecording;
    // Separate thread for data recording
    public RecordData NewRecord;

    private RecordingManager _recordingManager;

    #endregion

    void Awake()
    {
        if (photonView.isMine)
        {
			_recordingManager = FindObjectOfType<RecordingManager>();
			
        }

    }

    void Start()
    {
		_faceExpressionController = GetComponentInParent<FaceExpressionController>();
        _blendShapes = new float[_faceExpressionController.BlendShapeWeights.Length];
        _newBlendShapes = new float[_faceExpressionController.BlendShapeWeights.Length];

        if (HmdFace != null)
        {
            var mesh = HmdFace.sharedMesh;
            for (int j = 0; j < mesh.blendShapeCount; j++)
            {
                var blendShapeName = mesh.GetBlendShapeName(j);

                if (blendShapeName.Contains("Blink_L"))
                    _blinkLeftBlendShapeId = j;
                if (blendShapeName.Contains("Blink_R"))
                    _blinkRightBlendShapeId = j;
            }
        } 

      


        if (photonView.isMine) {

		    Camera eyeCam = transform.GetComponent<PlayerManager>()._camera.GetComponent<Camera>();

            TobiiPro_Host.Instance.SetCameraUsedToRender(eyeCam);
            _subscribingGazeData = SubscribingToHMDGazeData.SubscribingInstance;


            // Create a new thread for data recording
            RecordingFPS = _recordingManager.RecordingFPS;
            NewRecord = new RecordData();
		    NewRecord.BlendShapeNumber = HmdFace.sharedMesh.blendShapeCount;
		    NewRecord.BlendShapeValues = new float[NewRecord.BlendShapeNumber];
		    NewRecord.BlendShapeNames = new string[NewRecord.BlendShapeNumber];
		    NewRecord.ThreadSleepTime = Mathf.FloorToInt(1000 / RecordingFPS);


            if (_recordingManager.IsRecording)
            {               
                _recordingManager.StartRecording();
            }
		}
	}


    void FixedUpdate()
	{
		if (photonView.isMine) {
			if (_faceExpressionController == null)
				return;
			_blendShapes = _faceExpressionController.BlendShapeWeights;

            // Sync data to another thread for recording

            _gazeDirection = _subscribingGazeData.GazeDirection;
            _leftEyeOpenness = _subscribingGazeData.LeftEyeOpenness;  
			_rightEyeOpenness = _subscribingGazeData.RightEyeOpenness; 
            
            if (_recordingManager.IsRecording && HmdFace != null)
            {
                for (int i = 0; i < HmdFace.sharedMesh.blendShapeCount; i++)
                {
                    NewRecord.BlendShapeValues[i] = HmdFace.GetBlendShapeWeight(i);
                    NewRecord.BlendShapeNames[i] = HmdFace.sharedMesh.GetBlendShapeName(i);
                }

                NewRecord.GazeDirection = _gazeDirection;

            }
            // Set eye gaze direction
            if (TobiiPro_Host.EyeTrackerInstance != null && !TobiiPro_Host.isEyeTrackerConnected)
            {
                eye_L.transform.localRotation = Quaternion.identity;
                eye_R.forward = eye_L.forward;

                r_eye_L.transform.localRotation = Quaternion.identity;
                r_eye_R.forward = r_eye_L.forward;

                _gazeDirection = eye_L.forward;
            }
            else {
                // global rotation
                eye_L.forward = Vector3.Lerp(eye_L.forward, _gazeDirection, Time.deltaTime * EyeMoveLerpSpeed);
                eye_R.forward = eye_L.forward;

                r_eye_L.forward = Vector3.Lerp(r_eye_L.forward, _gazeDirection, Time.deltaTime * EyeMoveLerpSpeed);
                r_eye_R.forward = r_eye_L.forward;
            }     

        } else {
            // Lerp eye movement
		    eye_L.forward = Vector3.Lerp(eye_L.forward, new_gazeDirection, Time.deltaTime * EyeMoveLerpSpeed);
		    eye_R.forward = eye_L.forward;

		    r_eye_L.forward = Vector3.Lerp(r_eye_L.forward, new_gazeDirection, Time.deltaTime * EyeMoveLerpSpeed);
		    r_eye_R.forward = r_eye_L.forward;

            // Lerp the blendshape
            for (int i = 0; i < _newBlendShapes.Length; i++) {
				_blendShapes [i] = Mathf.Lerp (_blendShapes[i], _newBlendShapes[i], Time.deltaTime * FaceLerpSpeed);
			}
        }

        // Set blendshapes
        if (_blendShapes != null && _blendShapes.Length == expressionTargets.Length)
        {
            UpdateBlendShapes(HmdFace.gameObject.transform);
            UpdateBlendShapes(OptitrackFace.gameObject.transform);
            if (OtherMeshes != null)
            {
                foreach (Transform mesh in OtherMeshes)
                {
                    UpdateBlendShapes(mesh);
                }
            }
        }

        UpdateEyeBlink(HmdFace);
        UpdateEyeBlink(OptitrackFace);

    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            if (_blendShapes == null)
                return;

            // create a byte array and copy the floats into it...
            var byteArray = new byte[_blendShapes.Length * 4];
            Buffer.BlockCopy(_blendShapes, 0, byteArray, 0, byteArray.Length);
            stream.SendNext(byteArray);
            stream.SendNext(EnableWink);
            stream.SendNext(_leftEyeOpenness);
            stream.SendNext(_rightEyeOpenness);
            stream.SendNext(_gazeDirection);
        }
        else
        {
            //recive float array in byte
            byte[] byteArray = (byte[])stream.ReceiveNext();
            // create a second float array and copy the bytes into it...
            _newBlendShapes = new float[byteArray.Length / 4];
            Buffer.BlockCopy(byteArray, 0, _newBlendShapes, 0, byteArray.Length);
            EnableWink = (bool)stream.ReceiveNext();
            _leftEyeOpenness = (float)stream.ReceiveNext();
            _rightEyeOpenness = (float)stream.ReceiveNext();
            new_gazeDirection = (Vector3)stream.ReceiveNext();
        }
    }



    void UpdateBlendShapes(Transform gameObject)
    {
        //  Update this
        SkinnedMeshRenderer renderer = gameObject.GetComponent<SkinnedMeshRenderer>();
        if (renderer != null)
        {
            var mesh = renderer.sharedMesh;
            for (int i = 0; i < expressionTargets.Length; i++)
            {
                var expressionName = expressionTargets[i];
                var expressionWeight = expressionWeights[i];

                for (int j = 0; j < mesh.blendShapeCount; j++)
                {
                    var blendShapeName = mesh.GetBlendShapeName(j);
                    if (blendShapeName.Contains(expressionName))
                    {
                        renderer.SetBlendShapeWeight(j, _blendShapes[i] * 100.0f * expressionWeight);
                    }
                }
            }
        }    
    }

    void UpdateEyeBlink(SkinnedMeshRenderer faceRenderer)
    {
 
        if (faceRenderer != null)
        {
            
            if(TobiiPro_Host.EyeTrackerInstance!=null && !TobiiPro_Host.isEyeTrackerConnected)
            {
                faceRenderer.SetBlendShapeWeight(_blinkLeftBlendShapeId, 0f);
                faceRenderer.SetBlendShapeWeight(_blinkRightBlendShapeId, 0f);
                return;
            }

            _leftBlinkBlendShape = faceRenderer.GetBlendShapeWeight(_blinkLeftBlendShapeId);
            _rightBlinkBlendShape = faceRenderer.GetBlendShapeWeight(_blinkRightBlendShapeId);

            if (_leftBlinkTimeStamp - _leftPrevBlinkTimeStamp > 1.0f || _leftBlinkBlendShape != (1 - _leftEyeOpenness) * 100.0f)
            {
				faceRenderer.SetBlendShapeWeight(_blinkLeftBlendShapeId, Mathf.Lerp(_leftBlinkBlendShape, (1 - _leftEyeOpenness) * 100.0f, t_EyeL));
                t_EyeL += BlinkBlendSpeed * Time.deltaTime;


                if (t_EyeL >= 1.0f)
                {
					faceRenderer.SetBlendShapeWeight(_blinkLeftBlendShapeId, (1 - _leftEyeOpenness) * 100.0f);
                    t_EyeL = 0.0f;

                    // Take the timestamp after lerp to blink
                    if (_leftEyeOpenness == 0)
                    {
                        _leftPrevBlinkTimeStamp = Time.time;
                    }
                }      
            }
   
            if (EnableWink)
            {
                if (_rightBlinkTimeStamp - _rightPrevBlinkTimeStamp > 1.0f || _rightBlinkBlendShape != (1 - _rightEyeOpenness) * 100.0f)
                {
					faceRenderer.SetBlendShapeWeight(_blinkRightBlendShapeId, Mathf.Lerp(_rightBlinkBlendShape, (1 - _rightEyeOpenness) * 100.0f, t_EyeR));

                    t_EyeR += BlinkBlendSpeed * Time.deltaTime;


                    if (t_EyeR >= 1.0f)
                    {
						faceRenderer.SetBlendShapeWeight(_blinkRightBlendShapeId, (1 - _leftEyeOpenness) * 100.0f);
                        t_EyeR = 0.0f;

                        // Take the timestamp after lerp to blink
                        if (_rightEyeOpenness == 0)
                        {
                            _rightPrevBlinkTimeStamp = Time.time;
                        }
                    }
                }
            }
            else {
                faceRenderer.SetBlendShapeWeight(_blinkRightBlendShapeId, _leftBlinkBlendShape);
            }      
        }
    }
        


    public void ClapInDataRecording()
    {
        // For clap
        // TODO: communicate with recording thread

        for (int i = 0; i < HmdFace.sharedMesh.blendShapeCount; i++)
        {
            NewRecord.BlendShapeValues[i] = 1f;
            NewRecord.sw_facial.WriteLine(NewRecord.FrameCount + "," +
                                           (DateTime.Now - NewRecord.StartRecordingTime) +
                                           "," + HmdFace.sharedMesh.GetBlendShapeName(i) + ",100");
        }

        NewRecord.sw_eye.WriteLine(NewRecord.FrameCount + "," + (DateTime.Now - NewRecord.StartRecordingTime) + ",0,0,0");
    }
}
