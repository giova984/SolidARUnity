//#if !UNITY_EDITOR
//#if !UNITY_STANDALONE
//#define HOLO_CHECK
//#endif
//#endif

#if UNITY_WSA_10_0 && !UNITY_EDITOR
using Windows.Networking.Sockets;
#else
using System.Net.Sockets;
using System.Net;
#endif

using UnityEngine;
using System.Collections;
using System;
using System.Threading;

using System.Text;

public class HoloReceiver : MonoBehaviour
{
    public int port = 7778;
    public bool follow = false;
    public HoloID id = HoloID.Hololens;

    [Range(0f, 60.0f)]
    public float output_rate;// = 0.2f;

    private static Hashtable ht = new Hashtable();

    private Camera holoL;
    private Camera holoR;

    private HoloClient Instance { get; set; }

    void Awake()
    {
        if (HoloHelper.isHololens())
        {
            this.enabled = false;
            //Debug.Log("HoloReceiver disabed");
        }
    }

    void Start()
    {
        if (ht.ContainsKey(port))
        {
            //Debug.Log("HoloClient Instance Found");
            this.Instance = (HoloClient)ht[port];
        }
        else
        {
            //HoloClient hc = new HoloClient(port);
            this.Instance = this.gameObject.AddComponent<HoloClient>();
            this.Instance.Init(port, output_rate);
            ht[port] = this.Instance;
            //Debug.Log("HoloClient Instance " + this.Instance);
        }

        Transform hL = transform.FindChild("HoloCameraL");
        Transform hR = transform.FindChild("HoloCameraR");
        if(hL) holoL = hL.gameObject.GetComponent<Camera>();
        if(hR) holoR = hR.gameObject.GetComponent<Camera>();
    }

    void OnApplicationQuit()
    {
        if(this.Instance)
            this.Instance.Stop();
    }

