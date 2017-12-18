//======================================================================================================
// Copyright 2016, NaturalPoint Inc.
//======================================================================================================

using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using NaturalPoint;
using NaturalPoint.NatNetLib;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;


/// <summary>Skeleton naming conventions supported by OptiTrack Motive.</summary>
public enum OptitrackBoneNameConvention
{
    Motive,
    FBX,
    BVH,
}

[System.Serializable]
public struct Vector3Serializer{
	

	public float x;
	public float y;
	public float z;

	public Vector3Serializer(float x, float y, float z){
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public void Fill(Vector3 v3){
		x = v3.x;
		y = v3.y;
		z = v3.z;
	}
	public Vector3 V3{get{ return new Vector3 (x, y, z);} set{ Fill (value);}}
}
[System.Serializable]
public struct QuaternionSerializer{
	public float x;
	public float y;
	public float z;
	public float w;

	public QuaternionSerializer(float x, float y, float z, float w){
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	public void Fill(Quaternion q){
		x = q.x;
		y = q.y;
		z = q.z;
		w = q.w;
	}
	public Quaternion Q{get{ return new Quaternion (x, y, z, w);} set{ Fill (value);}}
}

/// <summary>Describes the position and orientation of a streamed tracked object.</summary>
[System.Serializable]
public class OptitrackPose
{
	private Vector3Serializer interpolatedPosition;
	public Vector3Serializer previousUpdatedPositon;
	private Vector3Serializer lastUpdatedPositon;
	private float lerpTime;

//	public OptitrackPose(Vector3Serializer previousUpdatedPositon, QuaternionSerializer previousUpdatedOrientation){
//		this.previousUpdatedPositon = previousUpdatedPositon;
//		this.previousUpdatedOrientation = previousUpdatedOrientation;
//	}
	public void ResetTime(){
		lerpTime = Time.time;
	}

	public Vector3Serializer Position{
		get{
			interpolatedPosition.V3 = Vector3.Lerp(interpolatedPosition.V3, lastUpdatedPositon.V3, Time.deltaTime*10);
//			Debug.Log (((Time.time - lerpTime)*10));
//			Debug.Log ("lastUpdatedPositon: " + lastUpdatedPositon.V3.ToString("F4") + "previousUpdatedPositon: " + previousUpdatedPositon.V3.ToString("F4") + "interpolatedPosition: " + interpolatedPosition.V3.ToString("F4"));
			return interpolatedPosition;
		}set{
			previousUpdatedPositon = lastUpdatedPositon;
//			Debug.Log ("previousUpdatedPositon: " + previousUpdatedPositon.V3 + "lastUpdatedPosition: " + lastUpdatedPositon.V3 + "value: " + value.V3);
			lastUpdatedPositon = value;
//			Debug.Log ("---previousUpdatedPositon: " + previousUpdatedPositon.V3 + "lastUpdatedPosition: " + lastUpdatedPositon.V3 + "value: " + value.V3);
		}
	}

	private QuaternionSerializer interpolatedOrientation;
	public QuaternionSerializer previousUpdatedOrientation;
	private QuaternionSerializer lastUpdatedOrientation;
	public QuaternionSerializer Orientation{
		get{ 
			interpolatedOrientation.Q = Quaternion.Lerp (interpolatedOrientation.Q, lastUpdatedOrientation.Q, Time.deltaTime*10);
			return interpolatedOrientation;
		}set{ 
			previousUpdatedOrientation = lastUpdatedOrientation;
			lastUpdatedOrientation = value;
		}
	}
}

[System.Serializable]
public struct OptitrackMarkerState
{
	public Vector3Serializer Position;
    public float Size;
}


/// <summary>Represents the state of a streamed rigid body at an instant in time.</summary>
[System.Serializable]
public class OptitrackRigidBodyState
{
    public OptitrackHiResTimer.Timestamp DeliveryTimestamp;
    public OptitrackPose Pose;
    public List<OptitrackMarkerState> Markers;
}


/// <summary>Represents the state of a streamed skeleton at an instant in time.</summary>
[System.Serializable]
public class OptitrackSkeletonState
{
    /// <summary>Maps from OptiTrack bone IDs to their corresponding bone poses.</summary>
    public Dictionary<Int32, OptitrackPose> BonePoses;
}


/// <summary>Describes the hierarchy and neutral pose of a streamed skeleton.</summary>
[System.Serializable]
public class OptitrackSkeletonDefinition
{
	[System.Serializable]
    public class BoneDefinition
    {
        /// <summary>The ID of this bone within this skeleton.</summary>
        public Int32 Id;

