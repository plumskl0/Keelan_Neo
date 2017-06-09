using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInZoneCheck : MonoBehaviour
{

    public bool isBallInZone { get; set; }

    private void OnTriggerEnter(Collider other)
    {
        if (IsBall(other))
        {
            isBallInZone = true;
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
