using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetBall : MonoBehaviour {

    public Transform resetPosition;

    private Rigidbody rb;
    private float radius;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        radius = GetComponent<SphereCollider>().radius;
	}
	
	// Update is called once per frame
	void Update () {
        if (ResetCar.debug && Input.GetKey(KeyCode.R))
        {
            // Ball unter beachtung des Radius auf dem Fahrzeug positionieren
            Vector3 pos = resetPosition.position;
            pos.Set(pos.x, pos.y + radius, pos.z);
            transform.position = pos;

            // Falls der Ball noch rollt die Geschwindigkeit entfernen
            rb.velocity = Vector3.zero;
        }
	}
}
