using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraAspectRatio : MonoBehaviour {

    public float aspectRatio = 16.0f / 9.0f;
	// Use this for initialization
	void Start () {
        GetComponent<Camera>().aspect = aspectRatio;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
