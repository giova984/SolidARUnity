using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
//[ExecuteInEditMode]
public class MaterialSwitcher : MonoBehaviour {

    public Material IdleMaterial;
    public Material PlayMaterial;
    public bool Children = true;
    
	// Use this for initialization
	void Start () {
        UpdateMaterial();
	}
	
	// Update is called once per frame
	void Update () {
#if UNITY_EDITOR
        if(!EditorApplication.isPlaying) 
            UpdateMaterial();
#endif
    }

    void UpdateMaterial()
    {
        if (Application.isPlaying)
        {
            if (PlayMaterial)
            {
                SetMaterial(PlayMaterial);
            }
        }
        else
        {
            if (IdleMaterial)
            {
                SetMaterial(IdleMaterial);
            }
        }
    }

    void OnUseIdleMaterial()
    {
        if (IdleMaterial)
        {
            SetMaterial(IdleMaterial);
        }
    }

    void OnUsePlayMaterial()
    {
        if (PlayMaterial)
        {
            SetMaterial(PlayMaterial);
        }
    }


    void SetMaterial(Material mat)
    {
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr)
            {
                //Debug.Log("SetMaterial " + mat.name + " to " + mr.gameObject.name);
                mr.sharedMaterial = mat;
                for (int i = 0; i < mr.sharedMaterials.Length; ++i)
                {
                    mr.sharedMaterials[i] = mat;
                }
            }
        }

        if (Children)
        {
            foreach (MeshRenderer mr in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                //Debug.Log("SetMaterial " + mat.name + " to " + mr.gameObject.name);
                mr.sharedMaterial = mat;
                for(int i = 0; i < mr.sharedMaterials.Length; ++i)
                {
                    //Debug.Log("SetMaterial " + mat.name + " to " + mr.gameObject.name + mr.sharedMaterials.Length);
                    mr.sharedMaterials[i] = mat;
                }
            }
        }
    }
}
