using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
public class DataReceive : MonoBehaviour
{
    FanReceive fanReceive;
    //three sockets, in, out and extra info
    private TcpClient socketConnectionIn, socketConnectionOut, socketConnectionExtra;
    //threads for sending and receiving messages
    private Thread clientReceiveThread, sendMsgThread;
    //strings for sending and receiving messages
    public string str, serverMessage;
    //encoded bytes
    byte[] _str;
    //string for the debug console
    string msg = "";

    public Text IPAddrInput;

    public Button UpdateIP;

    string IPAddr;

    public string extraID;
    string[] ID, status;

   // public Text extraIDDisp;

    // Start is called before the first frame update
    void Start()
    {
        ID = new string[100];
        status = new string[100];
        fanReceive = GameObject.Find("Twin").GetComponentInChildren<FanReceive>();
        UpdateIP.onClick.AddListener(NewIP);
        //begin server connections
        ConnectToTcpServer();
    }

    void NewIP()
    {
        try
        {
            socketConnectionIn.Close();
            socketConnectionOut.Close();
            socketConnectionExtra.Close();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        clientReceiveThread.Abort();
        ConnectToTcpServer();
    }

        //start receive thread
    void ConnectToTcpServer()
    {
        try
        {
            //create a thread to listen for data
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            //run thread in the background
            clientReceiveThread.IsBackground = true;
            //start the thread
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("Client connect " + e);
            msg = "Client connect " + e;
        }
    }

    //listening for data received
    void ListenForData()
    {
        IPAddr = IPAddrInput.text;
        Debug.Log(IPAddr);
        try
        {
            //new socket for inbound connections
            socketConnectionIn = new TcpClient(IPAddr, 22222);
            msg += "In Socket Connected";
            //new socket for outbound connections
            socketConnectionOut = new TcpClient(IPAddr, 33333);
            msg += "Out Socket Connected";
            //new socket for ID info
            socketConnectionExtra = new TcpClient(IPAddr, 55555);
            msg += "Extra Socket Connected";
            //Get extra ID info
            getExtraInfo();
            //reserve 1kb for the byte array
            byte[] bytes = new byte[1024];
            while (true)
            {
                //read from the inbound socket
                using (NetworkStream stream = socketConnectionIn.GetStream())
                {
                    //counter for each byte
                    int length;
                    //read through the bytes until there are none remaining (FiFo)
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        //create a new array for storing the received data
                        var incomingData = new byte[length];
                        //copy the received bytes into the array
                        Array.Copy(bytes, 0, incomingData, 0, length);
                        //decode the received bytes into a string and store in serverMessage
                        serverMessage = Encoding.Default.GetString(incomingData);
                        Debug.Log("Message received: " + serverMessage);
                    }
                }
            }
        }
        catch (SocketException e)
        {
            Debug.Log("Socket Exception " + e);
            msg = "Socket Exception " + e;
        }
    }

    public void getExtraInfo()
    {
        try
        {
            byte[] r = Encoding.ASCII.GetBytes("r");
            NetworkStream stream = socketConnectionExtra.GetStream();
            stream.Write(r, 0, r.Length);
            Debug.Log("Extra info requested");
            //reserve 1kb for the byte array
            byte[] bytes = new byte[1024];
            //read from the extra socket
            using (NetworkStream networkStream = socketConnectionExtra.GetStream())
            {
                //counter for each byte
                int length;
                //read through the bytes until there are none remaining (FiFo)
                while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    //create a new array for storing the received data
                    var incomingData = new byte[length];
                    //copy the received bytes into the array
                    Array.Copy(bytes, 0, incomingData, 0, length);
                    //decode the received bytes into a string and store in serverMessage
                    extraID = Encoding.Default.GetString(incomingData);
                    Debug.Log("Message received: " + extraID);
                    updateIDs();
                }
            }
        }
        catch (SocketException e)
        {
            Debug.Log("Socket Exception " + e);
            msg = "Socket Exception " + e;
        }
    }

    public void updateIDs()
    {
        //string[] extraTemp = extraID.Split('\n');
        //foreach (string data in extraTemp)
        //{
        //    if (data != "")
        //    {
        //        string[] idData = data.Split('+');
        //        if (idData.Length == 2)
        //        {
        //            string idTemp = idData[0];
        //            string nameTemp = idData[1];
        //            int id = int.Parse(idTemp);
        //            ID[id] = nameTemp;
        //        }
        //        if(idData.Length == 3)
        //        {
        //            string idTemp = idData[0];
        //            string nameTemp = idData[1];
        //            string statusTemp = idData[2];
        //            int id = int.Parse(idTemp);
        //            ID[id] = nameTemp;
        //            status[id] = statusTemp;
        //        }
        //    }
        //}

    }

    private void Update()
    {
        //extraIDDisp.text = "";
        //for (int i = 0; i < ID.Length; i++)
        //{
            
        //    if (status[i] != null)
        //    {
        //        extraIDDisp.text += ID[i] + status[i] + "\n";
        //    }
        //    else
        //    {
        //        extraIDDisp.text += ID[i] + "\n";
        //    }
        //}
    }


    //send a message to the server (this is called from outside scripts)
    public void SendMsg(string str)
    {
        //stop if there's no outbound connection
        if (socketConnectionOut == null)
        {
            Debug.Log("Socket Connection Null");
            return;
        }
        try
        {
            //encode the string to bytes
            _str = Encoding.ASCII.GetBytes(str);
            //get the outgoing network stream to write to
            NetworkStream stream = socketConnectionOut.GetStream();
            //write the message to the stream
            stream.Write(_str, 0, _str.Length);
            Debug.Log("Message Sent");
            msg = "Message Sent";
        }
        catch (SocketException e)
        {
            Debug.Log("Socket Exception " + e);
            msg = "Socket Exception " + e;
        }
    }

    //called when the application closes
    private void OnApplicationQuit()
    {
        //close the sockets to prevent application hang
        try
        {
            socketConnectionIn.Close();
            socketConnectionOut.Close();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}
