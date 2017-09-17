using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBooth : MonoBehaviour {

    private SharedFields sharedData = SharedFields.Instance;

    public int cost = 1;

    // Zur Vermeidung von doppelter Bezahlung
    public bool BoothUsedOnce { get; private set; }

    // Falls die Booth nicht direkt benutzt werden soll
    public bool ManualBooth { get; set; }

    public bool WasPayed { get; private set; }

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
                    if ((sharedData.CoinCount - cost) >= 0)
                    {
                        usedBooth();
                    }
                    else
                    {
                        // Zu wenig Geld
                        BoothUsedOnce = true;
                        WasPayed = false;
                    }
                }
            }
        }
    }

    private void usedBooth()
    {
        sharedData.payedCoins(cost);
        sharedData.PayedCoin = true;
        WasPayed = true;
        BoothUsedOnce = true;
        Debug.Log("Booth was payed and used");
    }

    public void usedManualBooth()
    {
        usedBooth();
    }

}
