using System.IO;
using System.Text;
using UnityEngine;

public class EyePlayback : MonoBehaviour
{

    public bool IsNetworked = false;
    public string FileName;
    public float FPS = 60;
	public bool isLoop = true;
    public bool isPlaying = true;

	public Transform EyeL;
    public Transform EyeR;
    public float LerpRate = 20f;

	private StreamReader theReader;
    private Vector3 _localGaze;
    private Vector3 _newGaze;
    private PhotonView _photonView;

    private Animator _anim;  

	void Start()
	{
	    //var steamObj = FindObjectOfType<SteamVR_Render>();
	    //steamObj.lockPhysicsUpdateRateToRenderFrequency = false;

		var vrcam = GameObject.Find ("[CameraRig]");
		if (vrcam != null) {
			vrcam.SetActive(false);
		}
	    _anim = GetComponent<Animator>();
	    Time.fixedDeltaTime = 1f / FPS;

		if (FileName == null)
			return;

		theReader = new StreamReader(FileName, Encoding.Default);
        _anim.SetBool("StartPlayback", true);
	    _photonView = GetComponent<PhotonView>();

	}

	void FixedUpdate () {
		if (isPlaying) {
			if (_photonView.isMine || IsNetworked == false) {
				string line;
				// Using also close the reader

				line = theReader.ReadLine ();
				if (line != null) {
					// Time, Blendshape id, Blendshape value, Blendshape name

					string[] entries = line.Split (',');
					if (entries.Length > 0) {
						// Assign blendshapes to faces
						_localGaze = new Vector3 (float.Parse (entries [2]), float.Parse (entries [3]), float.Parse (entries [4]));
                    
						//Debug.Log(Time.time + "gaze direction:" + _localGaze.x);

						EyeL.forward = Vector3.Lerp (EyeL.forward, _localGaze, Time.deltaTime * LerpRate);
						EyeR.forward = EyeL.forward;
					}
				} else if (isLoop) {
					Restart ();
				}
			} else {
				EyeL.forward = Vector3.Lerp (EyeL.forward, _newGaze, Time.deltaTime * LerpRate);
				EyeR.forward = EyeL.forward;
			}
		}
	    
	}
	public void Restart (){
		_anim.SetBool ("StartPlayback", false);
		theReader.Close ();
		theReader = new StreamReader (FileName, Encoding.Default);
		_anim.SetBool ("StartPlayback", true);
	}
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.isWriting)
        {
            stream.SendNext(_localGaze);
        }
        else
        {
            _newGaze = (Vector3)stream.ReceiveNext();
        }
    }

    void OnApplicationQuit(){
		if (theReader != null) {
			theReader.Close ();
		}

	}
}
