using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class NetworkReceiver : MonoBehaviour {

    private TcpListener tcpListener;
    private Thread tcpListenerThread;
    private TcpClient connectedTcpClient;
    public Text txt;
    string clientMessage;
    string[] values, objects;
    public int port;

    // Use this for initialization
    void Start () {
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncoming));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }
    void ListenForIncoming()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Any, 22222);
            tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            tcpListener.Start();
            Debug.Log("Server listening...");
            byte[] bytes = new byte[1024];
            while (true)
            {
                using (connectedTcpClient = tcpListener.AcceptTcpClient())
                {
                    using (NetworkStream stream = connectedTcpClient.GetStream())
                    {
                        int length;
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incomingData = new byte[length];
                            Array.Copy(bytes, 0, incomingData, 0, length);
                            clientMessage = Encoding.ASCII.GetString(incomingData);
                            Debug.Log("Message received" + clientMessage);
                        }
                    }
                }
            }
        }
        catch (SocketException e)
        {
            Debug.Log("Socket Exception " + e);
        }
    }

	// Update is called once per frame
	void Update () {
        
    }
}
