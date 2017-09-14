using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickupLogic : MonoBehaviour {

    public int CoinCount { get; private set; }

    public Text CoinCountText;

    public Text TimerText;

    public float time { get; private set; }

    public Image[] lifes;

    public Canvas GameOverCanvas;
   
    private bool timerStarted;
    private int lifeCount;
    private SharedFields sharedData = SharedFields.Instance;

    private void Start()
    {
        CoinCountText.text = " ";
        TimerText.text = "00:00:000";

        lifeCount = lifes.Length;

        startTimer();
    }

    void Update()
    {
        if (timerStarted)
            time += Time.deltaTime;

        //update the label value
        TimerText.text = getTimerText();

        if (sharedData.LostLife)
        {
            removeLife();
            sharedData.LostLife = false;
        }
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

    public void startTimer()
    {
        timerStarted = true;
    }

    public void stopTimer()
    {
        timerStarted = false;
    }

    public string getTimerText()
    {
        var minutes = time / 60; //Divide the guiTime by sixty to get the minutes.
        var seconds = time % 60;//Use the euclidean division for the seconds.
        var fraction = (time * 100) % 100;

        return string.Format("{0:00} : {1:00} : {2:000}", minutes, seconds, fraction);
    }

    public void removeLife()
    {
        //Debug.Log(lifeCount);
        lifeCount--;
        lifes[lifeCount].enabled = false;
        if (lifeCount <= 0)
        {
            sharedData.SetCursorVisible(true);
            GameOverCanvas.enabled = true;
        }
    }

}
