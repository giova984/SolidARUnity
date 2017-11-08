using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.IO;

#if UNITY_WSA_10_0 && !UNITY_EDITOR
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#else
using System.Net.Sockets;
using System.Net;

#endif
public class DigitalBoard : MonoBehaviour {

    private int port = 7882;
    public Material material;
    public float line_width = 0.002f;

    List<LineRenderer> renderers = new List<LineRenderer>();

    List<List<Vector3>> lines = new List<List<Vector3>>();
    private object q_lock = new object();
    Queue<byte[]> receivedPkts = new Queue<byte[]>();

    //UdpClient udp;
    UdpReceiver receiver;

    public void EnqueuePkt(byte[] pkt)
    {
        lock (q_lock)
        {
            receivedPkts.Enqueue(pkt);
        }
    }

    public void consumePkt(byte[] msg)
    {
        // while (!endReceive)
        {
            //Debug.Log("RECEIVING");

            //Debug.Log("RECV1 " + msg.Length);
            if (msg.Length >= 16) {
                int line_id = FromByteArray<int>(msg, 0, 4, 1)[0];
                float[] pos = FromByteArray< float>(msg, 4, 4, 3);
                //Debug.Log("RECV " + msg.Length + " " + line_id + " " + pos[0] + " " + pos[1] + " " + pos[2]);
                while (line_id > lines.Count-1)
                {
                    lines.Add(new List<Vector3>());
                    //renderer.numPositions = 0;
                    AddLineRenderer();
                }
                Vector3 p = new Vector3(-pos[0], pos[1], 0.001f);// pos[2]);
                lines[line_id].Add(p);

                //if (renderer != null)
                //{
                //    //renderer.SetPositions(lines[line_id].ToArray());
                //    renderer.numPositions = lines[line_id].Count;
                //    renderer.SetPosition(lines[line_id].Count - 1, p);
                //}
                if (renderers[line_id] != null)
                {
                    //renderer.SetPositions(lines[line_id].ToArray());
                    renderers[line_id].numPositions = lines[line_id].Count;
                    renderers[line_id].SetPosition(lines[line_id].Count - 1, p);
                }
            }
        }
    }

    void AddLineRenderer()
    {
        GameObject line = new GameObject();
        line.transform.parent = this.transform;
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.startWidth = line_width;
        lr.endWidth = line_width;
        lr.material = this.material;
        lr.useWorldSpace = false;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.numPositions = 0;
        line.transform.localPosition = new Vector3(0, 0, 0);
        line.transform.localRotation = new Quaternion();
        line.transform.localScale = new Vector3(1, 1, 1);
        renderers.Add(lr);
    }

    // Use this for initialization
    void Start()
    {
        receiver = this.gameObject.AddComponent<UdpReceiver>();
        receiver.Init(this.port, EnqueuePkt, 0.0f);
        //renderer = gameObject.GetComponent<LineRenderer>();
        //renderer.useWorldSpace = false;
    }

    // Update is called once per frame
    void Update() {

        while (receivedPkts.Count > 0)
        {
            string cmdstr = "";
            lock (q_lock)
            {
                if (receivedPkts.Count > 0)
                {
                    consumePkt(receivedPkts.Dequeue());
                }
            }
        }

        //int line_id = 0;
        //float[] pos = { UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f) };
        ////Debug.Log(" " + pos[0] + " " + pos[1] + " " + pos[2]);
        //while (line_id > lines.Count -1)
        //{
        //    lines.Add(new List<Vector3>());
        //}
        //Vector3 p = new Vector3(pos[0], pos[1], pos[2]);
        //lines[line_id].Add(p);
        //if (renderer != null)
        //{
        //    renderer.numPositions = lines[line_id].Count;
        //    //     renderer.SetVertexCount(lines[line_id].Count);
        //    //renderer.SetPositions(lines[line_id].ToArray());
        //        renderer.SetPosition(lines[line_id].Count - 1, p);
        //    renderer.SetPosition(0, new Vector3());
        //    renderer.SetPosition(1, p);
        //}
    }

    void OnApplicationQuit()
    {
        //Debug.Log("OnApplicationQuit");
        receiver.Stop();
    }
    void OnDestroy()
    {
     
    }

    public void Stop()
    {
#if UNITY_WSA_10_0 && !UNITY_EDITOR

#else
        //if (udp != null)
        //    udp.Close();
        receiver.Stop();
#endif
    }

