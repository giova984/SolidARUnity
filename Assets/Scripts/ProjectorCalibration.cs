using UnityEngine;
using System.Collections;
using System.IO;
#if UNITY_WSA_10_0 && !UNITY_EDITOR
using UnityEngine.VR.WSA.Persistence;
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Input;
#endif


public class ProjectorCalibration : MonoBehaviour
{

    static ProjectorCalibration[] Instances = { null, null, null, null, null, null, null, null };
    public static ProjectorCalibration GetInstance(int projID)
    {
        return Instances[projID];
    }

    public HoloID ProjectorID = HoloID.Projector1;

    //public bool useReferenceAnchor = true;
    public bool showCalibration = false;
    public bool showSceneDuringCalibration = true;

    public Vector3 calibrationPosition = new Vector3(0, -0.2f, 1.0f);
    public Vector3 calibrationRotation = new Vector3(-20.0f, 0, 0);


    bool calibrating = false;

    bool calibratingFine = false;
    float lastFineCalibTime = .0f;

    AudioSource audioSource = null;
    AudioClip startClip = null;
    AudioClip stopClip = null;

    GameObject CalibrationObject = null;
    GameObject CalibrationMeshObj = null;

    GameObject SceneObject = null;
#if UNITY_WSA_10_0 && !UNITY_EDITOR
    GestureRecognizer recognizer;
#endif
    void Awake()
    {
        if (Instances[(int)ProjectorID] != null)
            Debug.Log("Warning: multiple projector calibration instances with ID " + ProjectorID);

        Instances[(int) ProjectorID] = this;
    }

    void Start()
    {
        if (!HoloHelper.isHololens())
            return;

        //WorldAnchorStore.GetAsync(AnchorStoreReady);

        // Add an AudioSource component and set up some defaults
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        //audioSource.spatialize = true;
        //audioSource.spatialBlend = 1.0f;
        //audioSource.dopplerLevel = 0.0f;
        //audioSource.rolloffMode = AudioRolloffMode.Custom;

        // Load the Sphere sounds from the Resources folder
        startClip = Resources.Load<AudioClip>("calibrationStart");
        stopClip = Resources.Load<AudioClip>("calibrationStop");

        //CalibrationObject = transform.FindChild("ProjectorObj").gameObject;
        //CalibrationObject = transform.FindChild("ProjectorObj").gameObject.transform.FindChild("ProjectorMesh").gameObject;
        CalibrationObject = this.gameObject;
        CalibrationMeshObj = transform.FindChild("ProjectorMesh").gameObject;


        SceneObject = GameObject.Find("SceneObj").gameObject;
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            this.OnSelect();
        };
        //recognizer.StartCapturingGestures();
#endif

        HoloTransform savedTransform = LoadProjectorTransform(ProjectorID);
        if (savedTransform != null)
        {
            Debug.Log("Save found for " + ProjectorID.ToString());
            CalibrationObject.transform.localPosition = savedTransform.position;
            CalibrationObject.transform.localRotation = savedTransform.rotation;
        }

        //if (PlayerPrefs.HasKey(ProjectorID.ToString() + "PosX") && PlayerPrefs.HasKey(ProjectorID.ToString() + "PosY") && PlayerPrefs.HasKey(ProjectorID.ToString() + "PosZ")) { 
        //    Vector3 pos = new Vector3(PlayerPrefs.GetFloat(ProjectorID.ToString() + "PosX"), PlayerPrefs.GetFloat(ProjectorID.ToString() + "PosY"), PlayerPrefs.GetFloat(ProjectorID.ToString() + "PosZ"));
        //    Debug.Log("ProjPos " + pos);
        //    CalibrationObject.transform.localPosition = pos;
        //}
        //if (PlayerPrefs.HasKey(ProjectorID.ToString() + "RotX") && PlayerPrefs.HasKey(ProjectorID.ToString() + "RotY") && PlayerPrefs.HasKey(ProjectorID.ToString() + "RotZ") && PlayerPrefs.HasKey(ProjectorID.ToString() + "RotW"))
        //{
        //    Quaternion rot = new Quaternion(PlayerPrefs.GetFloat(ProjectorID.ToString() + "RotX"), PlayerPrefs.GetFloat(ProjectorID.ToString() + "RotY"), PlayerPrefs.GetFloat(ProjectorID.ToString() + "RotZ"), PlayerPrefs.GetFloat(ProjectorID.ToString() + "RotW"));
        //    Debug.Log("ProjRot " + rot);
        //    CalibrationObject.transform.localRotation = rot;
        //}

        if (showCalibration)
            OnShowCalibration();