        /// <summary>The ID of this bone's parent bone. A value of 0 means that this is the root bone.</summary>
        public Int32 ParentId;

        /// <summary>The name of this bone.</summary>
        public string Name;

        /// <summary>
        /// This bone's position offset from its parent in the skeleton's neutral pose.
        /// (The neutral orientation is always <see cref="Quaternion.identity"/>.)
        /// </summary>
        public Vector3Serializer Offset;
    }

    /// <summary>Skeleton ID. Used as an argument to <see cref="OptitrackStreamingClient.GetLatestSkeletonState"/>.</summary>
    public Int32 Id;

    /// <summary>Skeleton asset name.</summary>
    public string Name;

    /// <summary>Bone names, hierarchy, and neutral pose position information.</summary>
    public List<BoneDefinition> Bones;
}

[System.Serializable]
public static class OptitrackHiResTimer
{
	[System.Serializable]
    public struct Timestamp
    {
        internal Int64 m_ticks;

        public float AgeSeconds
        {
            get
            {
                return Now().SecondsSince( this );
            }
        }

        public float SecondsSince( Timestamp reference )
        {
            Int64 deltaTicks = m_ticks - reference.m_ticks;
            return deltaTicks / (float)System.Diagnostics.Stopwatch.Frequency;
        }
    }

    public static Timestamp Now()
    {
        return new Timestamp {
            m_ticks = System.Diagnostics.Stopwatch.GetTimestamp()
        };
    }
}


/// <summary>
/// Connects to a NatNet streaming server and makes the data available in lightweight Unity-friendly representations.
/// </summary>
public class OptitrackStreamingClient : Photon.MonoBehaviour
{
    public enum ClientConnectionType
    {
        Multicast,
        Unicast,
		Photon
    }


    public ClientConnectionType ConnectionType;
    public string LocalAddress = "127.0.0.1";
    public string ServerAddress = "127.0.0.1";
    public UInt16 ServerCommandPort = NatNetConstants.DefaultCommandPort;
    public UInt16 ServerDataPort = NatNetConstants.DefaultDataPort;
    public OptitrackBoneNameConvention BoneNamingConvention = OptitrackBoneNameConvention.Motive;


    #region Private fields
    private bool m_receivedFrameSinceConnect = false;
    private OptitrackHiResTimer.Timestamp m_lastFrameDeliveryTimestamp;
    private Coroutine m_connectionHealthCoroutine = null;

    private NatNetClient m_client;
    private NatNetClient.DataDescriptions m_dataDescs;

	public List<OptitrackSkeletonDefinition> m_skeletonDefinitions = new List<OptitrackSkeletonDefinition>();
    public List<OptitrackSkeletonDefinition> m_photonisedSkeletonDefinitions = new List<OptitrackSkeletonDefinition>();

    /// <summary>Maps from a streamed rigid body's ID to its most recent available pose data.</summary>
	private Dictionary<Int32, OptitrackRigidBodyState> m_latestRigidBodyStates = new Dictionary<Int32, OptitrackRigidBodyState>();
	private Dictionary<Int32, OptitrackRigidBodyState> m_photonisedLatestRigidBodyStates = new Dictionary<Int32, OptitrackRigidBodyState>();
//    private Dictionary<Int32, OptitrackRigidBodyState> m_previousPhotonisedLatestRigidBodyStates = new Dictionary<Int32, OptitrackRigidBodyState>();