    void Update()
    {
        if (follow && id == HoloID.Hololens)
        {
            if(Instance == null)
            {
                Debug.LogError("NULL holoClient");
                return;
            }

            if (Instance.bHT)
            {
                this.transform.position = Instance.lastHoloTransform.position;
                this.transform.rotation = Instance.lastHoloTransform.rotation;
            }
            //Debug.Log("Main camera: " + Camera.main.name);

            if (Instance.bHM)
            {
                if (holoL.stereoEnabled)
                {
                    int idx = 0; //0
                    holoL.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, Instance.lastHoloMatrices.holoProjMatrices[idx]);
                    holoL.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, Instance.lastHoloMatrices.holoProjMatrices[idx]);
                    Debug.LogError("HoloL Camera is stereo");
                }
                if (holoR.stereoEnabled)
                {
                    int idx = 1; //1
                    holoR.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, Instance.lastHoloMatrices.holoProjMatrices[idx]);
                    holoR.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, Instance.lastHoloMatrices.holoProjMatrices[idx]);
                    Debug.LogError("HoloR Camera is stereo");
                }

                holoL.projectionMatrix = Instance.lastHoloMatrices.holoProjMatrices[0]; //0
                holoR.projectionMatrix = Instance.lastHoloMatrices.holoProjMatrices[1]; //1
            }

            //Camera.main.worldToCameraMatrix = Instance.lastHoloMatrices.holoViewMatrices[1];

            //Camera.main.aspect = Instance.lastHoloMatrices.aspect;
            //Camera.main.fieldOfView = Instance.lastHoloMatrices.fov;
            //Camera.main.stereoSeparation = Instance.lastHoloMatrices.separation;
            //Camera.main.stereoConvergence = Instance.lastHoloMatrices.convergence;
            ////Camera.main.nearClipPlane = Instance.lastHoloMatrices.near;
            ////Camera.main.nearClipPlane = 0.01f;
            ////Camera.main.farClipPlane = Instance.lastHoloMatrices.far;
            ////Camera.main.farClipPlane = 20.0f;

            //this.transform.position = Quaternion.Inverse(lastHoloTransform.rrotation) * (lastHoloTransform.position - lastHoloTransform.rposition);
            //this.transform.rotation = Quaternion.Inverse(lastHoloTransform.rrotation) * lastHoloTransform.rotation;
        }
        else if (follow && id >= HoloID.Projector1 && id <= HoloID.Projector8)
        {
            if (Instance == null)
            {
                Debug.LogError("NULL holoClient");
                return;
            }

            //Debug.Log("FollowProj " + Instance.bPT);
            if (Instance.bPT)
            {
                this.transform.position = Instance.getlastProjTransform((int)id).position;
                this.transform.rotation = Instance.getlastProjTransform((int)id).rotation;
            }

            //this.transform.position = Quaternion.Inverse(lastHoloTransform.rrotation) * (lastHoloTransform.position - lastHoloTransform.rposition);
            //this.transform.rotation = Quaternion.Inverse(lastHoloTransform.rrotation) * lastHoloTransform.rotation;
        }
    }
    class HoloClient : MonoBehaviour
    {



        private int port = 7778;
        private float output_rate = 0;

        private HoloTransform _lastHoloTransform = new HoloTransform();
        private HoloMatrices _lastHoloMatrices = new HoloMatrices();
        private HoloTransform[] _lastProjTransform = new HoloTransform[8];

        public bool bHT = false;
        public bool bHM = false;
        public bool bPT = false;

        private object _lock = new object();

        public HoloTransform lastHoloTransform
        {
            get
            {
                lock (_lock)
                {
                    return _lastHoloTransform;
                }
            }
            private set
            {
                lock (_lock)
                {
                    _lastHoloTransform = value;
                }
            }
        }

        public HoloMatrices lastHoloMatrices
        {
            get
            {
                lock (_lock)
                {
                    return _lastHoloMatrices;
                }
            }
            private set
            {
                lock (_lock)
                {
                    _lastHoloMatrices = value;
                }
            }
        }

        //public HoloTransform lastProjTransform
        //{
        //    get
        //    {
        //        lock (_lock)
        //        {
        //            return _lastProjTransform;
        //        }
        //    }
        //    private set
        //    {
        //        lock (_lock)
        //        {
        //            _lastProjTransform = value;
        //        }
        //    }
        //}

        public HoloTransform getlastProjTransform(int proj)
        {
            lock (_lock)
            {
                return _lastProjTransform[proj];
            }
        }

        public void setlastProjTransform(int proj, HoloTransform t)
        {
            lock (_lock)
            {
                _lastProjTransform[proj] = t;
            }
        }

#if UNITY_WSA_10_0  && !UNITY_EDITOR 

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

        public void Init(int port_)
        {
            this.port = port_;
#if UNITY_WSA_10_0  && !UNITY_EDITOR

#else
            udp = new UdpClient(port);
            //StartCoroutine("receiveMsg");
            //_recvT = new Thread(receiveMsg);
            //_recvT.Start();
            udp.BeginReceive(new AsyncCallback(receiveMsg), null);
#endif
        }

        public void Init(int port_, float output_rate_)
        {
            this.output_rate = output_rate_;
            this.Init(port_);
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
                    Debug.Log("Net SPS: " + count_sps / sps_time);// + " - " + output_rate);
                    sps_time = 0;
                    count_sps = 0;
                    //Debug.Log("Hp " + lastHoloTransform.position + " Hr " + lastHoloTransform.rotation.eulerAngles);
                    //Debug.Log("Rp " + lastHoloTransform.rposition + " Rr " + lastHoloTransform.rrotation.eulerAngles);
                    //Debug.Log("Ap " + lastHoloTransform.aposition + " Ar " + lastHoloTransform.arotation.eulerAngles);
                    //Debug.Log("Pp " + lastHoloTransform.pposition + " Pr " + lastHoloTransform.protation.eulerAngles);
                    //for(int i=0; i<lastHoloTransform.holoViewMatrices.Length; ++i)
                    //for (int i = 0; i < 2; ++i)
                    //{
                    //    //V Debug.Log("HVM_" + i + ": " + lastHoloMatrices.holoViewMatrices[i].ToString());
                    //    Debug.Log("HPM_" + i + ": " + lastHoloMatrices.holoProjMatrices[i].ToString());
                    //}
                    //Debug.Log("isStereo " + (lastHoloMatrices.isStereo ? "true":"false"));
                    //Debug.Log("Separation " + lastHoloMatrices.separation);
                    //Debug.Log("Convergence " + lastHoloMatrices.convergence);
                    //Debug.Log("Near " + lastHoloMatrices.near);
                    //Debug.Log("Far " + lastHoloMatrices.far);
                    //Debug.Log("Fov " + lastHoloMatrices.fov);
                    //Debug.Log("aspect " + lastHoloMatrices.aspect);

                }
            }

        }


        void receiveMsg(IAsyncResult result)
        {
            // while (!endReceive)
            {
                //Debug.Log("RECEIVING");

#if UNITY_WSA_10_0  && !UNITY_EDITOR

#else
                //var remoteEP = new IPEndPoint(IPAddress.Any, port);
                //udp.BeginReceive
                //var data = udp.Receive(ref remoteEP); // listen on port
                //Debug.Log("receive data from " + remoteEP.ToString());

                IPEndPoint source = new IPEndPoint(0, 0);
                //byte[] message = udp.EndReceive(result, ref source);
                //Debug.Log("RECV " + Encoding.UTF8.GetString(message) + " from " + source);

                string message = Encoding.UTF8.GetString(udp.EndReceive(result, ref source));
                //Debug.Log("RECV " + message + " from " + source);
                HoloPacket hp = JsonUtility.FromJson<HoloPacket>(message);
                switch (hp.type)
                {
                    case HoloType.Transform:
                        {

                            switch (hp.id)
                            {
                                case HoloID.Hololens:
                                    lastHoloTransform = JsonUtility.FromJson<HoloTransform>(hp.data);
                                    //Debug.Log("Recv Holo " + lastHoloTransform.position.ToString() + " : " + lastHoloTransform.rotation.eulerAngles.ToString() + " from " + source);
                                    bHT = true;
                                    break;
                                case HoloID.Projector1:
                                case HoloID.Projector2:
                                case HoloID.Projector3:
                                case HoloID.Projector4:
                                case HoloID.Projector5:
                                case HoloID.Projector6:
                                case HoloID.Projector7:
                                case HoloID.Projector8:
                                    setlastProjTransform((int) hp.id, JsonUtility.FromJson<HoloTransform>(hp.data));
                                    //Debug.Log("Recv " + hp.id .ToString() + getlastProjTransform((int)hp.id).position.ToString() + " : " + getlastProjTransform((int)hp.id).rotation.eulerAngles.ToString() + " from " + source);
                                    bPT = true;
                                    break;

                            }
                            break;
                        }
                    case HoloType.CameraParams:
                        {
                            lastHoloMatrices = JsonUtility.FromJson<HoloMatrices>(hp.data);
                            bHM = true;
                            break;
                        }
                }
                //Debug.Log("HOLO " + ht.position.ToString() + " : " + ht.rotation.eulerAngles.ToString() + " from " + source);

                ++count_sps;

                // schedule the next receive operation once reading is done:
                udp.BeginReceive(new AsyncCallback(receiveMsg), udp);

#endif
            }
        }

        public void Stop()
        {
#if UNITY_WSA_10_0 && !UNITY_EDITOR

#else
            if(udp != null)
                udp.Close();
#endif
        }
    }
}
