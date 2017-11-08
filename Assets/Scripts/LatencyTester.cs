using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class LatencyTester : MonoBehaviour {
    private int count = 0;
    private Hashtable timeStamps = new Hashtable();
    // Use this for initialization

    Stopwatch sw;
	void Start () {
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        RemoteCmdHandler.Instance.RegisterForCmdAsync(RemoteCmdType.LatencyTest, this.name, receiveClient);
#else
        sw = Stopwatch.StartNew();
        RemoteCmdHandler.Instance.RegisterForCmdAsync(RemoteCmdType.LatencyTest, this.name, receiveServer);
#endif
    }

    void receiveClient(string det, string data)
    {
        RemoteCmdHandler.Instance.SendRemoteCmd(RemoteCmdType.LatencyTest, det, data, false);
    }
    void receiveServer(string det, string data)
    {
        float latency = (float)sw.ElapsedMilliseconds - (float)timeStamps[int.Parse(data)];
        UnityEngine.Debug.Log("Latency " + int.Parse(data) + " - " + latency);
    }
    // Update is called once per frame
    void Update () {
#if UNITY_WSA_10_0 && !UNITY_EDITOR
#else
        RemoteCmdHandler.Instance.SendRemoteCmd(RemoteCmdType.LatencyTest, this.name, count.ToString(),false);
        if (timeStamps.ContainsKey(count))
        {
            timeStamps[count] = (float)sw.ElapsedMilliseconds;
        }
        else
        {
            timeStamps.Add(count, (float)sw.ElapsedMilliseconds);
        }
        count = (count + 1) % 100;

#endif
    }
}
