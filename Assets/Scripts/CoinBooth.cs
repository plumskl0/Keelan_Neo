using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBooth : MonoBehaviour {

    private SharedFields sharedData = SharedFields.Instance;

    // Zur Vermeidung von doppelter Bezahlung
    public bool BoothUsedOnce { get; private set; }

    // Falls die Booth nicht direkt benutzt werden soll
    public bool ManualBooth { get; set; }

    public CoinBooth ()
    {
        ManualBooth = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Ball"))
        {
            if (!BoothUsedOnce)
            {
                if (!ManualBooth)
                {
                    sharedData.PayedCoin = true;
                    BoothUsedOnce = true;
                }
            }
        }
    }

    public void usedManualBooth()
    {
        sharedData.PayedCoin = true;
        BoothUsedOnce = true;
    }

}
