using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float smoothing = 6;
    public Transform cameraTarget;
    public Transform cameraPosition;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        UpdateCamera();
	}

    private void UpdateCamera()
    {
        transform.position = Vector3.Lerp(transform.position, cameraPosition.position, Time.deltaTime * smoothing);
        transform.LookAt(cameraTarget);
    }
}
