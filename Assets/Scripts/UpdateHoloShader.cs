using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class UpdateHoloShader : MonoBehaviour
{

    public bool projMatrices = false;
    public bool holoMatricesL = false;
    public bool holoMatricesR = false;

    private bool d3d;
    private Camera m_Camera;

    protected Camera currCamera
    {
        get
        {
            if (m_Camera == null)
            {
                //m_holoCamera = GameObject.Find("HoloCamera").GetComponent<Camera>();
                m_Camera = gameObject.GetComponent<Camera>();
            }
            return m_Camera;
        }
    }

    void Start()
    {
        d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
    }
    void OnPreRender()
    {


        //Debug.Log(this.name + " OnPreRender stereo:" + (holoCamera.stereoEnabled ? "true" : "false") + " target:" +
        //    ((holoCamera.stereoTargetEye == StereoTargetEyeMask.Both) ? "both" :
        //    ((holoCamera.stereoTargetEye == StereoTargetEyeMask.Left) ? "left" :
        //    ((holoCamera.stereoTargetEye == StereoTargetEyeMask.Right) ? "right" : "none")
        //    )));

        //Matrix4x4 holoM = holoCamera.transform.localToWorldMatrix;
        Matrix4x4 M = GameObject.Find("ReferenceRoot").transform.localToWorldMatrix;
        Matrix4x4 V = currCamera.worldToCameraMatrix;
        Matrix4x4 P = currCamera.projectionMatrix;
        if (d3d)
        {
            // Invert Y for rendering to a render texture
            for (int i = 0; i < 4; i++)
            {
                P[1, i] = -P[1, i];
            }
            // Scale and bias from OpenGL -> D3D depth range
            for (int i = 0; i < 4; i++)
            {
                P[2, i] = P[2, i] * 0.5f + P[3, i] * 0.5f;
            }
        }

        if (holoMatricesL)
        {
            Shader.SetGlobalMatrix("_holoML", M);
            Shader.SetGlobalMatrix("_holoVL", V);
            Shader.SetGlobalMatrix("_holoPL", P);
            //Debug.Log("HModel" + M.ToString());
            //Debug.Log(currCamera.name + " HViewL" + V.ToString());
            //Debug.Log(currCamera.name + " HProjL " + P.ToString());
        }
        if (holoMatricesR)
        {
            Shader.SetGlobalMatrix("_holoMR", M);
            Shader.SetGlobalMatrix("_holoVR", V);
            Shader.SetGlobalMatrix("_holoPR", P);
            //Debug.Log("HModel" + M.ToString());
            //Debug.Log(currCamera.name + " HViewR" + V.ToString());
            //Debug.Log(currCamera.name + " HProjR " + P.ToString());
        }
        if (projMatrices)
        {
            Shader.SetGlobalMatrix("_projM", M);
            Shader.SetGlobalMatrix("_projV", V);
            Shader.SetGlobalMatrix("_projP", P);
            //Debug.Log("PModel" + M.ToString());
            //Debug.Log(currCamera.name + " PView" + V.ToString());
            //Debug.Log(currCamera.name + " PProj " + P.ToString());
        }
    }
}

