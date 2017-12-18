using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using System.Text;
using System.Threading;
using UnityEngine;

public class RemoteTrigger : MonoBehaviour {

    // sender
    [HideInInspector]
    public UdpClient udpSender;
    IPEndPoint SendIpEndPoint;
    IPEndPoint LocalIpPoint;
    int senderPort = 1510;

    private List<string> listMessages = new List<string>();
    public Thread controlThread;

    void Start()
    {
        InitiateUDP();
    }

    public void InitiateUDP()
    {
        // Show available ip addresses of this machine
        string strMachineName = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(strMachineName);
        string localIP = null;
        foreach (IPAddress ip in ipHost.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                localIP = ip.ToString();
        }

        // init sender socket
        try
        {
            SendIpEndPoint = new IPEndPoint(IPAddress.Broadcast, senderPort);
            udpSender = new UdpClient();
            udpSender.EnableBroadcast = true;
            udpSender.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // in the event of multiple adapters in local PC, let user specify a particular adapter 
            LocalIpPoint = new IPEndPoint(IPAddress.Parse(localIP), senderPort);
            udpSender.Client.Bind(LocalIpPoint);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Exception during broadcast message attempt : " + ex);
        }
    }

    public void SendMessageAndSetTakeName(string takeName)
    {
        var message = "SetRecordTakeName," + takeName;
        SendMessage(message);
    }

    public void SendMessageAndStartRecording()
    {
        var message = "StartRecording";
        SendMessage(message);
    }

    public void SendMessageAndStopRecording()
    {
        var message = "StopRecording";
        SendMessage(message);
    }

    private void SendMessage(string message)
    {
        // motive assumes null terminated message strings (c-style)
        message += '\0';

        short messageID = 2;
        short payloadLength = (short)message.Length;
        byte[] messageIDBytes = BitConverter.GetBytes(messageID);
        byte[] payloadLengthBytes = BitConverter.GetBytes(payloadLength);
        int val = message.Length + 2 + 2;
        Byte[] sendBytes = new Byte[val];
        sendBytes[0] = messageIDBytes[0];
        sendBytes[1] = messageIDBytes[1];
        sendBytes[2] = payloadLengthBytes[0];
        sendBytes[3] = payloadLengthBytes[1];
        int ret = Encoding.ASCII.GetBytes(message, 0, message.Length, sendBytes, 4); // payload: encode a string to a byte array

        // Broadcast a message
        try
        {
            int nBytesSent = udpSender.Send(sendBytes, sendBytes.Length, SendIpEndPoint);
            Debug.Log("Sent Message (" + nBytesSent.ToString() + " bytes): " + Encoding.ASCII.GetString(sendBytes));
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception during broadcast message attempt : " + ex.ToString());
        }
    }

}
