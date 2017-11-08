using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;

#if UNITY_WSA_10_0 && !UNITY_EDITOR

#else
using System.Net;
using System.Net.Sockets;
using System.Text;
#endif

using System.Threading;


public class TcpServerCmd : MonoBehaviour
{
    public int ConnectionPort = 50000;
#if UNITY_WSA_10_0 && !UNITY_EDITOR

#else
    private TcpListener networkListener;
    private TcpClient networkClient;
    private bool ClientConnected = false;
    private NetworkStream stream;
    public RemoteCmdHandler handler;
    public bool canStart = false;
    bool shouldStartReceiving = false;


    // Use this for initialization
    void Start()
    {
        //IPAddress localAddress = IPAddress.Parse(ServerIP.Trim());

    }

    // Update is called once per frame
    void Update()
    {
        //return;
        if (shouldStartReceiving)
        {
            //StartCoroutine(StartReceiving());
            Thread th = new Thread(StartReceiving);
            th.Start();
            shouldStartReceiving = false;
        }

    }
    void StartReceiving()
    {
        int count = 0;
        //return;
        while(ClientConnected)
        {
            if (networkClient.Connected)
            {
                try
                {
                    count = 0;
                    //stream.ReadTimeout = 1;
                    if (stream.CanRead)
                    {
                        int responseLength = ReadInt(stream);
                        //stream.Read(responseLBytes, 0, sizeof(Int32));
                        //int responseLength = BitConverter.ToInt32(responseLBytes, 0);
                        byte[] responseBytes = new byte[responseLength];
                        //Debug.Log("available bytes in the client: " + responseLength);
                        stream.Read(responseBytes, 0, responseLength);
                        //Array.Reverse(responseBytes);
                        string responseString = System.Text.Encoding.ASCII.GetString(responseBytes);
                        RemoteCmd cmd = JsonUtility.FromJson<RemoteCmd>(responseString);
                        handler.OnRemoteCmdReceivedAsync(cmd);
                        handler.EnqueueCmd(responseString);
                        //Debug.Log(responseString);
                    }
                    else
                    {
                        Debug.Log("something wrong with reading cannot read");
                    }
                }
                catch (Exception ex)
                {
                    
                    Debug.Log("Exception Thrown");
                    Debug.Log(ex.Message);
                    //Debug.Log(count);
                    ClientConnected = false;
                    stream.Close();
                    networkClient.Close();
                        //AsyncCallback callback = new AsyncCallback(OnClientConnected);
                        //networkListener.BeginAcceptTcpClient(callback, networkListener);
                }
            }
        }
    }

    public void Init(string ip, int port, RemoteCmdHandler handler)
    {
        this.ConnectionPort = port;
        this.handler = handler;
        IPAddress localAddress = IPAddress.Any;
        networkListener = new TcpListener(localAddress, ConnectionPort);
        networkListener.Start();
        AsyncCallback callback = new AsyncCallback(OnClientConnected);
        networkListener.BeginAcceptTcpClient(callback, networkListener);
    }
    void OnClientConnected(IAsyncResult result)
    {
        Debug.Log("OnClient function started");
        if (result.IsCompleted)
        {
            networkClient = networkListener.EndAcceptTcpClient(result);
            //networkClient.Client.RemoteEndPoint;
            if (networkClient != null)
            {
                ClientConnected = true;
                Debug.Log("Connected");
                shouldStartReceiving = true;
                stream = networkClient.GetStream();
            }
        }
    }
  public void SendMsg(string data)
    {
        if (ClientConnected)
        {
            stream = networkClient.GetStream();
            byte[] dataBuffer = Encoding.ASCII.GetBytes(data);
            int length = dataBuffer.Length;
            byte[] datasize = BitConverter.GetBytes(length);

            //Debug.Log("networkClient is Connected");
            //Debug.Log("Data Changed");
            try
            {
                if (networkClient.Connected)
                {
                    if (stream.CanWrite)
                    {
                        stream.Write(datasize, 0, datasize.Length);

                        stream.Write(dataBuffer, 0, dataBuffer.Length);
                        //Debug.Log("Data size: " + dataBuffer.Length);
                        //Debug.Log("The stream can write");
                    }
                    else
                    {
                        Debug.Log("Can not write");
                    }
                }
                else
                {
                    //networkClient.Connected = false
                    Debug.Log("Client Disconnected");
                    ClientConnected = false;
                    stream.Close();
                    networkClient.Close();
                    AsyncCallback callback = new AsyncCallback(OnClientConnected);
                    networkListener.BeginAcceptTcpClient(callback, networkListener);
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(IOException))
                {
                    Debug.Log("Exception Thrown");
                    ClientConnected = false;
                    stream.Close();
                    networkClient.Close();
                    AsyncCallback callback = new AsyncCallback(OnClientConnected);
                    networkListener.BeginAcceptTcpClient(callback, networkListener);
                }
            }



        }
    }

    public void Stop()
    {
        if(stream != null)
            stream.Close();
        if(networkClient != null)
            networkClient.Close();
        ClientConnected = false;

    }
    public void sendCmd(RemoteCmdType type, string data)
    {
        sendCmd(ConnectionPort, type, data);
    }

    public void sendCmd(RemoteCmdType type, string dest, string data)
    {
        sendCmd(ConnectionPort, type, dest, data);
    }

    private void sendCmd(int port, RemoteCmdType type, string data)
    {
        sendCmd(ConnectionPort, type, "all", data);
    }
    private void sendCmd(int port, RemoteCmdType type, string dest, string data)
    {
        SendMsg(UnityEngine.JsonUtility.ToJson(new RemoteCmd(type, dest, data)));
    }

    int ReadInt(NetworkStream stream)
    {
        // The bytes arrive in the wrong order, so swap them.
        byte[] bytes = new byte[4];
        stream.Read(bytes, 0, 4);
        Array.Reverse(bytes);
        //byte t = bytes[0];
        //bytes[0] = bytes[3];
        //bytes[3] = t;

        //t = bytes[1];
        //bytes[1] = bytes[2];
        //bytes[2] = t;

        // Then bitconverter can read the int32.
        return BitConverter.ToInt32(bytes, 0);
    }
    //int ReadInt(Stream stream)
    //{
    //    byte[] bytes = new byte[4];
    //    stream.Read(bytes, 0, 4);
    //    byte t = bytes[1];
    //    bytes[0] = bytes[3];
    //    bytes[3] = t;

    //    t = bytes[1];
    //    bytes[1] = bytes[2];
    //    bytes[2] = t;

    //    return BitConverter.ToInt32(bytes, 0);
    //}
#endif
}
