//#if !UNITY_EDITOR
//#if !UNITY_STANDALONE
//#define HOLO_CHECK
//#endif
//#endif

#if UNITY_WSA_10_0  && !UNITY_EDITOR
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#else
using System.Net.Sockets;
#endif

using System.Net;

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;

public enum RemoteCmdType
{
    MoveRef,
    ShowObj,
    HideObj,

    SyncFrame,
    EnableObj,
    LatencyTest
}

[Serializable]
public class RemoteCmd
{
    public RemoteCmdType type;
    public string dest;
    public string data;
    public RemoteCmd(RemoteCmdType type_, string dest_, string data_) { type = type_; dest = dest_; data = data_; }
    public RemoteCmd(RemoteCmdType type_, string data_) { type = type_; dest = "all"; data = data_; }
}


public class RemoteCmdHandler : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public int port = 7779;

    public static RemoteCmdHandler Instance { get; private set; }

    public delegate void CmdDelegate(string dest_, string data_);
    //CmdDelegate[] delegateHandler = new CmdDelegate[System.Enum.GetValues(typeof(RemoteCmdType)).Length];
    //List<Tuple<string, CmdDelegate>>[] delegateHandler = new List<Tuple<string, CmdDelegate>>[System.Enum.GetValues(typeof(RemoteCmdType)).Length];
    List<KeyValuePair<string, CmdDelegate>>[] delegateHandler = new List<KeyValuePair<string, CmdDelegate>>[System.Enum.GetValues(typeof(RemoteCmdType)).Length];
    List<KeyValuePair<string, CmdDelegate>>[] delegateHandlerAsync = new List<KeyValuePair<string, CmdDelegate>>[System.Enum.GetValues(typeof(RemoteCmdType)).Length];

    private static Hashtable ht = new Hashtable();
#if UNITY_WSA_10_0 && !UNITY_EDITOR
    private TcpClientCmd client { get; set; }
#else
    private TcpServerCmd client { get; set; }