    /// <summary>Maps from a streamed skeleton's ID to its most recent available pose data.</summary>
	private Dictionary<Int32, OptitrackSkeletonState> m_latestSkeletonStates = new Dictionary<Int32, OptitrackSkeletonState>();
	public Dictionary<Int32, OptitrackSkeletonState> m_photonisedLatestSkeletonStates = new Dictionary<Int32, OptitrackSkeletonState>();
//    public Dictionary<Int32, OptitrackSkeletonState> m_previousPhotonisedLatestSkeletonStates = new Dictionary<Int32, OptitrackSkeletonState>();

    /// <summary>
    /// Lock held during access to fields which are potentially modified by <see cref="OnNatNetFrameReceived"/> (which
    /// executes on a separate thread). Note while the lock is held, any frame updates received are simply dropped.
    /// </summary>
    private object m_frameDataUpdateLock = new object();

	private bool getStreamThroughPhoton = false;

    #endregion Private fields


	public void SetGetStreamThroughPhoton(Toggle toggle){
		getStreamThroughPhoton = toggle.isOn;
	}

	public void OnJoinedRoom(){
		if (!PhotonNetwork.isMasterClient && getStreamThroughPhoton) {
			ConnectionType = ClientConnectionType.Photon;
		} else {
			UpdateDefinitions ();
		}
	}

    /// <summary>
    /// Returns the first <see cref="OptitrackStreamingClient"/> component located in the scene.
    /// Provides a convenient, sensible default in the common case where only a single client exists.
    /// Issues a warning if more than one such component is found.
    /// </summary>
    /// <returns>An arbitrary OptitrackClient from the scene, or null if none are found.</returns>
    public static OptitrackStreamingClient FindDefaultClient()
    {
        OptitrackStreamingClient[] allClients = FindObjectsOfType<OptitrackStreamingClient>();

        if ( allClients.Length == 0 )
        {
            Debug.LogError( "Unable to locate any " + typeof( OptitrackStreamingClient ).FullName + " components." );
            return null;
        }
        else if ( allClients.Length > 1 )
        {
            Debug.LogWarning( "Multiple " + typeof( OptitrackStreamingClient ).FullName + " components found in scene; defaulting to first available." );
        }

        return allClients[0];
    }


    /// <summary>Get the most recently received state for the specified rigid body.</summary>
    /// <param name="rigidBodyId">Corresponds to the "User ID" field in Motive.</param>
    /// <returns>The most recent available state, or null if none available.</returns>
    public OptitrackRigidBodyState GetLatestRigidBodyState( Int32 rigidBodyId )
    {
		OptitrackRigidBodyState rbState = null;


        lock ( m_frameDataUpdateLock )
        {
			if (ConnectionType == ClientConnectionType.Photon) {

				m_photonisedLatestRigidBodyStates.TryGetValue (rigidBodyId, out rbState);
			} else {
				m_latestRigidBodyStates.TryGetValue (rigidBodyId, out rbState);
			}
        }

        return rbState;
    }


    /// <summary>Get the most recently received state for the specified skeleton.</summary>
    /// <param name="skeletonId">
    /// Taken from the corresponding <see cref="OptitrackSkeletonDefinition.Id"/> field.
    /// To find the appropriate skeleton definition, use <see cref="GetSkeletonDefinitionByName"/>.
    /// </param>
    /// <returns>The most recent available state, or null if none available.</returns>
    public OptitrackSkeletonState GetLatestSkeletonState( Int32 skeletonId )
    {
        OptitrackSkeletonState skelState;

        lock ( m_frameDataUpdateLock )
        {
			if (ConnectionType == ClientConnectionType.Photon) {
				m_photonisedLatestSkeletonStates.TryGetValue( skeletonId, out skelState );

			} else {
				m_latestSkeletonStates.TryGetValue( skeletonId, out skelState );
			}
        }

        return skelState;
    }


