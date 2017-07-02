using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class zielbereichErreicht : MonoBehaviour {
    public Canvas endbildschirm;
    public int coinCount;
    private int timeNeeded;

    public Text coinCountText;
    public Text timeNeededText;


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("zielbereich"))
        {
            Debug.Log("LadeBild");
            coinCount = this.GetComponent<PickupLogic>().CoinCount;
            timeNeeded = this.GetComponent<PickupLogic>().CoinCount;

            coinCountText.text = coinCount.ToString();
            timeNeededText.text = timeNeeded.ToString();
            endbildschirm.enabled = true;
        }
    }
}
