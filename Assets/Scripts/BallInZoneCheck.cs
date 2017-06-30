using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInZoneCheck : MonoBehaviour
{

    public bool isBallInZone { get; set; }

    public Rigidbody ballRb { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (IsBall(other))
        {
            isBallInZone = true;
            ballRb = other.attachedRigidbody;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsBall(other))
        {
            isBallInZone = false;
        }
    }

    private bool IsBall(Collider other)
    {
        return other.CompareTag("Ball");
    }
}
