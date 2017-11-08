using UnityEngine;
using System.Collections;
using UnityEngine.VR;

public class DeviceDependant : MonoBehaviour {
    public bool PCEnabled = false;
    public bool HoloEnabled = false;

    // Use this for initialization
    void Awake () {
        bool active = false;
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        if ((HoloEnabled && VRDevice.isPresent && VRSettings.loadedDeviceName.Equals("HoloLens")))
            active = true;
        //Debug.Log("Branch WSA_10 " + HoloEnabled.ToString() + VRDevice.isPresent.ToString() + VRSettings.loadedDeviceName);
#else
        //Debug.Log("Branch NO_WSA_10 ");
#endif
        if (PCEnabled && (!VRDevice.isPresent || VRSettings.loadedDeviceName.Equals("stereo")))
            active = true;

        //Debug.Log("VRDevice: " + ((VRDevice.isPresent) ? VRSettings.loadedDeviceName : "non present"));

        this.gameObject.SetActive(active);
        Debug.Log("GameObject \"" + this.gameObject.name + "\" " + ((active) ? "enabled" : "disabled"));

        //Debug.Log("DevDep: " + gameObject.name + " " + active.ToString());
    }
}