#endif
    //private CmdClient client { get; set; }

    private object q_lock = new object();
    private Queue<string> receivedCmds = new Queue<string>();

    void Awake()
    {
        for(int i = 0; i < delegateHandler.Length; ++i)
        {
            //delegateHandler[i] = new List<Tuple<string, CmdDelegate>>();
            delegateHandler[i] = new List<KeyValuePair<string, CmdDelegate>>();

        }
        for (int i = 0; i < delegateHandlerAsync.Length; ++i)
        {
            //delegateHandler[i] = new List<Tuple<string, CmdDelegate>>();
            delegateHandlerAsync[i] = new List<KeyValuePair<string, CmdDelegate>>();

        }

        Instance = this;
    }

    void Start()
    {
        if (ht.ContainsKey(port))
        {
            //Debug.Log("HoloClient Instance Found");
            //this.client = (CmdClient)ht[port];
#if UNITY_WSA_10_0 && !UNITY_EDITOR
            this.client = (TcpClientCmd)ht[port];
#else
            this.client = (TcpServerCmd)ht[port];
#endif

        }
        else
        {
            //HoloClient hc = new HoloClient(port);
            //this.client = this.gameObject.AddComponent<CmdClient>();
#if UNITY_WSA_10_0 && !UNITY_EDITOR
            this.client = this.gameObject.AddComponent<TcpClientCmd>();
#else
            this.client = this.gameObject.AddComponent<TcpServerCmd>();
#endif

            this.client.Init(ip, port, this);
            ht[port] = this.client;
            //Debug.Log("HoloClient Instance " + this.Instance);
        }
    }

    void OnApplicationQuit()
    {
        if(this.client)
            this.client.Stop();
    }

    public void SendRemoteCmd(RemoteCmdType type, string dest, string data, bool sendToLocal = true)
    {
        client.sendCmd(type, dest, data);
        if (sendToLocal)
        {
            EnqueueCmd(GetCmdString(type, dest, data));
        }
    }

    public void SendRemoteCmd(RemoteCmdType type, string dest, HoloTransform ht, bool sendToLocal = true)
    {
        SendRemoteCmd(type, dest, UnityEngine.JsonUtility.ToJson(ht), sendToLocal);
    }

    public void RegisterForCmd(RemoteCmdType type, string dest, CmdDelegate func)
    {
        if(func != null && dest != null && dest.Length > 0)
        {
            //delegateHandler[(int)type] += func;
            //if (!delegateHandler[(int)type].Contains(new Tuple<string, CmdDelegate>(dest, func)))
            if (!delegateHandler[(int)type].Contains(new KeyValuePair<string, CmdDelegate>(dest, func)))
            {
                //delegateHandler[(int)type].Add(new Tuple<string, CmdDelegate>(dest, func));
                delegateHandler[(int)type].Add(new KeyValuePair<string, CmdDelegate>(dest, func));
                //Debug.Log("Register <" + type.ToString() + "," + dest + "," + func.ToString() + "> total:" + delegateHandler[(int)type].Count);
            }
        }
    }
    public void RegisterForCmdAsync(RemoteCmdType type, string dest, CmdDelegate func)
    {
        if (func != null && dest != null && dest.Length > 0)
        {
            //delegateHandler[(int)type] += func;
            //if (!delegateHandler[(int)type].Contains(new Tuple<string, CmdDelegate>(dest, func)))
            if (!delegateHandlerAsync[(int)type].Contains(new KeyValuePair<string, CmdDelegate>(dest, func)))
            {
                //delegateHandler[(int)type].Add(new Tuple<string, CmdDelegate>(dest, func));
                delegateHandlerAsync[(int)type].Add(new KeyValuePair<string, CmdDelegate>(dest, func));
                //Debug.Log("Register <" + type.ToString() + "," + dest + "," + func.ToString() + "> total:" + delegateHandler[(int)type].Count);
            }
        }
    }

    public void UnregisterForCmd(RemoteCmdType type, string dest, CmdDelegate func)
    {
        if (func != null && dest != null && dest.Length > 0)
        {
            //delegateHandler[(int)type] += func;
            //if(delegateHandler[(int)type].Remove(new Tuple<string, CmdDelegate>(dest, func)))
            if (delegateHandler[(int)type].Remove(new KeyValuePair<string, CmdDelegate>(dest, func)))
            { 
                //Debug.Log("Unregister <" + type.ToString() + "," + dest + "," + func.ToString() + "> total:" + delegateHandler[(int)type].Count);
            }
        }
    }

    public void EnqueueCmd(string cmdstr)
    {
        lock (q_lock)
        {
            receivedCmds.Enqueue(cmdstr);
        }
    }

    public void OnRemoteCmdReceived(RemoteCmd cmd)
    {
        //Debug.Log("OnRemoteCmdReceived");
        //if(delegateHandler[(int)cmd.type]!=null)
        //    delegateHandler[(int)cmd.type].Invoke(cmd.dest, cmd.data);

        //foreach (Tuple<string, CmdDelegate> t in delegateHandler[(int)cmd.type])
        foreach (KeyValuePair<string, CmdDelegate> t in delegateHandler[(int)cmd.type])
        {
            //if (cmd.dest.Equals("all") || t.Item1.Equals(cmd.dest))
            if (cmd.dest.Equals("all") || t.Key.Equals(cmd.dest))
            {
                //t.Item2.Invoke(cmd.dest, cmd.data);
                t.Value.Invoke(cmd.dest, cmd.data);
            }
        }
    }
    public void OnRemoteCmdReceivedAsync(RemoteCmd cmd)
    {
        //Debug.Log("OnRemoteCmdReceived");
        //if(delegateHandler[(int)cmd.type]!=null)
        //    delegateHandler[(int)cmd.type].Invoke(cmd.dest, cmd.data);

        //foreach (Tuple<string, CmdDelegate> t in delegateHandler[(int)cmd.type])
        foreach (KeyValuePair<string, CmdDelegate> t in delegateHandlerAsync[(int)cmd.type])
        {
            //if (cmd.dest.Equals("all") || t.Item1.Equals(cmd.dest))
            if (cmd.dest.Equals("all") || t.Key.Equals(cmd.dest))
            {
                //t.Item2.Invoke(cmd.dest, cmd.data);
                t.Value.Invoke(cmd.dest, cmd.data);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        while(receivedCmds.Count > 0)
        {
            string cmdstr = "";
            lock (q_lock)
            {
                if (receivedCmds.Count > 0)
                {
                    cmdstr = receivedCmds.Dequeue();
                }
            }
            if(cmdstr.Length > 0)
            {
                RemoteCmd cmd = JsonUtility.FromJson<RemoteCmd>(cmdstr);
                OnRemoteCmdReceived(cmd);
            }
        }

        

        //SendRemoteCmd(RemoteCmdType.MoveRef, new HoloTransform());
        //Debug.Log("VOID SEND");
    }

    public static string GetCmdString(RemoteCmdType type, string dest, string data) {
        return UnityEngine.JsonUtility.ToJson(new RemoteCmd(type, dest, data));
    }


    /* BEGIN CMD CLIENT CLASS*/
    public class CmdClient : MonoBehaviour
    {

        public int port = 7779;

#if UNITY_WSA_10_0 && !UNITY_EDITOR
        DatagramSocket socket;
        IOutputStream outstream;
        //DataReader reader;
        DataWriter writer;
#else
        UdpClient udp;
#endif

        IPEndPoint ep;
        RemoteCmdHandler handler;

        // Use this for initialization
        public void Init(string ip, int port_, RemoteCmdHandler handler_)
        {
            this.port = port_;
            ep = new IPEndPoint(IPAddress.Parse(ip), port);
            this.handler = handler_;
#if UNITY_WSA_10_0 && !UNITY_EDITOR
            socket = new DatagramSocket();
            socket.MessageReceived += OnMgsReceived;
            socket.BindServiceNameAsync(port.ToString()).GetResults();
            //outstream = socket.GetOutputStreamAsync(new HostName(ep.Address.ToString()), port.ToString()).GetResults();
            //writer = new DataWriter(outstream);
#else
            udp = new UdpClient(port);
            //udp.BeginReceive(new AsyncCallback(receiveCmd), null);
            udp.BeginReceive(new AsyncCallback(receiveCmd), null);
#endif
        }


        public void sendCmd(RemoteCmdType type, string data)
        {
            sendCmd(port, type, data);
        }

        public void sendCmd(RemoteCmdType type, string dest, string data)
        {
            sendCmd(port, type, dest, data);
        }

        private void sendCmd(int port, RemoteCmdType type, string data)
        {
            sendCmd(port, type, "all", data);
        }
        private void sendCmd(int port, RemoteCmdType type, string dest, string data)
        {
#if UNITY_WSA_10_0 && !UNITY_EDITOR
            SendMsg(UnityEngine.JsonUtility.ToJson(new RemoteCmd(type, dest, data)));
        //SendMsg("123456789123456789123456789123456789000");
#else
            //udp.Send(new byte[] { 1, 2, 3, 4, 5 }, 5);
            string pktstr = GetCmdString(type, dest, data);
            udp.Send(Encoding.UTF8.GetBytes(pktstr), Encoding.UTF8.GetByteCount(pktstr), ep);
            //Debug.Log("SEND " + pktstr);
#endif
        }

        //void HandleCmd(string cmdstr)
        //{
        //    RemoteCmd cmd = JsonUtility.FromJson<RemoteCmd>(cmdstr);
        //    handler.OnRemoteCmdReceived(cmd);
        //}

#if UNITY_WSA_10_0  && !UNITY_EDITOR

        private async void SendMsg(string message)
        {
            var socket = new DatagramSocket();

            using (var stream = await socket.GetOutputStreamAsync(new HostName(ep.Address.ToString()), port.ToString()))
            {
                using (var writer = new DataWriter(stream))
                {
                    var data = Encoding.UTF8.GetBytes(message);

                    writer.WriteBytes(data);
                    writer.StoreAsync();
                    //Debug.Log("sent " + data.Length);
                }
            }
        }

        private async void OnMgsReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
//            Debug.Log("RECEIVED VOID");

            var result = args.GetDataStream();
            var resultStream = result.AsStreamForRead(1400);

            using (var reader = new StreamReader(resultStream))
            {
                var text = await reader.ReadToEndAsync();
                //var text = reader.ReadToEnd();

                handler.EnqueueCmd(text);

//                Debug.Log("MESSAGE: " + text);
            }
        }
#else

        public void receiveCmd(IAsyncResult result)
        {
            // while (!endReceive)
            {
                //Debug.Log("RECEIVING");

                //var remoteEP = new IPEndPoint(IPAddress.Any, port);
                //udp.BeginReceive
                //var data = udp.Receive(ref remoteEP); // listen on port
                //Debug.Log("receive data from " + remoteEP.ToString());

                IPEndPoint source = new IPEndPoint(0, 0);
                //byte[] message = udp.EndReceive(result, ref source);
                //Debug.Log("RECV " + Encoding.UTF8.GetString(message) + " from " + source);

                string message = Encoding.UTF8.GetString(udp.EndReceive(result, ref source));
                //Debug.Log("RECV " + message + " from " + source);
                //HandleCmd(message);

                handler.EnqueueCmd(message);

                // schedule the next receive operation once reading is done:
                udp.BeginReceive(new AsyncCallback(receiveCmd), udp);

            }
        }
        
#endif
        public void Stop()
        {
#if UNITY_WSA_10_0 && !UNITY_EDITOR

#else
            udp.Close();
#endif
        }
    }
}
