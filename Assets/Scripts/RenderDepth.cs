using UnityEngine;
using System.Collections;

public class RenderDepth : MonoBehaviour
{
    private Material m_Material;
    public Shader shader;

    void Start()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
        GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        GetComponent<Camera>().backgroundColor = Color.white;
        //GetComponent<Camera>().renderingPath = RenderingPath.Forward;
        //GetComponent<Camera>().SetReplacementShader(Shader.Find("DepthShader"),"RenderType");
        //GetComponent<Camera>().SetReplacementShader(shader, "RenderType");
    }

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
       Graphics.Blit(src, dest, material);
    }

    protected Material material
    {
        get
        {
            if (m_Material == null)
            {
                m_Material = new Material(shader);
                m_Material.hideFlags = HideFlags.HideAndDontSave;
            }
            return m_Material;
        }
    }

    public void Update()
    {
       
    }
}
