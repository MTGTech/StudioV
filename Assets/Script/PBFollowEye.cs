using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBFollowEye : MonoBehaviour
{

    public SkinnedMeshRenderer randomEye;

    private SkinnedMeshRenderer thisEye;
	
	// Update is called once per frame
	void Update ()
	{

	    thisEye = GetComponent<SkinnedMeshRenderer>();
	    var n = thisEye.sharedMesh.blendShapeCount;

	    for (int i = 0; i < n; i++)
	    {
	        thisEye.SetBlendShapeWeight(i,randomEye.GetBlendShapeWeight(i));
	    }
	}
}
