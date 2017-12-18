using System.Collections.Generic;
using System.Linq;
using Tobii.Research;
using UnityEngine;


namespace Tobii.Research.Unity.CodeExamples
{
    // The events in the SDK are called on a thread internal to the SDK. That thread can not safely set values
    // that are to be read on the main thread. The simplest way to make it safe is to enqueue the date, and dequeue it
    // on the main thread, e.g. via Update() in a MonoBehaviour.
	public class SubscribingToHMDGazeData : MonoBehaviour
    {
        private IEyeTracker _eyeTracker;
        private Queue<HMDGazeDataEventArgs> _queue = new Queue<HMDGazeDataEventArgs>();

		private HMDGazeDataEventArgs _hmdEyeData;
        [HideInInspector]
        public bool isBoothEyesValid;
        [HideInInspector]
        public Vector3 GazeDirection;
        [HideInInspector]
        public int LeftEyeOpenness;
		[HideInInspector]
		public int RightEyeOpenness;

        private Vector3 _localGazeDirection;

        private static SubscribingToHMDGazeData _subscribingInstance;

        void Update()
        {
            PumpGazeData();

            if (_hmdEyeData == null)
                return;

            isBoothEyesValid = (_hmdEyeData.LeftEye.GazeDirection.Validity == Validity.Valid) &&
                                   (_hmdEyeData.RightEye.GazeDirection.Validity == Validity.Valid);
            //if (!isBoothEyesValid) {
            //    EyeOpenness = 0;
            //    return;
            //}

			if (_hmdEyeData.LeftEye.GazeDirection.Validity == Validity.Invalid)
				LeftEyeOpenness = 0;
			else
				LeftEyeOpenness = 1;
                
			if (_hmdEyeData.RightEye.GazeDirection.Validity == Validity.Invalid)
				RightEyeOpenness = 0;
			else
				RightEyeOpenness = 1;

            _localGazeDirection.x = (_hmdEyeData.LeftEye.GazeDirection.UnitVector.X +
                                     _hmdEyeData.RightEye.GazeDirection.UnitVector.X) / 2f;
            _localGazeDirection.y = (_hmdEyeData.LeftEye.GazeDirection.UnitVector.Y +
                                     _hmdEyeData.RightEye.GazeDirection.UnitVector.Y) / 2f;
            _localGazeDirection.z = (_hmdEyeData.LeftEye.GazeDirection.UnitVector.Z +
                                     _hmdEyeData.RightEye.GazeDirection.UnitVector.Z) / 2f;
            _localGazeDirection.Normalize();

            // Maximum 30 degree of rotation on x and y axis
            _localGazeDirection = new Vector3(Mathf.Clamp(_localGazeDirection.x, -0.5f, 0.5f), Mathf.Clamp(_localGazeDirection.y, -0.5f, 0.5f), _localGazeDirection.z);

            if (TobiiPro_Host.Instance.LocalToWorldTransform != null)
            {
                GazeDirection = TobiiPro_Host.Instance.LocalToWorldTransform.TransformDirection(_localGazeDirection);
            }

            //EyeOpenness = 1;
        }

        void OnEnable()
        {
			_eyeTracker = TobiiPro_Host.EyeTrackerInstance;

            if (_eyeTracker != null)
            {
                _eyeTracker.HMDGazeDataReceived += EnqueueEyeData;
            }
        }

        void OnDisable()
        {
            if (_eyeTracker != null)
            {
                _eyeTracker.HMDGazeDataReceived -= EnqueueEyeData;
            }
        }

        void OnDestroy()
        {
            EyeTrackingOperations.Terminate();
        }

        // This method will be called on a thread belonging to the SDK, and can not safely change values
        // that will be read from the main thread.
        private void EnqueueEyeData(object sender, HMDGazeDataEventArgs e)
        {
            lock (_queue)
            {
                _queue.Enqueue(e);
            }
        }

        private HMDGazeDataEventArgs GetNextGazeData()
        {
            lock (_queue)
            {
                return _queue.Count > 0 ? _queue.Dequeue() : null;
            }
        }

        private void PumpGazeData()
        {
            var next = GetNextGazeData();
            while (next != null)
            {
                HandleGazeData(next);
                next = GetNextGazeData();
            }
        }

        // This method will be called on the main Unity thread
        private void HandleGazeData(HMDGazeDataEventArgs e)
        {
            // Do something with gaze data

			_hmdEyeData = e;

//            Debug.Log(string.Format(
//                "Got gaze data with {0} left eye origin at point ({1}, {2}, {3}) in the HMD coordinate system.",
//                e.LeftEye.GazeOrigin.Validity,
//                e.LeftEye.GazeOrigin.PositionInHMDCoordinates.X,
//                e.LeftEye.GazeOrigin.PositionInHMDCoordinates.Y,
//                e.LeftEye.GazeOrigin.PositionInHMDCoordinates.Z));
		
        }

		public HMDGazeDataEventArgs GetHMDEyeData(){
			return _hmdEyeData;

		}

		public static SubscribingToHMDGazeData SubscribingInstance{
			get
			{
				if (_subscribingInstance != null)
				{
					return _subscribingInstance;
				}

				var newGameObject = new GameObject("TobiiPro_Data");
				DontDestroyOnLoad(newGameObject);
				_subscribingInstance = newGameObject.AddComponent<SubscribingToHMDGazeData>();

				return _subscribingInstance;
			}
		}
    }
}
