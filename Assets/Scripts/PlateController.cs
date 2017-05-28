using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateController : MonoBehaviour {

    public float angle = 25f;

    //public Transform plateTransform;
    private Transform plateTransform;

    private float y;
    private float z;

    // Use this for initialization
    void Start () {
        plateTransform = GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () {
        y = Input.GetAxis("VerticalPlate") * angle;
        z = Input.GetAxis("HorizontalPlate") * angle;

        plateTransform.localRotation = Quaternion.Euler(y, 0f, z);

	}
}
