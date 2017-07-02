using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickupLogic : MonoBehaviour {

    public int CoinCount { get; private set; }

    public Text CoinCountText;

    public Text TimerText;

    private float time;

    private void Start()
    {
        CoinCountText.text = " ";
    }

    void Update()
    {
        time += Time.deltaTime;

        var minutes = time / 60; //Divide the guiTime by sixty to get the minutes.
        var seconds = time % 60;//Use the euclidean division for the seconds.
        var fraction = (time * 100) % 100;

        //update the label value
        TimerText.text = string.Format("{0:00} : {1:00} : {2:000}", minutes, seconds, fraction);
    }

    private void OnTriggerEnter(Collider other)
    {        
        if (other.CompareTag("Coin"))
        {
            other.gameObject.SetActive(false);
            CoinCount++;
            setText();
        }
    }

    private void setText()
    {
        CoinCountText.text = "Münzen " + CoinCount;
    }
}