        RemoteCmdHandler.Instance.RegisterForCmd(RemoteCmdType.MoveRef, ProjectorID.ToString(), MoveProjector);

    }

    void MoveProjector(string dest, string data)
    {

        HoloTransform ht = JsonUtility.FromJson<HoloTransform>(data);
        //Debug.Log("Move " + ProjectorID.ToString() + ": " + ht.position.ToString() + " - " + ht.rotation.ToString());

        // return;

        if (!calibratingFine)
        {
            //Debug.Log("Start Fine Calib " + Time.time);
            OnStartFineProjectorCalibration();

            lastFineCalibTime = Time.time;
            StartCoroutine(StopFineCalibration());
        }

        lastFineCalibTime = Time.time;

        CalibrationObject.transform.position += CalibrationObject.transform.rotation * ht.position;
        CalibrationObject.transform.Rotate(ht.rotation.eulerAngles);
        //this.transform.rotation = ht.rotation;
        //Debug.Log("X: " + ht.rotation.eulerAngles.x);
    }

    IEnumerator StopFineCalibration()
    {
        //yield return new WaitUntil(() => (calibratingFine && Time.time - lastFineCalibTime < 1.0));
        while (calibratingFine && Time.time - lastFineCalibTime < 1.0)
        {
            //Debug.Log("Wait " + calibratingFine.ToString() + (Time.time - lastFineCalibTime));
            yield return new WaitForSeconds(0.1f);
        }

        OnStopFineProjectorCalibration();

        //Debug.Log("Stop Fine Calib " + ProjectorID.ToString() + Time.time);
        yield return 0;
    }

    public bool isCalibrating()
    {
        return calibrating;
    }

    void OnShowCalibration()
    {
        showCalibration = true;
        CalibrationMeshObj.SetActive(true);
    }

    void OnHideCalibration()
    {
        showCalibration = false;
        CalibrationMeshObj.SetActive(false);
    }

    void OnStartProjectorCalibration()
    {
        //Debug.Log("Start Proj Calib " + ProjectorID.ToString());
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        recognizer.StartCapturingGestures();
#endif
        calibrating = true;

        audioSource.clip = startClip;
        audioSource.Play();

        OnShowCalibration();

        if(!showSceneDuringCalibration)
            SceneObject.SetActive(false);

        //SpatialMapping.Instance.DrawVisualMeshes = true;
    }

    void OnStartFineProjectorCalibration()
    {
        calibratingFine = true;
        //audioSource.clip = startClip;
        //audioSource.Play();

        OnShowCalibration();

        if (!showSceneDuringCalibration)
            SceneObject.SetActive(false);
    }

    void OnSelect()
    {
        OnStopProjectorCalibration();
    }

    static string GetSaveFileName(HoloID id)
    {
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        //Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;
        string path = Path.Combine(localFolder.Path, id.ToString() + "Save.save");
        Debug.Log("Savename " + path);
        return path;
#else
        return Path.Combine(Application.persistentDataPath, id.ToString() + "Save.save");
#endif
    }

    void SaveProjectorTransform()
    {
        //PlayerPrefs.SetFloat(ProjectorID.ToString() + "PosX", CalibrationObject.transform.localPosition.x);
        //PlayerPrefs.SetFloat(ProjectorID.ToString() + "PosY", CalibrationObject.transform.localPosition.y);
        //PlayerPrefs.SetFloat(ProjectorID.ToString() + "PosZ", CalibrationObject.transform.localPosition.z);
        //PlayerPrefs.SetFloat(ProjectorID.ToString() + "RotX", CalibrationObject.transform.localRotation.x);
        //PlayerPrefs.SetFloat(ProjectorID.ToString() + "RotY", CalibrationObject.transform.localRotation.y);
        //PlayerPrefs.SetFloat(ProjectorID.ToString() + "RotZ", CalibrationObject.transform.localRotation.z);
        //PlayerPrefs.SetFloat(ProjectorID.ToString() + "RotW", CalibrationObject.transform.localRotation.w);
        //PlayerPrefs.Save();
        //Debug.Log("Save " + CalibrationObject.transform.localPosition + " - " + CalibrationObject.transform.localRotation);
        HoloTransform projTr = new HoloTransform(CalibrationObject.transform.localPosition, CalibrationObject.transform.localRotation);
        System.IO.File.WriteAllText(GetSaveFileName(ProjectorID), UnityEngine.JsonUtility.ToJson(projTr));
    }

    HoloTransform LoadProjectorTransform(HoloID proj)
    {
        HoloTransform projTr = null;
        if (System.IO.File.Exists(GetSaveFileName(ProjectorID)))
        {
            projTr = UnityEngine.JsonUtility.FromJson<HoloTransform>(System.IO.File.ReadAllText(GetSaveFileName(ProjectorID)));
            //Debug.Log("Load  " + GetSaveFileName(ProjectorID));
        }
        else
        {
            //Debug.Log("Load unable to find " + GetSaveFileName(ProjectorID));
        }
        return projTr;
    }

    void OnStopProjectorCalibration()
    {
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        recognizer.CancelGestures();
        //recognizer.StartCapturingGestures();
        recognizer.StopCapturingGestures();
#endif
        calibrating = false;

        audioSource.clip = stopClip;
        audioSource.Play();

        if (!showCalibration)
            OnHideCalibration();
        SceneObject.SetActive(true);

        //Quaternion rot = new Quaternion(PlayerPrefs.GetFloat("ProjRotX"), PlayerPrefs.GetFloat("ProjRotY"), PlayerPrefs.GetFloat("ProjRotZ"), PlayerPrefs.GetFloat("ProjRotW"));

        SaveProjectorTransform();
        //Debug.Log(CalibrationObject.transform.localPosition.ToString("F10"));          
    }

    void OnStopFineProjectorCalibration()
    {
        calibratingFine = false;
        //audioSource.clip = stopClip;
        //audioSource.Play();

        if (!showCalibration)
            OnHideCalibration();
        SceneObject.SetActive(true);
        
        SaveProjectorTransform();
    }

    // Update is called once per frame
    void Update()
    {
        if (!HoloHelper.isHololens())
            return;

        // If the user is in placing mode,
        // update the placement to match the user's gaze.

        if (calibrating)
        {
            CalibrationObject.transform.position = Camera.main.transform.position + Camera.main.transform.TransformDirection(calibrationPosition);
            CalibrationObject.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(calibrationRotation); ;
            //this.transform.position = Camera.main.transform.position + Camera.main.transform.TransformVector(.0f,.0f,-1.5f);
            //    this.transform.rotation = Camera.main.transform.rotation;
            //this.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(HeadRelativeRotation);
            //this.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(new Vector3(-35, 0, 0));
        }
    }

    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }
}