    /// <summary>Retrieves the definition of the skeleton with the specified asset name.</summary>
    /// <param name="skeletonAssetName">The name of the skeleton for which to retrieve the definition.</param>
    /// <returns>The specified skeleton definition, or null if not found.</returns>
    public OptitrackSkeletonDefinition GetSkeletonDefinitionByName( string skeletonAssetName )
    {
		if (ConnectionType == ClientConnectionType.Photon) {
			for (int i = 0; i < m_photonisedSkeletonDefinitions.Count; ++i) {
				OptitrackSkeletonDefinition skelDef = m_photonisedSkeletonDefinitions [i];

				if (skelDef.Name.Equals (skeletonAssetName, StringComparison.InvariantCultureIgnoreCase)) {
					return skelDef;
				}
			}
		} else {
			for (int i = 0; i < m_skeletonDefinitions.Count; ++i) {
				OptitrackSkeletonDefinition skelDef = m_skeletonDefinitions [i];

				if (skelDef.Name.Equals (skeletonAssetName, StringComparison.InvariantCultureIgnoreCase)) {
					return skelDef;
				}
			}
		}
        return null;
    }


    /// <summary>Request data descriptions from the host, then update our definitions.</summary>
    /// <exception cref="NatNetException">
    /// Thrown by <see cref="NatNetClient.GetDataDescriptions"/> if the request to the server fails.
    /// </exception>
    public void UpdateDefinitions()
    {
        if (m_client == null)
            return;

        // This may throw an exception if the server request times out or otherwise fails.
        m_dataDescs = m_client.GetDataDescriptions();

        m_skeletonDefinitions.Clear();

        for ( int nativeDescIdx = 0; nativeDescIdx < m_dataDescs.SkeletonDescriptions.Count; ++nativeDescIdx )
        {
            sSkeletonDescription nativeSkel = m_dataDescs.SkeletonDescriptions[nativeDescIdx];

            OptitrackSkeletonDefinition skelDef = new OptitrackSkeletonDefinition {
                Id = nativeSkel.Id,
                Name = nativeSkel.Name,
                Bones = new List<OptitrackSkeletonDefinition.BoneDefinition>( nativeSkel.RigidBodyCount ),
            };

            // Populate nested bone definitions.
            for ( int nativeBoneIdx = 0; nativeBoneIdx < nativeSkel.RigidBodyCount; ++nativeBoneIdx )
            {
                sRigidBodyDescription nativeBone = nativeSkel.RigidBodies[nativeBoneIdx];

                OptitrackSkeletonDefinition.BoneDefinition boneDef = new OptitrackSkeletonDefinition.BoneDefinition {
                    Id = nativeBone.Id,
                    ParentId = nativeBone.ParentId,
                    Name = nativeBone.Name,
                    Offset = new Vector3Serializer( nativeBone.OffsetX, nativeBone.OffsetY, nativeBone.OffsetZ ),
                };

                skelDef.Bones.Add( boneDef );
            }

            m_skeletonDefinitions.Add( skelDef );
        }

		BinaryFormatter binFormatter = new BinaryFormatter ();
		MemoryStream mStream = new MemoryStream ();
		binFormatter.Serialize (mStream, m_skeletonDefinitions);

		photonView.RPC ("RPC_UpdateDefinitions", PhotonTargets.AllBuffered, new object[]{mStream.ToArray ()});
    }
	[PunRPC]
	void RPC_UpdateDefinitions(byte[] skeletonDefinitions){
		BinaryFormatter binFormatter = new BinaryFormatter ();
		MemoryStream mStream = new MemoryStream ();

		mStream.Write (skeletonDefinitions, 0, skeletonDefinitions.Length);
		mStream.Position = 0;
		m_photonisedSkeletonDefinitions = binFormatter.Deserialize (mStream) as List<OptitrackSkeletonDefinition>;
	}
	public void TriggerUpdateDefinitions(){
		photonView.RPC ("RPC_TriggerUpdateDefinitions", PhotonTargets.MasterClient);
	}
	[PunRPC]
	void RPC_TriggerUpdateDefinitions(){
		UpdateDefinitions ();
	}

    /// <summary>
    /// (Re)initializes <see cref="m_client"/> and connects to the configured streaming server.
    /// </summary>
    void OnEnable(){
		InitializeConnection ();
    }
	public void InitializeConnection(){
		if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable ()) {
			LocalAddress = GetLocalIPAddress ();
		} else {
			Debug.LogError("No local network found");
		}
		IPAddress serverAddr = IPAddress.Parse( ServerAddress );
		IPAddress localAddr = IPAddress.Parse( LocalAddress );

