using UnityEngine;
using System.Collections;



public enum CalibrationID
{
    Reference = 9,
    Projector1 = 0,
    Projector2 = 1,
    Projector3 = 2,
    Projector4 = 3,
    Projector5 = 4,
    Projector6 = 5,
    Projector7 = 6,
    Projector8 = 7,
    None = 8
}

public class PC_InputHandler : MonoBehaviour
{

    public bool enable_AR_shadows = true;
    public bool enable_Occlusion_shadows = true;

    public float calibStepMin = 0.001f;
    public float calibStepMax = 0.01f;
    private float calibPressTime = 0;

    public bool showCalibration = false;

    RemoteCmdHandler cmdHandler = null;
    public CalibrationID calibratingID = CalibrationID.None;

    bool showHiRes = false;
    bool showLowRes = false;

    // Use this for initialization
    void Start()
    {
        UpdateShadowsState();
        cmdHandler = GetComponent<RemoteCmdHandler>();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.C))
        {
            showCalibration = !showCalibration;
            if (showCalibration)
            {
                BroadcastMessage("OnShowRoomCalibration");
                MaterialSwitcher[] mss = GameObject.FindObjectsOfType<MaterialSwitcher>();
                foreach (MaterialSwitcher ms in mss)
                {
                    ms.SendMessage("OnUseIdleMaterial");
                }
            }
            else
            {
                BroadcastMessage("OnHideRoomCalibration");
                MaterialSwitcher[] mss = GameObject.FindObjectsOfType<MaterialSwitcher>();
                foreach (MaterialSwitcher ms in mss)
                {
                    ms.SendMessage("OnUsePlayMaterial");
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            enable_Occlusion_shadows = !enable_Occlusion_shadows;
            //Debug.Log("S pressed");
            UpdateShadowsState();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            enable_AR_shadows = !enable_AR_shadows;
            //Debug.Log("S pressed");
            UpdateShadowsState();
        }

        if (cmdHandler)
        {
            //foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
            //{
            //    if (Input.GetKey(vKey))
            //    {
            //        //your code here

            //    }
            //}

            CalibrationID prev_calibratingID = calibratingID;
            if(Input.GetKeyUp(KeyCode.Alpha9))
            {
                calibratingID = CalibrationID.None;
            }
            else if (Input.GetKeyUp(KeyCode.Alpha0))
            {
                calibratingID = CalibrationID.Reference;
            }
            else if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                calibratingID = CalibrationID.Projector1;
            }
            else if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                calibratingID = CalibrationID.Projector2;
            }
            else if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                calibratingID = CalibrationID.Projector3;
            }
            else if (Input.GetKeyUp(KeyCode.Alpha4))
            {
                calibratingID = CalibrationID.Projector4;
            }
            else if (Input.GetKeyUp(KeyCode.Alpha5))
            {
                calibratingID = CalibrationID.Projector5;
            }
            else if (Input.GetKeyUp(KeyCode.Alpha6))
            {
                calibratingID = CalibrationID.Projector6;
            }
            else if (Input.GetKeyUp(KeyCode.Alpha7))
            {
                calibratingID = CalibrationID.Projector7;
            }
            else if (Input.GetKeyUp(KeyCode.Alpha8))
            {
                calibratingID = CalibrationID.Projector8;
            }

            if (calibratingID != prev_calibratingID)
                Debug.Log("Calibrating " + calibratingID.ToString());

            if (calibratingID != CalibrationID.None)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow)
                    || Input.GetKeyDown(KeyCode.DownArrow)
                    || Input.GetKeyDown(KeyCode.RightArrow)
                    || Input.GetKeyDown(KeyCode.LeftArrow)
                    || Input.GetKeyDown(KeyCode.PageUp)
                    || Input.GetKeyDown(KeyCode.PageDown))
                {
                    calibPressTime = Time.time;
                }

                float calibStep = Mathf.Lerp(calibStepMin, calibStepMax, (Time.time - calibPressTime) / 5.0f);
                Vector3 offset = new Vector3(0,0,0);

                if (Input.GetKey(KeyCode.UpArrow)) //X axis
                {
                    offset.z = calibStep;
                    //Debug.Log("Up pressed");
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    offset.z = -calibStep;
                    //Debug.Log("Down pressed");
                }
                if (Input.GetKey(KeyCode.RightArrow)) //Z axis
                {
                    offset.x = calibStep; 
                    //Debug.Log("Right pressed");
                }
                else if (Input.GetKey(KeyCode.LeftArrow))
                {
                    offset.x = -calibStep; 
                    //Debug.Log("Left pressed");
                }
                if (Input.GetKey(KeyCode.PageUp)) //Y axis
                {
                    offset.y = calibStep; 
                    //Debug.Log("PgUp pressed");
                }
                else if (Input.GetKey(KeyCode.PageDown))
                {
                    offset.y = -calibStep; 
                    //Debug.Log("PgDown pressed");
                }

                if (offset.magnitude > 0)
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) //Rotation if Shift pressed
                    {
                        float angle_factor = 100.0f;
                        cmdHandler.SendRemoteCmd(RemoteCmdType.MoveRef, calibratingID.ToString(), new HoloTransform(new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(offset.z, offset.y, -offset.x) * angle_factor)));
                    }
                    else //Translation
                    {
                        cmdHandler.SendRemoteCmd(RemoteCmdType.MoveRef, calibratingID.ToString(), new HoloTransform(offset, Quaternion.Euler(0,0,0)));
                    }
                }
                
            }
        }

        if (Input.GetKeyDown(KeyCode.V))//restart video
        {
            //Debug.Log("PCIH v press");
            cmdHandler.SendRemoteCmd(RemoteCmdType.SyncFrame, "all", "0", true);
            //CustomMeshPlayer[] players = FindObjectsOfType<CustomMeshPlayer>();
            //foreach (CustomMeshPlayer player in players)
            //{
            //    player.SyncFrame(0);
            //}
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            showHiRes = !showHiRes;
            //Debug.Log("PCIH v press");
            cmdHandler.SendRemoteCmd(RemoteCmdType.EnableObj, "hires", showHiRes.ToString(), true);
            //RemoteEnabler[] players = FindObjectsOfType<RemoteEnabler>();
            //foreach (RemoteEnabler player in players)
            //{
            //    player.EnableThis("hires", showHiRes.ToString());
            //}
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            showLowRes = !showLowRes;
            //Debug.Log("PCIH v press");
            cmdHandler.SendRemoteCmd(RemoteCmdType.EnableObj, "lowres", showLowRes.ToString(), true);
            //RemoteEnabler[] players = FindObjectsOfType<RemoteEnabler>();
            //foreach (RemoteEnabler player in players)
            //{
            //    player.EnableThis("lowres", showLowRes.ToString());
            //}
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        //if (Input.GetKeyDown(KeyCode.Alpha0))
        //{
        //    Debug.Log("Force Eye No");
        //    Shader.SetGlobalInt("force_eye", 0);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    Debug.Log("Force Eye Left");
        //    Shader.SetGlobalInt("force_eye", 1);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    Debug.Log("Force Eye Right");
        //    Shader.SetGlobalInt("force_eye", 2);
        //}

    }

    void UpdateShadowsState()
    {
        Debug.Log("Shadows Occlusion:" + (enable_Occlusion_shadows ? "enabled" : "disabled") + " AR:" + (enable_AR_shadows ? "enabled" : "disabled"));
        Shader.SetGlobalInt("_enableARShadows", enable_AR_shadows ? 1 : 0);
        Shader.SetGlobalInt("_enableOcclusionShadows", enable_Occlusion_shadows ? 1 : 0);
    }
}
