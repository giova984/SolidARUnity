using UnityEngine;

[System.Serializable]
public struct ProjectorParams
{
    public float left, right, bottom, top, near, far, taken_dist;

    public ProjectorParams(float left, float right, float bottom, float top, float near, float far, float taken_dist)
    {
        this.left = left;
        this.right = right;
        this.bottom = bottom;
        this.top = top;
        this.near = near;
        this.far = far;
        this.taken_dist = taken_dist;
    }
}

public enum ProjectorModel
{
    OptomaG750,
    OptomaGT1080,
    Custom
}

static class KnownProjectors
{
    public static ProjectorParams OptomaG750 = new ProjectorParams(-135.5f, 135.5f, 26.0f, 179.0f, 0.3f, 10.0f, 202.9f);
    public static ProjectorParams OptomaGT1080 = new ProjectorParams(-177.9f, 177.9f, 32.0f, 232.2f, 0.3f, 10.0f, 183.7f);
}

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraFrustum : MonoBehaviour {

    public ProjectorModel Model = ProjectorModel.Custom;
    public bool rotatedProjection = false;
    public ProjectorParams Params;
    //public float left, right, bottom, top, near, far, taken_dist;

    static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far, float taken_dist)
    {
        float scale = near / taken_dist;
        float x = (2.0f * near) / (right * scale - left * scale);
        float y = (2.0f * near) / (top * scale - bottom * scale);
        float a = (right * scale + left * scale) / (right * scale - left * scale);
        float b = (top * scale + bottom * scale) / (top * scale - bottom * scale);
        float c = -(far + near) / (far - near);
        float d = -(2.0f * far * near) / (far - near);
        float e = -1.0f;
        
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x; m[0, 1] = 0; m[0, 2] = a; m[0, 3] = 0;
        m[1, 0] = 0; m[1, 1] = y; m[1, 2] = b; m[1, 3] = 0;
        m[2, 0] = 0; m[2, 1] = 0; m[2, 2] = c; m[2, 3] = d;
        m[3, 0] = 0; m[3, 1] = 0; m[3, 2] = e; m[3, 3] = 0;
        return m;
    }

    void UpdateProjectorParams()
    {
        if(Model == ProjectorModel.OptomaG750)
        {
            Params = KnownProjectors.OptomaG750;
        }else if (Model == ProjectorModel.OptomaGT1080)
        {
            Params = KnownProjectors.OptomaGT1080;
        }
    }

    void UpdateCameraFrustum()
    {
        
        if (cam.stereoEnabled)
        {
            Matrix4x4 lm;
            if (rotatedProjection)
                lm = PerspectiveOffCenter(-Params.right, -Params.left, -Params.top, -Params.bottom, Params.near, Params.far, Params.taken_dist);
            else
                lm = PerspectiveOffCenter(Params.left, Params.right, Params.bottom, Params.top, Params.near, Params.far, Params.taken_dist);
            cam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, lm);
            cam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, lm);
            cam.projectionMatrix = lm;
        }
        else
        {
            Matrix4x4 m;
            if (rotatedProjection)
                m = PerspectiveOffCenter(-Params.right, -Params.left, -Params.top, -Params.bottom, Params.near, Params.far, Params.taken_dist);
            else
                m = PerspectiveOffCenter(Params.left, Params.right, Params.bottom, Params.top, Params.near, Params.far, Params.taken_dist);
            cam.projectionMatrix = m;
        }
    }

    Camera cam;

        // Use this for initialization
    void Start () {
        cam = GetComponent<Camera>();
        UpdateProjectorParams();
        UpdateCameraFrustum();
	}

    int eye = 0;
     void OnPreRender()
    {
        //Debug.Log("OnPreRender: "+ name + " Stereo:" + cam.stereoEnabled +" set eye: " + eye % 2);
        Shader.SetGlobalInt("_eye", eye % 2);
        //Camera.main.GetComponent<Renderer>().
        ++eye;
    }

	// Update is called once per frame
	void Update () {
        //Debug.Log("Update: " + name + " Stereo:" + cam.stereoEnabled + " set eye: " + eye % 2);
        eye = 0;
//#if UNITY_EDITOR
        UpdateProjectorParams();
        UpdateCameraFrustum();
//#endif
    }
}
