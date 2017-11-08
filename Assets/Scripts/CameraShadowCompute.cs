using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraShadowCompute : MonoBehaviour
{
    public bool occlusionShadows = true;
    public bool realShadows = true;

    //private Material m_Material;
    public Shader shader;
    //public Material material;
    public Texture texL;
    public Texture texR;
    public Texture shadows;

    public Shader calibrationShader;

    private bool blurEnabled;

    void Start()
    {
        //return;
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
        if (GetComponent<Camera>())
        {
            Debug.Log("SetShader for " + GetComponent<Camera>().name + " : " + shader.name);
            GetComponent<Camera>().SetReplacementShader(shader, "RenderType");
            //Renderer rend = GetComponent<Camera>().GetComponent<Renderer>();
            //rend.material.SetTexture("_HoloDepthTextureL", texL);
            //rend.material.SetTexture("_HoloDepthTextureR", texR);
            //rend.material.SetTexture("_ProjShadowMap", shadows);
            Shader.SetGlobalTexture("_HoloDepthTextureL", texL);
            Shader.SetGlobalTexture("_HoloDepthTextureR", texR);
            //Shader.SetGlobalTexture("_ProjShadowMap", shadows);
            //GetComponent<Camera>().ResetReplacementShader();
        }

    }

    void OnShowRoomCalibration()
    {
        if (!enabled)
            return;
        if (GetComponent<Camera>())
        {
            Debug.Log("SetShader for " + GetComponent<Camera>().name + " : " + calibrationShader.name);
            GetComponent<Camera>().SetReplacementShader(calibrationShader, "RenderType");
        }
        if (gameObject.GetComponent<UnityStandardAssets.ImageEffects.BlurOptimized>())
        {
            blurEnabled = gameObject.GetComponent<UnityStandardAssets.ImageEffects.BlurOptimized>().enabled;
            gameObject.GetComponent<UnityStandardAssets.ImageEffects.BlurOptimized>().enabled = false;
        }
    }

    void OnHideRoomCalibration()
    {
        if (!enabled)
            return;
        if (GetComponent<Camera>())
        {
            Debug.Log("SetShader for " + GetComponent<Camera>().name + " : " + shader.name);
            GetComponent<Camera>().SetReplacementShader(shader, "RenderType");
        }
        if (blurEnabled)
        {
            gameObject.GetComponent<UnityStandardAssets.ImageEffects.BlurOptimized>().enabled = true;
        }
    }

    //[ImageEffectOpaque]
    //void OnRenderImage(RenderTexture src, RenderTexture dest)
    //{
    //    Graphics.Blit(src, dest, material);
    //}

    void OnPreRender()
    {
        //Debug.Log("OnPreRender");
        //Shader.SetGlobalTexture("_HoloDepthTextureL", texL);
        //Shader.SetGlobalTexture("_HoloDepthTextureR", texR);
        Shader.SetGlobalTexture("_ProjShadowMap", shadows);
    }

    //protected Material material
    //{
    //    get
    //    {
    //        if (m_Material == null)
    //        {
    //            m_Material = new Material(shader);
    //            m_Material.hideFlags = HideFlags.HideAndDontSave;
    //        }
    //        return m_Material;
    //    }
    //}

    //public void Update()
    //{
    //}
}
