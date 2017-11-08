using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;

#if UNITY_WSA_10_0 && !UNITY_EDITOR
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Networking;
using Windows.Foundation;
using Windows.System;

using System;
#endif

public enum DataType
{
    Integer,
    Float,
    Mesh
}


public class TcpClientCmd : MonoBehaviour
{
    public string ServerIP;
    public int ConnectionPort = 11000;
    public RemoteCmdHandler handler;

    private bool canStart = false;
#if UNITY_WSA_10_0 && !UNITY_EDITOR

    private StreamSocket networkConnection;
    private bool Connecting = false;
    private bool Waiting = false;
    private bool NeedToWait = false;
    private float deferTime = 0.0f;
    byte[] data = new byte[1024];
    private float timeToDeferFailedConnections = 10.0f;
    DataReader reader;
    DataWriter writer;
    bool ConnectionEstablished = false;


    // Use this for initialization
    void Start()
    {

        //ConnectToServer();
        //networkConnection = new StreamSocket();
        //HostName networkHost = new HostName("152.2.133.188");
        //IAsyncAction outstandingAcion = networkConnection.ConnectAsync(networkHost, "50000");
        //AsyncActionCompletedHandler aach = new AsyncActionCompletedHandler(NetworkConnectedHandler);
        //outstandingAcion.Completed = aach;

    }

    // Update is called once per frame
    void Update()
    {
        if (!canStart)
            return;

        if (!Connecting && !ConnectionEstablished && NeedToWait)
        {
            WaitAndStartConnecting(timeToDeferFailedConnections);
        }
        if (!Connecting && !ConnectionEstablished && !Waiting)
        {
            ConnectToServer();
        }
        //if (ConnectionEstablished)
        //{
        //    return;
        //    //reader.InputStreamOptions = InputStreamOptions.Partial;
        //    if (reader != null)
        //    {
        //        //string recv = reader.ReadString(1024);
        //        //if(recv.Length > 0)
        //        //Debug.Log("RECV " + recv);
        //        DataReaderLoadOperation operation = reader.LoadAsync(1);
        //        operation.Completed = new AsyncOperationCompletedHandler<uint>(DataReceivedHandler);
        //    }

        //}

    }
    public void Init(string ip, int port,RemoteCmdHandler handler)
    {
        this.ServerIP = ip;
        this.ConnectionPort = port;
        this.handler = handler;
        canStart = true;
    }

    async void NetworkConnectedHandler(IAsyncAction asyncInfo, AsyncStatus status)
    {
        Debug.Log("NetworkConnectedHandler started");
        if (status == AsyncStatus.Completed)
        {
            Debug.Log("NetworkConnectedHandler connected");
            writer = new DataWriter(networkConnection.OutputStream);
            reader = new DataReader(networkConnection.InputStream);
            reader.InputStreamOptions = InputStreamOptions.Partial;
            ConnectionEstablished = true;
            Connecting = false;

            while (true)
            {
                //Debug.Log("while loop");
                try
                {
                    uint fieldCount = await reader.LoadAsync(sizeof(uint));
                    int stringLength = ReadInt(reader);
                    uint restLength = reader.UnconsumedBufferLength;
                   // Debug.Log("unconsumedBufferLength is " + restLength + " stringlengthToread is:" + stringLength);
                    uint ActualStringLength = await reader.LoadAsync((uint)stringLength);
                    uint byteCollected = 0;
                    string readString = "";
                    while (byteCollected < stringLength)
                    {
                        readString += reader.ReadString(ActualStringLength);
                        byteCollected += ActualStringLength;
                        if ((uint) stringLength - byteCollected > 0)
                           ActualStringLength = await reader.LoadAsync((uint)stringLength - byteCollected);
                    }
                    RemoteCmd cmd = JsonUtility.FromJson<RemoteCmd>(readString);
                    handler.OnRemoteCmdReceivedAsync(cmd);
                    handler.EnqueueCmd(readString);
                    //Debug.Log("String: " + readString);
                }
                catch (Exception ex)
                {
                    Debug.Log("Connection Lost");
                    //Debug.Log(ex.Message);
                    ConnectionEstablished = false;
                    NeedToWait = true;
                    //ConnectToServer();
                    return;
                }
            }



            //operation.Completed = new AsyncOperationCompletedHandler<uint>(DataReceivedHandler);
        }
        else
        {
            Debug.Log("Failed to establish connection. Error Code: " + asyncInfo.ErrorCode);
            // In the failure case we'll requeue the data and wait before trying again.
            networkConnection.Dispose();

            Connecting = false;
            NeedToWait = true;
        }
    }

