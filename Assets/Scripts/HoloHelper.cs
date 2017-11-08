using UnityEngine;
using System.Collections;
using UnityEngine.VR;

public static class HoloHelper
{
    public static bool isHololens()
    {
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        return (VRDevice.isPresent && VRSettings.loadedDeviceName.Equals("HoloLens"));
#endif
        return false;
    }
}