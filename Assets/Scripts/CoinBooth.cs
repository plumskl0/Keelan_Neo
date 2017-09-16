using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBooth : MonoBehaviour {

    private SharedFields sharedData = SharedFields.Instance;

    // Zur Vermeidung von doppelter Bezahlung
    private bool boothUsedOnce = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!boothUsedOnce)
            {
                sharedData.PayedCoin = true;
                boothUsedOnce = true;
            }
        }
    }

}