    private static T[] FromByteArray<T>(byte[] source, int startIndex, int size, int count) where T : struct
    {
        //T[] destination = new T[source.Length / Marshal.SizeOf(typeof(T))];
        T[] destination = new T[count];
        GCHandle handle = GCHandle.Alloc(destination, GCHandleType.Pinned);
        try
        {
            IntPtr pointer = handle.AddrOfPinnedObject();
            Marshal.Copy(source, startIndex, pointer, size * count);
            return destination;
        }
        finally
        {
            if (handle.IsAllocated)
                handle.Free();
        }
    }
}


class UdpReceiver : MonoBehaviour
{
    private int port;
    private float output_rate = 0;

    public delegate void ReceiveDelegate(byte[] data_);
    //CmdDelegate[] delegateHandler = new CmdDelegate[System.Enum.GetValues(typeof(RemoteCmdType)).Length];
    //List<Tuple<string, CmdDelegate>>[] delegateHandler = new List<Tuple<string, CmdDelegate>>[System.Enum.GetValues(typeof(RemoteCmdType)).Length];
    //List<KeyValuePair<string, ReceiveDelegate>> delegateHandler = new List<KeyValuePair<string, ReceiveDelegate>>();
    ReceiveDelegate delegateHandler;

#if UNITY_WSA_10_0 && !UNITY_EDITOR
    DatagramSocket socket;
    IOutputStream outstream;
#else
    UdpClient udp;
#endif
    //bool endReceive = false;
    //Thread _recvT;

    float sps_time = 0;
    int count_sps = 0;

    //public HoloClient(int port_)
    //{
    //    this.port = port_;
    //}

    public void Init(int port_, ReceiveDelegate callb)
    {
        this.port = port_;
        this.delegateHandler = callb;
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        socket = new DatagramSocket();
        socket.MessageReceived += SocketOnMessageReceived;
        socket.BindServiceNameAsync(port.ToString()).GetResults();
#else
        udp = new UdpClient(port);
        //StartCoroutine("receiveMsg");
        //_recvT = new Thread(receiveMsg);
        //_recvT.Start();
        udp.BeginReceive(new AsyncCallback(receiveMsg), null);
#endif
    }

    public void Init(int port_, ReceiveDelegate callb, float output_rate_)
    {
        this.output_rate = output_rate_;
        this.Init(port_, callb);
    }

    // Use this for initialization
    void Start()
    {
        sps_time = 0;
        count_sps = 0;
    }



    // Update is called once per frame
    void Update()
    {
        if (output_rate > 0)
        {
            sps_time += Time.deltaTime;
            if (sps_time >= 1.0f / output_rate)
            {
                Debug.Log("Udp Net SPS: " + count_sps / sps_time);// + " - " + output_rate);
                sps_time = 0;
                count_sps = 0;
            }
        }

    }




#if UNITY_WSA_10_0 && !UNITY_EDITOR
    private async void SocketOnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
    {
        //            Debug.Log("RECEIVED VOID");

        var result = args.GetDataStream();
        var resultStream = result.AsStreamForRead(1400);

        using (var reader = new BinaryReader(resultStream))
        {
            byte[] bytes = reader.ReadBytes(1400);
            //var text = reader.ReadToEnd();

            delegateHandler.Invoke(bytes);

            ++count_sps;

            //                Debug.Log("MESSAGE: " + text);
        }
    }
#else
        void receiveMsg(IAsyncResult result)
    {
        // while (!endReceive)
        {
            //Debug.Log("RECEIVING");

            IPEndPoint source = new IPEndPoint(0, 0);

            byte[] data = udp.EndReceive(result, ref source);
            //string message = System.Text.Encoding.UTF8.GetString(data);
            //Debug.Log("RECV " + message + " from " + source);
            //HoloPacket hp = JsonUtility.FromJson<HoloPacket>(message);

            delegateHandler.Invoke(data);

            ++count_sps;

            // schedule the next receive operation once reading is done:
            udp.BeginReceive(new AsyncCallback(this.receiveMsg), this.udp);

        }
    }
#endif

    public void Stop()
    {
#if UNITY_WSA_10_0 && !UNITY_EDITOR

#else
        if (udp != null)
            udp.Close();
#endif
    }
}
