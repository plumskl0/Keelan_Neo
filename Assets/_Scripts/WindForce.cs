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
            Rigidbody ball = other.attachedRigidbody;

            // Rechnet die globale Geschwindigkeit des Balls um und bewegt den Ball 
            // immer in einer Richtung weg, egal in welcher Position der Ball sich befindet
            Vector3 locVel = transform.InverseTransformDirection(ball.velocity);
            locVel.z = windForce;
            ball.velocity = transform.TransformDirection(locVel);
        }
    }
}