    int ReadInt(DataReader reader)
    {
        // The bytes arrive in the wrong order, so swap them.
        byte[] bytes = new byte[4];
        reader.ReadBytes(bytes);
        // Array.Reverse(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }
    void WaitAndStartConnecting(float deferTime)
    {
        NeedToWait = false;
        Invoke("StartConnecting", deferTime);
        Waiting = true;
    }
    private async void sendMsg(string data)
    {
        byte[] responseBytes;

        if (!ConnectionEstablished)
            return;
            try
            {
                    
                    responseBytes = System.Text.Encoding.ASCII.GetBytes(data);
                    //Debug.Log((Int32)responseBytes.Length);
                    writer.WriteInt32(responseBytes.Length);
                    writer.WriteBytes(responseBytes);
                    await writer.StoreAsync();

                
            }
            catch (Exception ex)
            {
                Debug.Log("Exception Thrown when sending");
                Debug.Log(ex.Message);
            }
        //Debug.Log("store result is " + storeResult + "response is " + responseBytes.Length);
    }

    void StartConnecting()
    {
        Waiting = false;
        //ConnectToServer();
    }
    //void DataReceivedHandler(IAsyncOperation<uint> operation, AsyncStatus status)
    //{
    //    Debug.Log("DataReceivedHandler Started");
    //    if (status == AsyncStatus.Error)
    //    {
    //        Debug.Log("Connection Lost");
    //        ConnectToServer();
    //    }
    //    else
    //    {
    //        string recv = "";
    //        while (reader.UnconsumedBufferLength > 0)
    //        {
    //            recv = reader.ReadString(reader.UnconsumedBufferLength);
    //            //await reader.LoadAsync(256);
    //        }
    //        Connecting = false;
    //        //Debug.Log("Received String is: " + System.Text.Encoding.ASCII.GetString(data));
    //        Debug.Log("Received String is: " + recv);
    //        DataWriter writer = new DataWriter(networkConnection.OutputStream);
    //        string response = "Data Received with " + recv.Length + "Characters";
    //        //byte[] responseBytes = System.Text.Encoding.ASCII.GetBytes(response);
    //        //writer.WriteInt32(responseBytes.Length);
    //        //writer.WriteBytes(responseBytes);

    //        //DataWriterStoreOperation dwso = writer.StoreAsync();
    //        //dwso.Completed
    //        //networkConnection.Dispose();
    //    }
    //    //Debug.Log("Received String is: " + System.Text.Encoding.ASCII.GetString(data));
    //    //networkConnection.Dispose();
    //}

    void ConnectToServer()
    {
        Connecting = true;
        Debug.Log("try connecting now");
        networkConnection = new StreamSocket();
        HostName networkHost = new HostName(this.ServerIP);
        IAsyncAction outstandingAcion = networkConnection.ConnectAsync(networkHost, this.ConnectionPort.ToString());
        AsyncActionCompletedHandler aach = new AsyncActionCompletedHandler(NetworkConnectedHandler);
        outstandingAcion.Completed = aach;
    }
    public void Stop()
    {
        canStart = false;
        networkConnection.Dispose();
        ConnectionEstablished = false;
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
        this.sendMsg(UnityEngine.JsonUtility.ToJson(new RemoteCmd(type, dest, data)));
    }
#endif
}
