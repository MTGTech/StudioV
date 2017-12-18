using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class WorldTimer : MonoBehaviour
{
    private Stopwatch _stopwatch;

    public TimeSpan ElapsedTimeSinceStart;  // TimeSpan from start of each data recording
    [HideInInspector]
    public int FrameCount = 0;              // World frame count
    [HideInInspector]
    public int ThreadSleepTime;             // Set by LaucherInput

    // Use this for initialization
    void Start () {
	    _stopwatch = new Stopwatch();
    }
	
	// Update is called once per frame
	void Update ()
	{
	    ElapsedTimeSinceStart = _stopwatch.Elapsed;	    
    }

    public void StartTimer()
    {
        _stopwatch.Start();
    }

    public void StopTimer()
    {
        _stopwatch.Stop();
    }

	public void ResetTimer(){
		_stopwatch.Reset ();
	}

    public void SetThreadSleepTime(int FPS)
    {
        ThreadSleepTime = Mathf.FloorToInt(1000 / FPS);
    }

}
