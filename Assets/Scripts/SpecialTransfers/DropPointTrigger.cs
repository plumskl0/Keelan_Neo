using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropPointTrigger : MonoBehaviour {

    public GameObject endpoint;
    private Rigidbody car;

    private void Update()
    {
        if (car != null)
        {
            if (car.velocity.sqrMagnitude <= 1)
                endpoint.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            car = other.attachedRigidbody;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            car = null;
            endpoint.SetActive(true);   
        }
    }
}
