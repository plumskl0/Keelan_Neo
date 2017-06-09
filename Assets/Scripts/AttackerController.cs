using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackerController : MonoBehaviour {

    public Transform ballDropZone;

    public BallInZoneCheck attackZone;

    private Rigidbody rb;
    private Rigidbody ballRb;
    
    private bool ballCatched;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Update()
    {
        if (attackZone.isBallInZone)
        {
            Debug.Log("Ball is in Zone");
        }
        else
        {
            Debug.Log("Ball is not in Zone");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            ballRb = other.gameObject.GetComponent<Rigidbody>();

        }
    }
}