using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Diagnostics;
using MicroLibrary;
using UnityEngine;

public class RecordData : ThreadedJob
{

    public Vector3 GazeDirection;
    public Vector3 EyeRotationInDegree;
    public StreamWriter sw_eye;

    public StreamWriter sw_facial;
    public int BlendShapeNumber;
    public float[] BlendShapeValues;
    public string[] BlendShapeNames;

	public int FrameCount;
    public int ThreadSleepTime;
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

        MicroTimer.Enabled = true; // Start timer

    }

    void OnTimedEvent(object sender,MicroTimerEventArgs timerEventArgs)
    {

        FrameCount = timerEventArgs.TimerCount;

        if (sw_eye == null || sw_facial == null)
        {
			UnityEngine.Debug.Log(timerEventArgs.TimerCount + "," + (System.DateTime.Now - StartRecordingTime) + ",0,0,0");
        }
        else
        {
            //sw_eye.WriteLine(timerEventArgs.TimerCount + "," + (System.DateTime.Now - StartRecordingTime) + "," + GazeDirection.x + "," + GazeDirection.y + "," + GazeDirection.z);
            sw_eye.WriteLine(timerEventArgs.TimerCount + "," + (System.DateTime.Now - StartRecordingTime) + "," + EyeRotationInDegree.x + "," + EyeRotationInDegree.y + "," + EyeRotationInDegree.z);

            for (int i = 0; i < BlendShapeNumber; i++)
            {
				sw_facial.WriteLine(timerEventArgs.TimerCount + "," + (System.DateTime.Now - StartRecordingTime) + "," +
                    BlendShapeNames[i] + "," + BlendShapeValues[i]);
            }

        }

//        UnityEngine.Debug.Log(string.Format(
//            "Count = {0:#,0}  Timer = {1:#,0} µs, " +
//            "LateBy = {2:#,0} µs, ExecutionTime = {3:#,0} µs",
//            timerEventArgs.TimerCount, timerEventArgs.ElapsedMicroseconds,
//            timerEventArgs.TimerLateBy, timerEventArgs.CallbackFunctionExecutionTime));
    }

}
