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
        //wenn der Teller das Terrain berührt ist das Auto wahrscheinlich umgefallen -> muss wieder aufgestellt werden
        if (other.gameObject.CompareTag("Teller"))
            setResetAccordingToTag(other.gameObject.tag);
    }

    private void setResetAccordingToTag(string tag)
    {
        sharedData.LostLife = true;
        sharedData.CarResetNeeded = true;

        if (tag.Equals("Ball"))
            sharedData.BallResetNeeded = true;
            
    }
}
