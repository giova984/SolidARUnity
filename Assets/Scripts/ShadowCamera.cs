using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ShadowCamera : MonoBehaviour
{

    Camera projCamera = null;

    void UpdateCameraMatrices()
    {
        if (projCamera)
        {
            Camera cam = this.GetComponent<Camera>();
            cam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, projCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left));
            cam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, projCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right));
            cam.SetStereoViewMatrix(Camera.StereoscopicEye.Left, projCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left));
            cam.SetStereoViewMatrix(Camera.StereoscopicEye.Right, projCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right));
            cam.worldToCameraMatrix = projCamera.worldToCameraMatrix;
            cam.projectionMatrix = projCamera.projectionMatrix;
        }
    }

    // Use this for initialization
    void Start()
    {
        if (transform.parent && transform.parent.GetComponent<Camera>())
        {
            projCamera = transform.parent.GetComponent<Camera>();
            //Debug.Log("projCamera found");
        }
        UpdateCameraMatrices();

    }

    // Update is called once per frame
    void Update()
    {
        UpdateCameraMatrices();
    }
}
