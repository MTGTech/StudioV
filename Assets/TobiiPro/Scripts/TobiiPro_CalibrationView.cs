using System;
using System.Collections;
using UnityEngine;
using Tobii.Research;
using Tobii.Research.CodeExamples;
using Tobii.Research.Unity.CodeExamples;
using Math = UnityEngine.Mathf;

// TODO...
public class TobiiPro_CalibrationView : MonoBehaviour {
    // The calibration dots should be in front of 
    // the user and currently requires minimum 5
    // Define the points in the HMD space we should calibrate at.
    private readonly Point3D[] _calibrationPoints = {
                new Point3D(500f, 500f, 2000f),
                new Point3D(-500f, 500f, 2000f),
                new Point3D(-500f, -500f, 2000f),
                new Point3D(500f, -500f, 2000f),
                new Point3D(0f, 0f, 2000f), };

    public GameObject TargetPrefab;
    public KeyCode CalibrationKey = KeyCode.F6;

    [HideInInspector]
    public GameObject _instance;
    private Vector3 _originalScale;     // Original scale of target prefab
    private HMDBasedCalibration_Calibrate _calibration;
    private SubscribingToHMDGazeData _subscribingGazeData;
    

    // Use this for initialization
    void Start () {
        _calibration = GetComponent<HMDBasedCalibration_Calibrate>();
        _subscribingGazeData = SubscribingToHMDGazeData.SubscribingInstance;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(CalibrationKey) && TobiiPro_Host.EyeTrackerInstance!=null && _subscribingGazeData.isBoothEyesValid)
        {
            InitiateCalibration();
        }
    }

    public bool InitiateCalibration()
    {
        if (TobiiPro_Host.Instance.IsCalibrating)
        {
            Debug.LogWarning("TobiiPro, calibration: Cannot start multiple calibrations at the same time.");
            return false;
        }

        if (_instance != null)
        {
            Debug.LogWarning("Cannot start calibration as cleaning up is still under going.");
            return false;
        }

        if (TargetPrefab == null)
        {
            Debug.LogError("No calibration target assigned, cannot start calibration.");
            return false;
        }

        _instance = Instantiate(TargetPrefab, new Vector3(int.MinValue, int.MinValue), Quaternion.identity) as GameObject;
        _originalScale = _instance.transform.localScale;
        _instance.transform.parent = TobiiPro_Host.Instance.LocalToWorldTransform;
        _instance.transform.localScale = _originalScale;


        Debug.Log("TobiiPro: Launching calibration.");
        _calibration.WaitingOnIntroAnimation += StartIntroAnimation;
        _calibration.WaitingOnOutroAnimation += StartOutroAnimation;
        _calibration.OnCalibrationCompleted += OnCalibrationCompleted;
		_calibration.LaunchCalibration (_calibrationPoints);
	
        return true;
    }

    #region TobiiPro calibration animation

    private void StartIntroAnimation(object sender, IntroAnimationArgs e)
    {
        // Treat the first point specially as it should draw the user's attention.
        StartCoroutine(e.CalibrationIndex == 0
            ? AppearAnimation(convertToVector3(_calibrationPoints[e.CalibrationIndex]))
            : TransistionAnimation(convertToVector3(_calibrationPoints[e.CalibrationIndex])));
    }

    private void StartOutroAnimation(object sender, EventArgs e)
    {
        StartCoroutine(CalibratedPointAnimation());
    }

    private void OnCalibrationCompleted(object sender, CalibrationCompletedArgs e)
    {
        StartCoroutine(DisplayCalibrationResult(e));
    }

    private IEnumerator AppearAnimation(Vector3 finalPosition)
    {
        var original = _instance.transform.localScale;
        _instance.transform.localPosition = finalPosition;

        var progress = 0f; // 0 - 1 
        while (progress < 1f)
        {
            progress += Time.deltaTime * (1f / 1f);
            var scaleFactor = Easings.BackEaseOut(progress);
            _instance.transform.localScale = original * scaleFactor;
            yield return null;
        }

        _calibration.HasCompletedAnimation = true;
    }

    private IEnumerator TransistionAnimation(Vector3 finalPosition)
    {
        var startPosition = _instance.transform.localPosition;
        var v = finalPosition - startPosition;

        yield return new WaitForSeconds(0.3f);

        var progress = 0f; // 0 - 1
        while (progress < 1f)
        {
            progress += Time.deltaTime * (1f / 0.35f);
            var derp = Easings.QuadraticEaseOut(progress);
            _instance.transform.localPosition = startPosition + v * derp;

            derp = Easings.QuadraticEaseOut(progress);
            _instance.transform.localScale = Vector3.Lerp(Vector3.zero, _originalScale, derp);

            yield return null;
        }

        _instance.transform.localScale = _originalScale;
        _instance.transform.localPosition = finalPosition;

        yield return new WaitForSeconds(0.25f);

		_calibration.HasCompletedAnimation = true;
    }

    private IEnumerator CalibratedPointAnimation()
    {
        yield return new WaitForSeconds(0.2f);

        var startScale = _instance.transform.localScale;

        var progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * (1f / 0.2f);
            var derp = 1 - Easings.QuadraticEaseOut(progress);
            _instance.transform.localScale = startScale * derp;
            yield return null;
        }

        _instance.transform.localScale = Vector3.zero;
        _calibration.HasCompletedAnimation = true;
    }

    private IEnumerator DisplayCalibrationResult(CalibrationCompletedArgs e)
    {
        switch (e.Result)
        {
            case CalibrationStatus.Success:
                _instance.GetComponent<Renderer>().material.color = Color.green;
                Debug.Log("TobiiPro: Successfully calibrated the user.");
                break;
            case CalibrationStatus.Failure:
                _instance.GetComponent<Renderer>().material.color = Color.red;
                Debug.LogWarning("TobiiPro: Failed to calibrate the user.");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var progress = 0f; // 0 - 1
        while (progress < 1f)
        {
            progress += Time.deltaTime * (1f / 0.1f);
            var derp = Easings.QuadraticEaseOut(progress);
            _instance.transform.localScale = Vector3.Lerp(Vector3.zero, _originalScale, derp);

            yield return null;
        }

        yield return new WaitForSeconds(0.35f);
        _calibration.WaitingOnIntroAnimation -= StartIntroAnimation;
        _calibration.WaitingOnOutroAnimation -= StartOutroAnimation;
        _calibration.OnCalibrationCompleted -= OnCalibrationCompleted;

        Destroy(_instance);
        _instance = null;
    }

    private Vector3 convertToVector3(Point3D point) {
        return new Vector3(point.X, point.Y, point.Z)/1000f;
    }
}

