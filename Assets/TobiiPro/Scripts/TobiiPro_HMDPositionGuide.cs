using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tobii.Research;
using Tobii.Research.Unity.CodeExamples;

public class TobiiPro_HMDPositionGuide : MonoBehaviour {

	public TobiiPro_HMDPlacementCanvas HMDPlacementCanvas;

	private IEyeTracker _eyeTracker;

	private Vector2 _leftPupilXY;
	private Vector2 _rightPupilXY;

	private Vector2 _sizeOfparent;
	private float _lcs;

	void Start()
	{
		_eyeTracker = TobiiPro_Host.EyeTrackerInstance;

		_sizeOfparent = HMDPlacementCanvas.LeftPupil.parent.GetComponent<RectTransform>().sizeDelta;
	}
		


	public void ToggleVisualization()
	{
		HMDPlacementCanvas.gameObject.SetActive(!HMDPlacementCanvas.gameObject.activeSelf);
	}

	/// <summary>
	/// Warning, a lot of guesstimating happening right here!!!
	/// </summary>
	void Update()
	{

		if (HMDPlacementCanvas.gameObject.activeSelf == false)
		{
			return;
		}

		float lensCupSeparationInM;
		if (TobiiPro_Util.TryGetHmdLensCupSeparationInMeter(out lensCupSeparationInM) == false)
		{
			Debug.LogError("TobiiPro: Failed to get hmd lens cup separation.");
		}
		else
		{
			_lcs = lensCupSeparationInM;

			if (TobiiPro_Host.Instance.IsCalibrating == false && Time.frameCount % 45 == 0)
			{
				TobiiPro_Util.SetLensCupSeparation(_lcs);
			}
		}

		if (HMDPlacementCanvas != null && HMDPlacementCanvas.gameObject.activeSelf)
		{
			var hmdLcsInMM = _lcs * 1000f;

			var lHPos = new Vector3(-hmdLcsInMM, 0);
			var rHPos = new Vector3(hmdLcsInMM, 0);

			HMDPlacementCanvas.TargetLeft.localPosition = lHPos;
			HMDPlacementCanvas.TargetRight.localPosition = rHPos;

			var pupilL = SubscribingToHMDGazeData.SubscribingInstance.GetHMDEyeData ().LeftEye.PupilPosition.PositionInTrackingArea;
			var pupilR = SubscribingToHMDGazeData.SubscribingInstance.GetHMDEyeData ().RightEye.PupilPosition.PositionInTrackingArea;

			_leftPupilXY = new Vector2 (pupilL.X, pupilL.Y);
			_rightPupilXY = new Vector2 (pupilR.X, pupilR.Y);

			var pupilLeft = new Vector2(
				_leftPupilXY.x * _sizeOfparent.x,
				_leftPupilXY.y * _sizeOfparent.y * -1
			);

			var pupilRight = new Vector2(
				_rightPupilXY.x * _sizeOfparent.x,
				_rightPupilXY.y * _sizeOfparent.y * -1
			);

			if (Mathf.Abs(pupilLeft.y - pupilRight.y) < (0.15f * _sizeOfparent.y))
			{
				pupilLeft.y = (pupilLeft.y + pupilRight.y) * 0.5f;
				pupilRight.y = pupilLeft.y;
			}

			HMDPlacementCanvas.LeftPupil.anchoredPosition = pupilLeft;
			HMDPlacementCanvas.RightPupil.anchoredPosition = pupilRight;

			// Coloring
			var c = new Vector2(0.5f, 0.5f);
			var distLeft = Vector2.Distance(c, _leftPupilXY);
			var distRight = Vector2.Distance(c, _rightPupilXY);

			HMDPlacementCanvas.LeftPupil.GetComponent<Image>().color = Color.Lerp(Color.green, Color.red, distLeft / 0.35f);
			HMDPlacementCanvas.RightPupil.GetComponent<Image>().color = Color.Lerp(Color.green, Color.red, distRight / 0.35f);

			// Info to the user
			HMDPlacementCanvas.Status.text = distLeft + distRight < 0.25f ? "Awesome!" : "OK";
		}
	}
}
