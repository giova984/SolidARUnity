//#if UNITY_UWP 
//#if !UNITY_EDITOR
//#if UNITY_METRO && !UNITY_EDITOR


//#if !UNITY_EDITOR
//#if !UNITY_STANDALONE
//#define HOLO_CHECK
//#endif
//#endif

//UNITY_WSA_10_0
//#define directive for Windows Store Apps when targeting Universal Windows 10 Apps. 
//Additionally WINDOWS_UWP and NETFX_CORE are defined when compiling C# files against .NET Core.

//#if UNITY_WSA_10_0
//#define HOLO_CHECK
//#endif



#if UNITY_WSA_10_0  && !UNITY_EDITOR
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#else
using System.Net.Sockets;
#endif

using System.Text;
using System.Net;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

#if UNITY_WSA_10_0
using UnityEngine.VR.WSA.Persistence;
#endif

using UnityEngine.VR.WSA;

public enum HoloType { Transform, CameraParams };

public enum HoloID { Hololens = 8, Projector1 = 0, Projector2 = 1, Projector3 = 2, Projector4 = 3, Projector5 = 4, Projector6 = 5, Projector7 = 6, Projector8 = 7 };

[Serializable]
public class HoloPacket
{
    public HoloType type;
    public HoloID id;
    public string data;
    public HoloPacket(HoloType type_, HoloID id_, string data_) { this.type = type_; this.id = id_; this.data = data_; }
}

[Serializable]
public class HoloTransform
{
    public Vector3 position;
    public Quaternion rotation;

    public HoloTransform() { position = new Vector3(); rotation = new Quaternion(); }
    public HoloTransform(Vector3 pos, Quaternion quat) { position = pos; rotation = quat; }
}

[Serializable]
public class HoloMatrices
{
    //V public Matrix4x4[] holoViewMatrices = new Matrix4x4[2];
    public Matrix4x4[] holoProjMatrices = new Matrix4x4[2];
    public float separation;
    public float convergence;
    public float aspect;
    public float fov;
    public float near;
    public float far;
    public bool isStereo;
    public int hres;
    public int vres;
    public HoloMatrices()
    {
        //V holoViewMatrices = new Matrix4x4[2]; 
        holoProjMatrices = new Matrix4x4[2];
    }
    public HoloMatrices(Matrix4x4[] views_, Matrix4x4[] projs_)
    {
        //V holoViewMatrices = views_; 
        holoProjMatrices = projs_;
    }
}

public class HoloSender : MonoBehaviour
{

    public string ip = "127.0.0.1";
    public int port = 7778;

    [Range(1,60)]
    public int fixedParamsRate = 1;

    private int fixedParamsRate_count = 60;

    IPEndPoint ep;
    GameObject ReferenceRoot = null;
    //GameObject ProjectorObj = null;
    List<GameObject> ProjectorObjs = new List<GameObject>();

#if UNITY_WSA_10_0 && !UNITY_EDITOR
    DatagramSocket socket;
    DataWriter dataWriter;
#else
    UdpClient udp;
#endif

    // Use this for initialization
    void Start()
    {
        ep = new IPEndPoint(IPAddress.Parse(ip), port); // endpoint where server is listening
                                                        //ep = new IPEndPoint(IPAddress.Parse("152.2.130.69"), 7778); // endpoint where server is listening
        ReferenceRoot = GameObject.Find("ReferenceRoot");
        //ProjectorObjs = GameObject.Find("ProjectorObj").gameObject.transform.FindChild("ProjectorMesh").gameObject;
        ProjectorCalibration[]  Scrips = FindObjectsOfType<ProjectorCalibration>();
        foreach ( ProjectorCalibration s in Scrips)
        {
            if (s.enabled && !ProjectorObjs.Contains(s.gameObject))
                ProjectorObjs.Add(s.gameObject);
        }
        Debug.Log("Found " + ProjectorObjs.Count + " Projectors");

#if UNITY_WSA_10_0 && !UNITY_EDITOR
        //socket = new DatagramSocket();

        ////socket.MessageReceived += SocketOnMessageReceived;

        //using (var stream = await socket.GetOutputStreamAsync(new HostName(ep.Address.ToString()), port.ToString()))
        //{
        //    using (var writer = new DataWriter(stream))
        //    {
#else
        udp = new UdpClient();
        //IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port); // endpoint where server is listening
        //udp.Connect(ep);
#endif
            }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("SENDING");
        //while (true)
        //SendMessage("123456789123456789123456789123456789000", port);

