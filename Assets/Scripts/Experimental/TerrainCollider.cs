using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainCollider : MonoBehaviour {

    private SharedFields sharedData = SharedFields.Instance;

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ball"))
           setResetAccordingToTag(col.gameObject.tag);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Teller"))
            setResetAccordingToTag(other.gameObject.tag);
    }

    private void setResetAccordingToTag(string tag)
    {
        sharedData.LostLife = true;
        sharedData.CarReset = true;

        if (tag.Equals("Ball"))
            sharedData.BallReset = true;
            
    }
}