/// <summary>
/// Credits https://github.com/acron0/Easings
/// </summary>
static public class Easings
{
    /// <summary>
    /// Constant Pi.
    /// </summary>
    private const float PI = Math.PI;

    /// <summary>
    /// Constant Pi / 2.
    /// </summary>
    private const float HALFPI = Math.PI / 2.0f;

    /// <summary>
    /// Modeled after the line y = x
    /// </summary>
    static public float Linear(float p)
    {
        return p;
    }

    /// <summary>
    /// Modeled after the parabola y = x^2
    /// </summary>
    static public float QuadraticEaseIn(float p)
    {
        return p * p;
    }

    /// <summary>
    /// Modeled after the parabola y = -x^2 + 2x
    /// </summary>
    static public float QuadraticEaseOut(float p)
    {
        return -(p * (p - 2));
    }

    /// <summary>
    /// Modeled after the piecewise quadratic
    /// y = (1/2)((2x)^2)             ; [0, 0.5)
    /// y = -(1/2)((2x-1)*(2x-3) - 1) ; [0.5, 1]
    /// </summary>
    static public float QuadraticEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 2 * p * p;
        }
        else
        {
            return (-2 * p * p) + (4 * p) - 1;
        }
    }

    /// <summary>
    /// Modeled after the cubic y = x^3
    /// </summary>
    static public float CubicEaseIn(float p)
    {
        return p * p * p;
    }

    /// <summary>
    /// Modeled after the cubic y = (x - 1)^3 + 1
    /// </summary>
    static public float CubicEaseOut(float p)
    {
        float f = (p - 1);
        return f * f * f + 1;
    }

    /// <summary>	
    /// Modeled after the piecewise cubic
    /// y = (1/2)((2x)^3)       ; [0, 0.5)
    /// y = (1/2)((2x-2)^3 + 2) ; [0.5, 1]
    /// </summary>
    static public float CubicEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 4 * p * p * p;
        }
        else
        {
            float f = ((2 * p) - 2);
            return 0.5f * f * f * f + 1;
        }
    }

    /// <summary>
    /// Modeled after the quartic x^4
    /// </summary>
    static public float QuarticEaseIn(float p)
    {
        return p * p * p * p;
    }

    /// <summary>
    /// Modeled after the quartic y = 1 - (x - 1)^4
    /// </summary>
    static public float QuarticEaseOut(float p)
    {
        float f = (p - 1);
        return f * f * f * (1 - p) + 1;
    }

    /// <summary>
    // Modeled after the piecewise quartic
    // y = (1/2)((2x)^4)        ; [0, 0.5)
    // y = -(1/2)((2x-2)^4 - 2) ; [0.5, 1]
    /// </summary>
    static public float QuarticEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 8 * p * p * p * p;
        }
        else
        {
            float f = (p - 1);
            return -8 * f * f * f * f + 1;
        }
    }

    /// <summary>
    /// Modeled after the quintic y = x^5
    /// </summary>
    static public float QuinticEaseIn(float p)
    {
        return p * p * p * p * p;
    }

    /// <summary>
    /// Modeled after the quintic y = (x - 1)^5 + 1
    /// </summary>
    static public float QuinticEaseOut(float p)
    {
        float f = (p - 1);
        return f * f * f * f * f + 1;
    }

    /// <summary>
    /// Modeled after the piecewise quintic
    /// y = (1/2)((2x)^5)       ; [0, 0.5)
    /// y = (1/2)((2x-2)^5 + 2) ; [0.5, 1]
    /// </summary>
    static public float QuinticEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 16 * p * p * p * p * p;
        }
        else
        {
            float f = ((2 * p) - 2);
            return 0.5f * f * f * f * f * f + 1;
        }
    }

    /// <summary>
    /// Modeled after quarter-cycle of sine wave
    /// </summary>
    static public float SineEaseIn(float p)
    {
        return Math.Sin((p - 1) * HALFPI) + 1;
    }

    /// <summary>
    /// Modeled after quarter-cycle of sine wave (different phase)
    /// </summary>
    static public float SineEaseOut(float p)
    {
        return Math.Sin(p * HALFPI);
    }

    /// <summary>
    /// Modeled after half sine wave
    /// </summary>
    static public float SineEaseInOut(float p)
    {
        return 0.5f * (1 - Math.Cos(p * PI));
    }

    /// <summary>
    /// Modeled after shifted quadrant IV of unit circle
    /// </summary>
    static public float CircularEaseIn(float p)
    {
        return 1 - Math.Sqrt(1 - (p * p));
    }

    /// <summary>
    /// Modeled after shifted quadrant II of unit circle
    /// </summary>
    static public float CircularEaseOut(float p)
    {
        return Math.Sqrt((2 - p) * p);
    }

    /// <summary>	
    /// Modeled after the piecewise circular function
    /// y = (1/2)(1 - Math.Sqrt(1 - 4x^2))           ; [0, 0.5)
    /// y = (1/2)(Math.Sqrt(-(2x - 3)*(2x - 1)) + 1) ; [0.5, 1]
    /// </summary>
    static public float CircularEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 0.5f * (1 - Math.Sqrt(1 - 4 * (p * p)));
        }
        else
        {
            return 0.5f * (Math.Sqrt(-((2 * p) - 3) * ((2 * p) - 1)) + 1);
        }
    }

    /// <summary>
    /// Modeled after the exponential function y = 2^(10(x - 1))
    /// </summary>
    static public float ExponentialEaseIn(float p)
    {
        return (p == 0.0f) ? p : Math.Pow(2, 10 * (p - 1));
    }

    /// <summary>
    /// Modeled after the exponential function y = -2^(-10x) + 1
    /// </summary>
    static public float ExponentialEaseOut(float p)
    {
        return (p == 1.0f) ? p : 1 - Math.Pow(2, -10 * p);
    }

    /// <summary>
    /// Modeled after the piecewise exponential
    /// y = (1/2)2^(10(2x - 1))         ; [0,0.5)
    /// y = -(1/2)*2^(-10(2x - 1))) + 1 ; [0.5,1]
    /// </summary>
    static public float ExponentialEaseInOut(float p)
    {
        if (p == 0.0 || p == 1.0) return p;

        if (p < 0.5f)
        {
            return 0.5f * Math.Pow(2, (20 * p) - 10);
        }
        else
        {
            return -0.5f * Math.Pow(2, (-20 * p) + 10) + 1;
        }
    }

    /// <summary>
    /// Modeled after the damped sine wave y = sin(13pi/2*x)*Math.Pow(2, 10 * (x - 1))
    /// </summary>
    static public float ElasticEaseIn(float p)
    {
        return Math.Sin(13 * HALFPI * p) * Math.Pow(2, 10 * (p - 1));
    }

    /// <summary>
    /// Modeled after the damped sine wave y = sin(-13pi/2*(x + 1))*Math.Pow(2, -10x) + 1
    /// </summary>
    static public float ElasticEaseOut(float p)
    {
        return Math.Sin(-13 * HALFPI * (p + 1)) * Math.Pow(2, -10 * p) + 1;
    }

    /// <summary>
    /// Modeled after the piecewise exponentially-damped sine wave:
    /// y = (1/2)*sin(13pi/2*(2*x))*Math.Pow(2, 10 * ((2*x) - 1))      ; [0,0.5)
    /// y = (1/2)*(sin(-13pi/2*((2x-1)+1))*Math.Pow(2,-10(2*x-1)) + 2) ; [0.5, 1]
    /// </summary>
    static public float ElasticEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 0.5f * Math.Sin(13 * HALFPI * (2 * p)) * Math.Pow(2, 10 * ((2 * p) - 1));
        }
        else
        {
            return 0.5f * (Math.Sin(-13 * HALFPI * ((2 * p - 1) + 1)) * Math.Pow(2, -10 * (2 * p - 1)) + 2);
        }
    }

    /// <summary>
    /// Modeled after the overshooting cubic y = x^3-x*sin(x*pi)
    /// </summary>
    static public float BackEaseIn(float p)
    {
        return p * p * p - p * Math.Sin(p * PI);
    }

    /// <summary>
    /// Modeled after overshooting cubic y = 1-((1-x)^3-(1-x)*sin((1-x)*pi))
    /// </summary>	
    static public float BackEaseOut(float p)
    {
        float f = (1 - p);
        return 1 - (f * f * f - f * Math.Sin(f * PI));
    }

    /// <summary>
    /// Modeled after the piecewise overshooting cubic function:
    /// y = (1/2)*((2x)^3-(2x)*sin(2*x*pi))           ; [0, 0.5)
    /// y = (1/2)*(1-((1-x)^3-(1-x)*sin((1-x)*pi))+1) ; [0.5, 1]
    /// </summary>
    static public float BackEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            float f = 2 * p;
            return 0.5f * (f * f * f - f * Math.Sin(f * PI));
        }
        else
        {
            float f = (1 - (2 * p - 1));
            return 0.5f * (1 - (f * f * f - f * Mathf.Sin(f * PI))) + 0.5f;
        }
    }

    /// <summary>
    /// </summary>
    static public float BounceEaseIn(float p)
    {
        return 1 - BounceEaseOut(1 - p);
    }

    /// <summary>
    /// </summary>
    static public float BounceEaseOut(float p)
    {
        if (p < 4 / 11.0f)
        {
            return (121 * p * p) / 16.0f;
        }
        else if (p < 8 / 11.0f)
        {
            return (363 / 40.0f * p * p) - (99 / 10.0f * p) + 17 / 5.0f;
        }
        else if (p < 9 / 10.0f)
        {
            return (4356 / 361.0f * p * p) - (35442 / 1805.0f * p) + 16061 / 1805.0f;
        }
        else
        {
            return (54 / 5.0f * p * p) - (513 / 25.0f * p) + 268 / 25.0f;
        }
    }

    /// <summary>
    /// </summary>
    static public float BounceEaseInOut(float p)
    {
        if (p < 0.5f)
        {
            return 0.5f * BounceEaseIn(p * 2);
        }
        else
        {
            return 0.5f * BounceEaseOut(p * 2 - 1) + 0.5f;
        }
    }

    #endregion
}
