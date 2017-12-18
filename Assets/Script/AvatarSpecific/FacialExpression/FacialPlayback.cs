using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: create another thread to playback

public class FacialPlayback : MonoBehaviour {

    // The facial log data file name
    public string FileName;

    private float _currentTimeStamp;
    private float _prevTimeStamp;

    [HideInInspector]
    public SkinnedMeshRenderer OptitrackFace;

    // Use this for initialization
    void Start()
    {

        _currentTimeStamp = 0;
        _prevTimeStamp = 0;

        OptitrackFace = GetComponent<FacialController>().OptitrackFace;

        try
        {
            StartCoroutine(LoadByTime());
        }
        // If anything broke in the try block, we throw an exception with information
        // on what didn't work
        catch (Exception e)
        {
            Console.WriteLine("{0}\n", e.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator LoadByTime()
    {
        string line;
        // Create a new StreamReader, tell it which file to read and what encoding the file
        // was saved as
        if (FileName == null)
            yield break;

        StreamReader theReader = new StreamReader(FileName, Encoding.Default);


        // Using also close the reader
        using (theReader)
        {
            // While there's lines left in the text file, do this:
            do
            {

                line = theReader.ReadLine();
                if (line != null)
                {
                    // Time, Blendshape id, Blendshape value, Blendshape name

                    string[] entries = line.Split(',');
                    if (entries.Length > 0)
                    {
                        _currentTimeStamp = float.Parse(entries[0]);
						Debug.Log ("korv" + _currentTimeStamp);
                        yield return new WaitForSeconds(_currentTimeStamp - _prevTimeStamp);

                        // Assign blendshapes to faces
                        OptitrackFace.SetBlendShapeWeight(int.Parse(entries[1]), float.Parse(entries[2]));
						_prevTimeStamp = _currentTimeStamp;
						Debug.Log("Blendshape" + int.Parse(entries[1]) + "value" + float.Parse(entries[2]));
                    }

                }
            }
            while (line != null);
        }
        // Handle any problems that might arise when reading the text

    }
}
