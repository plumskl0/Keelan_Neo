using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainCollider : MonoBehaviour {

    private SharedFields sharedData = SharedFields.Instance;

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ball"))
        {
            sharedData.LostLife = true;
            sharedData.CarReset = true;
        }
    }
}
