using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickupLogic : MonoBehaviour {

    public int CoinCount { get; private set; }

    public Text CoinCountText;

    private void OnTriggerEnter(Collider other)
    {        
        if (other.CompareTag("Coin"))
        {
            Debug.Log("Bin drin");
            other.gameObject.SetActive(false);
            CoinCount++;
            setText();
        }
    }


    private void setText()
    {
        CoinCountText.text = "Aufgesammelte Coins" + CoinCount;
    }
}
