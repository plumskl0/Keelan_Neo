using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarFlipper : MonoBehaviour {

    private Rigidbody rb;
    private bool flipped;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}

    public float power = 10.0F;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ramp"))
        {
            rb.AddForce(Vector3.up * power, ForceMode.VelocityChange);
        }
        
    }

   
}
