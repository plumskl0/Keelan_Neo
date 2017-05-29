using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindForce : MonoBehaviour {

    [Tooltip("Kraft die der Wind auf den Ball auswirkt.")]
    public float windForce = 1f;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            //ball.AddForce(
            // Vector3.right * windForce, ForceMode.Acceleration);
            Rigidbody ball = other.attachedRigidbody;
            
            Vector3 locVel = transform.
                InverseTransformDirection(ball.velocity);
            locVel.z = windForce;
            ball.velocity = transform.TransformDirection(locVel);
        }
    }
}