        if (ReferenceRoot)
        {
            HoloTransform ht = new HoloTransform(
                Quaternion.Inverse(ReferenceRoot.transform.rotation) * (transform.position - ReferenceRoot.transform.position),
                Quaternion.Inverse(ReferenceRoot.transform.rotation) * transform.rotation);
            SendHoloPacket(port, HoloType.Transform, HoloID.Hololens, UnityEngine.JsonUtility.ToJson(ht));
        }
        //else
        //{
        //    ht.rposition = new Vector3(1, 1, 1);
        //    ht.rrotation = Quaternion.Euler(100, 100, 100);
        //}



        //if (ProjectorObj)
        //{
        //    HoloTransform ht = new HoloTransform(
        //        Quaternion.Inverse(ReferenceRoot.transform.rotation) * (ProjectorObj.transform.position - ReferenceRoot.transform.position),
        //        Quaternion.Inverse(ReferenceRoot.transform.rotation) * ProjectorObj.transform.rotation);
        //    SendHoloPacket(port, HoloType.ProjectorTransform, UnityEngine.JsonUtility.ToJson(ht));
        //}
        //else
        //{
        //    HoloTransform ht = new HoloTransform(new Vector3(1, 1, 1), Quaternion.Euler(100, 100, 100));
        //    SendHoloPacket(port, HoloType.ProjectorTransform, UnityEngine.JsonUtility.ToJson(ht));
        //    Debug.LogError("Proj obj not found");
        //}
        foreach(GameObject p in ProjectorObjs)
        {
            HoloTransform ht = new HoloTransform(
                Quaternion.Inverse(ReferenceRoot.transform.rotation) * (p.transform.position - ReferenceRoot.transform.position),
                Quaternion.Inverse(ReferenceRoot.transform.rotation) * p.transform.rotation);
            //Debug.Log("Send Proj " + p.GetComponent<ProjectorCalibration>().ProjectorID.ToString() + (int)p.GetComponent<ProjectorCalibration>().ProjectorID);
            SendHoloPacket(port, HoloType.Transform, p.GetComponent<ProjectorCalibration>().ProjectorID , UnityEngine.JsonUtility.ToJson(ht));
        }


        if (fixedParamsRate_count >= 60)
        {
            //retrieving camera prameters
            HoloMatrices hm = new HoloMatrices();
            hm.holoProjMatrices[0] = Camera.main.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            hm.holoProjMatrices[1] = Camera.main.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
            //hm.holoViewMatrices[0] = Camera.main.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
            //hm.holoViewMatrices[1] = Camera.main.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
            hm.convergence = Camera.main.stereoConvergence;
            hm.aspect = Camera.main.aspect;
            hm.fov = Camera.main.fieldOfView;
            hm.near = Camera.main.nearClipPlane;
            hm.far = Camera.main.farClipPlane;
            hm.isStereo = Camera.main.stereoEnabled;
            hm.vres = Camera.main.pixelHeight;
            hm.hres = Camera.main.pixelWidth;
            SendHoloPacket(port, HoloType.CameraParams, HoloID.Hololens, UnityEngine.JsonUtility.ToJson(hm));
            fixedParamsRate_count = 0;
        }
        fixedParamsRate_count += fixedParamsRate;
    }

    public void SendHoloPacket(int port, HoloType type, HoloID id, string data)
    {
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        SendMessage(UnityEngine.JsonUtility.ToJson(new HoloPacket(type, id, data)), port);
        //SendMessage("123456789123456789123456789123456789000", port);
#else
        //udp.Send(new byte[] { 1, 2, 3, 4, 5 }, 5);
        string pktstr = UnityEngine.JsonUtility.ToJson(new HoloPacket(type, id, data));
        udp.Send(Encoding.UTF8.GetBytes(pktstr), Encoding.UTF8.GetByteCount(pktstr), ep);
#endif
    }

#if UNITY_WSA_10_0 && !UNITY_EDITOR


    private async void SendMessage(string message, int port)
    {
        var socket = new DatagramSocket();

        //socket.MessageReceived += SocketOnMessageReceived;

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

    private async void SocketOnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
    {
        var result = args.GetDataStream();
        var resultStream = result.AsStreamForRead(1024);

        using (var reader = new StreamReader(resultStream))
        {
            var text = await reader.ReadToEndAsync();
            //Debug.Log("MESSAGE: " + text);
        }
    }
#endif

}