using UnityEngine;
using System.Collections;

public class MoveAround : MonoBehaviour {

    public Vector3 moveVec = new Vector3(1, 0, 0);
    public float speed = 0.5f;
    private Vector3 initPos;
	// Use this for initialization
	void Start () {
        initPos = this.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.position = initPos + (Mathf.PingPong(Time.time * speed, 2) - 1.0f) * moveVec;
	}
}
