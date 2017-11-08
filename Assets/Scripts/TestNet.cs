using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNet : MonoBehaviour {
    float defertime = 0;
	// Use this for initialization
	void Start () {
#if UNITY_WSA_10_0 && !UNITY_EDITOR
#else
        RemoteCmdHandler.Instance.RegisterForCmd(RemoteCmdType.SyncFrame, this.name, recv);
#endif   
    }
	
    public void recv(string dest, string data)
    {
        Debug.Log("Server recv " + data.Length);
    }
	// Update is called once per frame
	void Update () {
        if (defertime == 1)
        {

        }
        else
        {
            SendCmd();
        }
    }
    void SendCmd()
    {
        defertime = 1;
        Invoke("Send", 1);
    }
    void Send()
    {
#if UNITY_WSA_10_0 && !UNITY_EDITOR
          RemoteCmdHandler.Instance.SendRemoteCmd(RemoteCmdType.SyncFrame, "all", "testholo");
#else
          RemoteCmdHandler.Instance.SendRemoteCmd(RemoteCmdType.SyncFrame, "all", "testpc");
#endif
        defertime = 0;
    }
}
