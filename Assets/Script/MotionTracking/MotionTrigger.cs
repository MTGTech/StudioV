using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionTrigger : MonoBehaviour
{

    private Animator _avatarAnimator;
    private Vector3[] _recordedBoneRotations;
    private int _numOfBones;

    public float TolerantAngle = 10f;

	// Use this for initialization
	void Start ()
	{
	    _avatarAnimator = GetComponent<Animator>();
	    _numOfBones = System.Enum.GetValues(typeof(HumanBodyBones)).Length;
        _recordedBoneRotations = new Vector3[_numOfBones];
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RecordPose();
        }
    }

    // Update is called once per frame
	void FixedUpdate ()
	{
	    // if(isTPose())

	}

    void RecordPose()
    {
        for (var i = 0; i < _numOfBones; i++)
        {
            // Except lefteye, righteye and jaw
            if (i != 21 && i != 22 && i != 23)
            {
                var bone = (HumanBodyBones)i;
                _recordedBoneRotations[i] = _avatarAnimator.GetBoneTransform(bone).localEulerAngles;
            }
            
        }       
    }

    private bool isTPose()
    {
        if (_recordedBoneRotations == null)
            return false;

        for (var i = 0; i < _numOfBones; i++)
        {
            // Except lefteye, righteye and jaw
            if (i != 21 && i != 22 && i != 23)
            {
                var bone = (HumanBodyBones)i;
                Vector3 diff = _avatarAnimator.GetBoneTransform(bone).localEulerAngles - _recordedBoneRotations[i] ;

                if (!(diff.x < TolerantAngle && diff.y < TolerantAngle && diff.z < TolerantAngle))
                    return false;
            }
        }

        return true;
    }
}
