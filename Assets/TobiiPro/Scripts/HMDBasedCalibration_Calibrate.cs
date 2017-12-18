namespace Tobii.Research.CodeExamples
{
    using System;   
    using System.Collections;
    using UnityEngine;

    public class IntroAnimationArgs : EventArgs
    {
        public int CalibrationIndex { get; private set; }

        public IntroAnimationArgs(int calibrationIndex)
        {
            CalibrationIndex = calibrationIndex;
        }
    }

    public class CalibrationCompletedArgs : EventArgs
    {
        public CalibrationStatus Result { get; private set; }

        public CalibrationCompletedArgs(CalibrationStatus result)
        {
            Result = result;
        }
    }

    class HMDBasedCalibration_Calibrate:MonoBehaviour
    {
        //public GameObject TargetPrefab;
        //public KeyCode CalibrationKey = KeyCode.F6;

        public static GameObject _instance;       // Instance of target prefab
        private Vector3 _originalScale;     // Original scale of target prefab
        public static IEyeTracker _eyeTracker;
		private TobiiPro_CalibrationView _calibrationView;

        private bool _hasAnimated;

        public event EventHandler<IntroAnimationArgs> WaitingOnIntroAnimation = delegate { };
        public event EventHandler WaitingOnOutroAnimation = delegate { };
        public event EventHandler<CalibrationCompletedArgs> OnCalibrationCompleted = delegate { };

        public bool HasCompletedAnimation { set { _hasAnimated = value; } }

        private void Start()
        {		
			_eyeTracker = TobiiPro_Host.EyeTrackerInstance;
        }

        public bool LaunchCalibration(Point3D[] calibrationPoints) {
            if (TobiiPro_Host.Instance == null)
                return false;
			StartCoroutine(Calibrate(calibrationPoints, _instance));
            return true;
        }
			
        // <BeginExample>
        private IEnumerator Calibrate(Point3D[] calibrationPoints, GameObject target)
        {
			if (_eyeTracker == null)
				yield break;

            _eyeTracker.CalibrationModeEntered += EyeTracker_CalibrationModeEntered;
            _eyeTracker.CalibrationModeLeft += EyeTracker_CalibrationModeLeft;

            // Create a calibration object.
            var calibration = new HMDBasedCalibration(_eyeTracker);
            // Enter calibration mode.
            calibration.EnterCalibrationMode();

            // Get and set the lens cup separation
            float hmdIpdInMeter;
            if (TobiiPro_Util.TryGetHmdLensCupSeparationInMeter(out hmdIpdInMeter) == false)
            {
                Debug.LogWarning("TobiiPro: Failed to get lens cup separation from HMD. Setting default lens cup separation.");
                hmdIpdInMeter = 0.0635f;
            }

            TobiiPro_Util.SetLensCupSeparation(hmdIpdInMeter);
            // Collect data.
            var index = 0;
            var remainingPoints = true;
            while (remainingPoints)
            {
                // Play intro animation for the point
                _hasAnimated = false;
                WaitingOnIntroAnimation.Invoke(this, new IntroAnimationArgs(index));

                yield return StartCoroutine(WaitingForAnimation());

                var point = calibrationPoints[index];

                point = new Point3D(-point.X, point.Y, point.Z);

                // Show an image on screen where you want to calibrate.
                // target.transform.localPosition = new Vector3(-point.X, point.Y, point.Z) / 1000f;

                // Wait a little for user to focus.
                 yield return new WaitForSeconds(1.5f);


                // Collect data.
                CalibrationStatus status = calibration.CollectData(point);
                if (status != CalibrationStatus.Success)
                {
                    // Try again if it didn't go well the first time.
                    // Not all eye tracker models will fail at this point, but instead fail on ComputeAndApply.
                    Debug.Log("TobiiPro: Calibration for this point wasn't success, try again.");
                    calibration.CollectData(point);
                }

                index++;

                // Outro animation for the point
                _hasAnimated = false;
                WaitingOnOutroAnimation.Invoke(this, null);
				yield return StartCoroutine (WaitingForAnimation ());

                // Check if there are more points remaining
                if (index > calibrationPoints.Length - 1)
                {
                    remainingPoints = false;
                }
            }
            // Compute and apply the calibration.
            HMDCalibrationResult calibrationResult = calibration.ComputeAndApply();
            Debug.Log(string.Format("Compute and apply returned {0}.", calibrationResult.Status));
            // See that you're happy with the result.
            // The calibration is done. Leave calibration mode.

            OnCalibrationCompleted.Invoke(this, new CalibrationCompletedArgs(calibrationResult.Status));

            if (target!=null){
				Destroy (target);
			}

            _eyeTracker.CalibrationModeEntered -= EyeTracker_CalibrationModeEntered;            
            calibration.LeaveCalibrationMode();

            _eyeTracker.CalibrationModeLeft -= EyeTracker_CalibrationModeLeft;
        }

        private IEnumerator WaitingForAnimation()
        {
            while (!_hasAnimated)
            {
                yield return null;
            }
        }

        public bool InitiateCalibration()
        {

            if (_instance != null)
            {
                Debug.LogWarning("Cannot start calibration as cleaning up is still under going.");
                return false;
            }

			if (_calibrationView.TargetPrefab == null)
            {
                Debug.LogError("No calibration target assigned, cannot start calibration.");
                return false;
            }

			_instance = Instantiate(_calibrationView.TargetPrefab, new Vector3(int.MinValue, int.MinValue), Quaternion.identity) as GameObject;
            _originalScale = _instance.transform.localScale;
            _instance.transform.parent = TobiiPro_Host.Instance.LocalToWorldTransform;
            _instance.transform.localScale = _originalScale;

            return true;
        }

        private static void EyeTracker_CalibrationModeEntered(object sender, CalibrationModeEnteredEventArgs e)
        {
            TobiiPro_Host.Instance.IsCalibrating = true;
            Debug.Log(string.Format("Calibration mode was entered at time stamp {0}.", e.SystemTimeStamp));
        }
        private static void EyeTracker_CalibrationModeLeft(object sender, CalibrationModeLeftEventArgs e)
        {
            TobiiPro_Host.Instance.IsCalibrating = false;
            

            Debug.Log(string.Format("Calibration mode was left at time stamp {0}.", e.SystemTimeStamp));
        }

    }
}
