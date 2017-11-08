using UnityEngine;
using System.Collections;
using UnityEngine.VR.WSA;
#if UNITY_WSA_10_0
using UnityEngine.VR.WSA.Persistence;
using UnityEngine.VR.WSA.Input;
#endif
public class ReferenceCalibration : MonoBehaviour
{

    public static ReferenceCalibration Instance { get; private set; }

    public Vector3 calibratingPosition = new Vector3(0, -1.0f, 1.2f);
    public Vector3 calibratingRotation = new Vector3(-35, 0, 0);

    public bool showCalibration = false;

    bool calibrating = false;
    bool anchorLoaded = false;

    AudioSource audioSource = null;
    AudioClip startClip = null;
    AudioClip stopClip = null;

    GameObject CalibrationObject = null;
    GameObject SceneObject = null;

    
    //bool savedRoot = false;

    bool calibratingFine = false;
    float lastFineCalibTime = .0f;

    public string ObjectAnchorStoreName;

#if UNITY_WSA_10_0
    WorldAnchorStore anchorStore = null;

    GestureRecognizer recognizer;
#endif
    void Awake()
    {
        if (!HoloHelper.isHololens())
        {
            this.enabled = false;
            //Debug.Log("ReferenceCalibration disabled");
        }
        Instance = this;
    }

    void Start()
    {
#if UNITY_WSA_10_0
        WorldAnchorStore.GetAsync(AnchorStoreReady);
#endif
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

        CalibrationObject = transform.FindChild("CalibrationObj").gameObject;
        SceneObject = GameObject.Find("SceneObj").gameObject;
        if (showCalibration)
            CalibrationObject.SetActive(true);

        //if(SpatialMapping.Instance)
        //    SpatialMapping.Instance.DrawVisualMeshes = true;
#if UNITY_WSA_10_0
        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            this.OnSelect();
        };
        //recognizer.StartCapturingGestures();
#endif
        RemoteCmdHandler.Instance.RegisterForCmd(RemoteCmdType.MoveRef, CalibrationID.Reference.ToString(), MoveReference);
    }
#if UNITY_WSA_10_0
    void AnchorStoreReady(WorldAnchorStore store)
    {
        anchorStore = store;
        //placing = true;

        //Debug.Log("looking for " + ObjectAnchorStoreName);
        string[] ids = anchorStore.GetAllIds();
        for (int index = 0; index < ids.Length; index++)
        {
            //Debug.Log(ids[index]);
            if (ids[index] == ObjectAnchorStoreName)
            {
                //WorldAnchor wa = 
                    anchorStore.Load(ids[index], gameObject);
                calibrating = false;
                anchorLoaded = true;
                break;
            }
        }

        if(!anchorLoaded)
            OnStartReferenceCalibration();
    }
#endif
    public bool isCalibrating()
    {
        return calibrating;
    }

    void OnShowCalibration()
    {
        showCalibration = true;
        CalibrationObject.SetActive(true);
    }

    void OnHideCalibration()
    {
        showCalibration = false;
        CalibrationObject.SetActive(false);
    }

    void destroyAnchor()
    {
 #if UNITY_WSA_10_0
        WorldAnchor anchor = gameObject.GetComponent<WorldAnchor>();
        if (anchor != null)
        {
            DestroyImmediate(anchor);
        }

        string[] ids = anchorStore.GetAllIds();
        for (int index = 0; index < ids.Length; index++)
        {
            //Debug.Log(ids[index]);
            if (ids[index] == ObjectAnchorStoreName)
            {
                //bool deleted = 
                    anchorStore.Delete(ids[index]);
                //Debug.Log("deleted: " + deleted);
                break;
            }
        }
#endif
    }

    void createAnchor()
    {
 #if UNITY_WSA_10_0
        WorldAnchor attachingAnchor = gameObject.AddComponent<WorldAnchor>();
        if (attachingAnchor.isLocated)
        {
            //Debug.Log("Saving persisted position immediately");
            //bool saved = 
                anchorStore.Save(ObjectAnchorStoreName, attachingAnchor);
            //Debug.Log("saved: " + saved);
        }
        else
        {
            attachingAnchor.OnTrackingChanged += AttachingAnchor_OnTrackingChanged;
        }
#endif
    }

    void MoveReference(string dest, string data)
    {

        HoloTransform ht = JsonUtility.FromJson<HoloTransform>(data);
        //Debug.Log("Move Reference " + ht.position.ToString() + " - " + ht.rotation.ToString());

        // return;

        if (!calibratingFine)
        {
            //Debug.Log("Start Fine Calib " + Time.time);
            destroyAnchor();
            calibratingFine = true;
            lastFineCalibTime = Time.time;
            StartCoroutine(StopFineCalibration());
        }

        lastFineCalibTime = Time.time;

        this.transform.position += this.transform.rotation * ht.position;
        this.transform.Rotate(ht.rotation.eulerAngles);
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

        createAnchor();
        calibratingFine = false;
        //Debug.Log("Stop Fine Calib " + Time.time);
        yield return 0;
    }

    void OnStartReferenceCalibration()
    {
#if UNITY_WSA_10_0
        if (anchorStore == null)
        {
            return;
        }
        recognizer.StartCapturingGestures();
#endif
        calibrating = true;

        audioSource.clip = startClip;
        audioSource.Play();

        CalibrationObject.SetActive(true);
        SceneObject.SetActive(false);

        destroyAnchor();

        //SpatialMapping.Instance.DrawVisualMeshes = true;
    }

    void OnSelect()
    {
        OnStopReferenceCalibration();
    }
    void OnStopReferenceCalibration()
    {
#if UNITY_WSA_10_0
        if (anchorStore == null)
        {
            return;
        }
        recognizer.CancelGestures();
        recognizer.StopCapturingGestures();
#endif
        calibrating = false;

        audioSource.clip = stopClip;
        audioSource.Play();

        if(!showCalibration)
            CalibrationObject.SetActive(false);
        SceneObject.SetActive(true);

        destroyAnchor();

        createAnchor();

        //if(SpatialMapping.Instance)
        //  SpatialMapping.Instance.DrawVisualMeshes = false;
       
    }

    void OnClearCalibration()
    {
#if UNITY_WSA_10_0
        if (anchorStore == null)
        {
            return;
        }
#endif
        OnStopReferenceCalibration();

        destroyAnchor();
    }

#if UNITY_WSA_10_0
    private void AttachingAnchor_OnTrackingChanged(WorldAnchor self, bool located)
    {
        if (located)
        {
            //Debug.Log("Saving persisted position in callback");
            //bool saved = 
                anchorStore.Save(ObjectAnchorStoreName, self);
            //Debug.Log("saved: " + saved);
            self.OnTrackingChanged -= AttachingAnchor_OnTrackingChanged;
        }
    }
#endif

    // Update is called once per frame
    void Update()
    {
        // If the user is in placing mode,
        // update the placement to match the user's gaze.

        if (calibrating)
        {
            //Debug.Log("Calibrating " + Camera.main.name + " " + Camera.main.transform.position.ToString());
            //this.transform.position = Camera.main.transform.position;
            this.transform.position = Camera.main.transform.position + Camera.main.transform.rotation * (Quaternion.Euler(calibratingRotation) * calibratingPosition);
            //this.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(HeadRelativeRotation);
            this.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(calibratingRotation);
            //this.transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(-35,0,0);
        }
    }
}