		NatNetConnectionType connType;
		switch ( ConnectionType )
		{
		case ClientConnectionType.Unicast:
			connType = NatNetConnectionType.NatNetConnectionType_Unicast;
			break;
		case ClientConnectionType.Multicast:
		default:
			connType = NatNetConnectionType.NatNetConnectionType_Multicast;
			break;
		case ClientConnectionType.Photon:
			return;
		}

		try
		{
			m_client = new NatNetClient( connType );
			m_client.Connect( localAddr, serverAddr );
		}
		catch ( Exception ex )
		{
//			Debug.LogException( ex, this );
//			Debug.LogError( GetType().FullName + ": Error connecting to server; check your configuration, and make sure the server is currently streaming.", this );
			//Add Call to fallback, giving an error message
			Fallback.Instance.SendErrorMessage("Couldn't connect to motive. Make sure Motive has started and then relaunch playmode");
			this.enabled = false;
			return;
		}

		m_client.NativeFrameReceived += OnNatNetFrameReceived;
		m_connectionHealthCoroutine = StartCoroutine( CheckConnectionHealth() );
	}


    /// <summary>
    /// Disconnects from the streaming server and cleans up <see cref="m_client"/>.
    /// </summary>
    void OnDisable()
    {
        if ( m_connectionHealthCoroutine != null )
        {
            StopCoroutine( m_connectionHealthCoroutine );
            m_connectionHealthCoroutine = null;
        }

        m_client.NativeFrameReceived -= OnNatNetFrameReceived;
        m_client.Disconnect();
        m_client.Dispose();
        m_client = null;
    }


    System.Collections.IEnumerator CheckConnectionHealth()
    {
        const float kHealthCheckIntervalSeconds = 1.0f;
        const float kRecentFrameThresholdSeconds = 5.0f;

        // The lifespan of these variables is tied to the lifespan of a single connection session.
        // The coroutine is stopped on disconnect and restarted on connect.
        YieldInstruction checkIntervalYield = new WaitForSeconds( kHealthCheckIntervalSeconds );
        OptitrackHiResTimer.Timestamp connectionInitiatedTimestamp = OptitrackHiResTimer.Now();
        OptitrackHiResTimer.Timestamp lastFrameReceivedTimestamp;
        bool wasReceivingFrames = false;
        bool warnedPendingFirstFrame = false;

        while ( true )
        {
            yield return checkIntervalYield;

            if ( m_receivedFrameSinceConnect == false )
            {
                // Still waiting for first frame. Warn exactly once if this takes too long.
                if ( connectionInitiatedTimestamp.AgeSeconds > kRecentFrameThresholdSeconds )
                {
                    if ( warnedPendingFirstFrame == false )
                    {
                        Debug.LogWarning( GetType().FullName + ": No frames received from the server yet. Verify your connection settings are correct and that the server is streaming.", this );
                        warnedPendingFirstFrame = true;
                    }

                    continue;
                }
            }
            else
            {
                // We've received at least one frame, do ongoing checks for changes in connection health.
                lastFrameReceivedTimestamp.m_ticks = Interlocked.Read( ref m_lastFrameDeliveryTimestamp.m_ticks );
                bool receivedRecentFrame = lastFrameReceivedTimestamp.AgeSeconds < kRecentFrameThresholdSeconds;

                if ( wasReceivingFrames == false && receivedRecentFrame == true )
                {
                    // Transition: Bad health -> good health.
                    wasReceivingFrames = true;
                    Debug.Log( GetType().FullName + ": Receiving streaming data from the server.", this );
                    continue;
                }
                else if ( wasReceivingFrames == true && receivedRecentFrame == false )
                {
                    // Transition: Good health -> bad health.
                    wasReceivingFrames = false;
                    Debug.LogWarning( GetType().FullName + ": No streaming frames received from the server recently.", this );
                    continue;
                }
            }
        }
    }

	public static string GetLocalIPAddress()
	{
		var host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in host.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				return ip.ToString();
			}
		}
		throw new Exception("Local IP Address Not Found!");
	}


    #region Private methods
    /// <summary>
    /// Event handler for NatNet frame delivery. Updates our simplified state representations.
    /// NOTE: This executes in the context of the NatNetLib network service thread!
    /// </summary>
    /// <remarks>
    /// Because the <see cref="sFrameOfMocapData"/> type is expensive to marshal, we instead utilize the
    /// <see cref="NatNetClient.NativeFrameReceivedEventArgs.NativeFramePointer"/>, treating it as as opaque, and
    /// passing it to some helper "accessor" functions to retrieve the subset of data we care about, using only
    /// blittable types which do not cause any garbage to be allocated.
    /// </remarks>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    private void OnNatNetFrameReceived( object sender, NatNetClient.NativeFrameReceivedEventArgs eventArgs )
    {
        // In the event of contention, drop the frame being delivered and return immediately.
        // We don't want to stall NatNetLib's internal network service thread.
        if ( ! Monitor.TryEnter( m_frameDataUpdateLock ) )
        {
            return;
        }

        try
        {
            // Update health markers.
            m_receivedFrameSinceConnect = true;
            Interlocked.Exchange( ref m_lastFrameDeliveryTimestamp.m_ticks, OptitrackHiResTimer.Now().m_ticks );

            // Process received frame.
            IntPtr pFrame = eventArgs.NativeFramePointer;
            NatNetError result = NatNetError.NatNetError_OK;

            // Update rigid bodies.
            Int32 frameRbCount;
            result = NaturalPoint.NatNetLib.NativeMethods.NatNet_Frame_GetRigidBodyCount( pFrame, out frameRbCount );
            NatNetException.ThrowIfNotOK( result, "NatNet_Frame_GetRigidBodyCount failed." );

            for ( int rbIdx = 0; rbIdx < frameRbCount; ++rbIdx )
            {
                sRigidBodyData rbData = new sRigidBodyData();
                result = NaturalPoint.NatNetLib.NativeMethods.NatNet_Frame_GetRigidBody( pFrame, rbIdx, out rbData );
                NatNetException.ThrowIfNotOK( result, "NatNet_Frame_GetRigidBody failed." );

                bool bTrackedThisFrame = (rbData.Params & 0x01) != 0;
                if ( bTrackedThisFrame == false )
                {
                    continue;
                }

                // Ensure we have a state corresponding to this rigid body ID.
                OptitrackRigidBodyState rbState = GetOrCreateRigidBodyState( rbData.Id );

                rbState.DeliveryTimestamp = OptitrackHiResTimer.Now();

                // Flip coordinate handedness from right to left by inverting X and W.
				rbState.Pose.Position = new Vector3Serializer( -rbData.X, rbData.Y, rbData.Z );
				rbState.Pose.Orientation = new QuaternionSerializer( -rbData.QX, rbData.QY, rbData.QZ, -rbData.QW );

                rbState.Markers.Clear();

                for ( int markerIdx = 0; markerIdx < rbData.MarkerCount; ++markerIdx )
                {
                    float markerX, markerY, markerZ;
					result = NaturalPoint.NatNetLib.NativeMethods.NatNet_Frame_RigidBody_Marker_GetPosition( pFrame, rbIdx, markerIdx, out markerX, out markerY, out markerZ );
                    NatNetException.ThrowIfNotOK( result, "NatNet_Frame_RigidBody_Marker_GetPosition failed." );

                    float markerSize;
                    result = NaturalPoint.NatNetLib.NativeMethods.NatNet_Frame_RigidBody_Marker_GetSize( pFrame, rbIdx, markerIdx, out markerSize );
                    NatNetException.ThrowIfNotOK( result, "NatNet_Frame_RigidBody_Marker_GetPosition failed." );

                    // Change of basis from right- to left-handed by inverting X.
                    rbState.Markers.Add( new OptitrackMarkerState {
						Position = new Vector3Serializer( -markerX, markerY, markerZ ),
                        Size = markerSize
                    } );
                }
            }

            // Update skeletons.
            Int32 frameSkeletonCount;
            result = NaturalPoint.NatNetLib.NativeMethods.NatNet_Frame_GetSkeletonCount( pFrame, out frameSkeletonCount );
            NatNetException.ThrowIfNotOK( result, "NatNet_Frame_GetSkeletonCount failed." );

            for ( int skelIdx = 0; skelIdx < frameSkeletonCount; ++skelIdx )
            {
                Int32 skeletonId;
                result = NaturalPoint.NatNetLib.NativeMethods.NatNet_Frame_Skeleton_GetId( pFrame, skelIdx, out skeletonId );
                NatNetException.ThrowIfNotOK( result, "NatNet_Frame_GetSkeletonId failed." );

                // Ensure we have a state corresponding to this skeleton ID.
                OptitrackSkeletonState skelState = GetOrCreateSkeletonState( skeletonId );

                // Enumerate this skeleton's bone rigid bodies.
                Int32 skelRbCount;
                result = NaturalPoint.NatNetLib.NativeMethods.NatNet_Frame_Skeleton_GetRigidBodyCount( pFrame, skelIdx, out skelRbCount );
                NatNetException.ThrowIfNotOK( result, "NatNet_Frame_GetSkeletonRigidBodyCount failed." );

                for ( int boneIdx = 0; boneIdx < skelRbCount; ++boneIdx )
                {
                    sRigidBodyData boneData = new sRigidBodyData();
                    result = NaturalPoint.NatNetLib.NativeMethods.NatNet_Frame_Skeleton_GetRigidBody( pFrame, skelIdx, boneIdx, out boneData );
                    NatNetException.ThrowIfNotOK( result, "NatNet_Frame_GetSkeletonRigidBody failed." );

                    // In the context of frame data (unlike in the definition data), this ID value is a
                    // packed composite of both the asset/entity (skeleton) ID and member (bone) ID.
                    Int32 boneSkelId, boneId;
                    NaturalPoint.NatNetLib.NativeMethods.NatNet_DecodeID( boneData.Id, out boneSkelId, out boneId );

                    // TODO: Could pre-populate this map when the definitions are retrieved.
                    // Should never allocate after the first frame, at least.
                    if ( skelState.BonePoses.ContainsKey( boneId ) == false )
                    {
                        skelState.BonePoses[boneId] = new OptitrackPose();
                    }

                    // Flip coordinate handedness from right to left by inverting X and W.
					skelState.BonePoses[boneId].Position = new Vector3Serializer( -boneData.X, boneData.Y, boneData.Z );
					skelState.BonePoses[boneId].Orientation = new QuaternionSerializer( -boneData.QX, boneData.QY, boneData.QZ, -boneData.QW );
                }
            }
        }
        catch ( Exception ex )
        {
            Debug.LogError( GetType().FullName + ": OnNatNetFrameReceived encountered an exception.", this );
            Debug.LogException( ex, this );
        }
        finally
        {
            Monitor.Exit( m_frameDataUpdateLock );
        }
    }


    /// <summary>
    /// Returns the <see cref="OptitrackRigidBodyState"/> corresponding to the provided <paramref name="rigidBodyId"/>.
    /// If the requested state object does not exist yet, it will initialize and return a newly-created one.
    /// </summary>
    /// <remarks>Makes the assumption that the lock on <see cref="m_frameDataUpdateLock"/> is already held.</remarks>
    /// <param name="rigidBodyId">The ID of the rigid body for which to retrieve the corresponding state.</param>
    /// <returns>The existing state object, or a newly created one if necessary.</returns>
    private OptitrackRigidBodyState GetOrCreateRigidBodyState( Int32 rigidBodyId )
    {
        OptitrackRigidBodyState returnedState = null;

        if ( m_latestRigidBodyStates.ContainsKey( rigidBodyId ) )
        {
            returnedState = m_latestRigidBodyStates[rigidBodyId];
        }
        else
        {
            OptitrackRigidBodyState newRbState = new OptitrackRigidBodyState {
                Pose = new OptitrackPose(),
                Markers = new List<OptitrackMarkerState>(),
            };

            m_latestRigidBodyStates[rigidBodyId] = newRbState;

            returnedState = newRbState;
        }

        return returnedState;
    }


    /// <summary>
    /// Returns the <see cref="OptitrackSkeletonState"/> corresponding to the provided <paramref name="skeletonId"/>.
    /// If the requested state object does not exist yet, it will initialize and return a newly-created one.
    /// </summary>
    /// <remarks>Makes the assumption that the lock on <see cref="m_frameDataUpdateLock"/> is already held.</remarks>
    /// <param name="skeletonId">The ID of the skeleton for which to retrieve the corresponding state.</param>
    /// <returns>The existing state object, or a newly created one if necessary.</returns>
    private OptitrackSkeletonState GetOrCreateSkeletonState( Int32 skeletonId )
    {
        OptitrackSkeletonState returnedState = null;

        if ( m_latestSkeletonStates.ContainsKey( skeletonId ) )
        {
            returnedState = m_latestSkeletonStates[skeletonId];
        }
        else
        {
            OptitrackSkeletonState newSkeletonState = new OptitrackSkeletonState {
                BonePoses = new Dictionary<Int32, OptitrackPose>()
            };

            m_latestSkeletonStates[skeletonId] = newSkeletonState;

            returnedState = newSkeletonState;
        }

        return returnedState;
    }


    public void _EnterFrameDataUpdateLock()
    {
        Monitor.Enter( m_frameDataUpdateLock );
    }


    public void _ExitFrameDataUpdateLock()
    {
        Monitor.Exit( m_frameDataUpdateLock );
    }
    #endregion Private methods

    public void StartRecording()
    {
        lock (m_frameDataUpdateLock)
        {
            m_client.RemoteTrigger("StartRecording");
        }
        
        m_client.NativeFrameReceived += OnNatNetFrameReceived;
    }

    public void StopRecording()
    {
        lock (m_frameDataUpdateLock)
        {
            m_client.RemoteTrigger("StopRecording");
        }
        
        m_client.NativeFrameReceived += OnNatNetFrameReceived;
    }







	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting )
		{
			BinaryFormatter binFormatter = new BinaryFormatter ();
			MemoryStream mStream = new MemoryStream ();

			binFormatter.Serialize (mStream, m_latestRigidBodyStates);
			stream.SendNext(mStream.ToArray ());



			binFormatter = new BinaryFormatter ();
			mStream = new MemoryStream ();

			binFormatter.Serialize (mStream, m_latestSkeletonStates);
			stream.SendNext(mStream.ToArray ());
		}
		else
		{
			BinaryFormatter binFormatter = new BinaryFormatter ();
			MemoryStream mStream = new MemoryStream ();

//			m_previousPhotonisedLatestRigidBodyStates = m_photonisedLatestRigidBodyStates;
		
			byte[] byteArray = (byte[])stream.ReceiveNext ();
			mStream.Write (byteArray, 0, byteArray.Length);
			mStream.Position = 0;
			m_photonisedLatestRigidBodyStates = binFormatter.Deserialize (mStream) as Dictionary<int, OptitrackRigidBodyState>;

			foreach(var item in m_photonisedLatestRigidBodyStates){
				m_photonisedLatestRigidBodyStates [item.Key].Pose.ResetTime ();
			}

//			foreach(var item in m_previousPhotonisedLatestRigidBodyStates){
//				Debug.Log ("item: " + item);
//				Debug.Log ("item.key: " + item.Key);
//				Debug.Log ("item.Value: " + item.Value);
//				OptitrackRigidBodyState ors = item.Value as OptitrackRigidBodyState;
//				m_photonisedLatestRigidBodyStates[item.Key].Pose.previousUpdatedPositon = ors.Pose.Position;
//				m_photonisedLatestRigidBodyStates[item.Key].Pose.previousUpdatedOrientation = ors.Pose.Orientation;
//			}
			// THESE NEW DICTIONARIES OVVERRIDES THE LERP

			binFormatter = new BinaryFormatter ();
			mStream = new MemoryStream ();

//			m_previousPhotonisedLatestSkeletonStates = m_photonisedLatestSkeletonStates;


			byteArray = (byte[])stream.ReceiveNext ();
			mStream.Write (byteArray, 0, byteArray.Length);
			mStream.Position = 0;
			m_photonisedLatestSkeletonStates = binFormatter.Deserialize (mStream) as Dictionary<int, OptitrackSkeletonState>;
		}
	}
}
