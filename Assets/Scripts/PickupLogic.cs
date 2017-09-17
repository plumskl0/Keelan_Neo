using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickupLogic : MonoBehaviour {

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
        //CoinCountText.text = " ";
        //TimerText.text = "00:00:000";

        //setLifeCount();

        //startTimer();
        CoinCountText.text = sharedData.CoinCount.ToString();
        time = sharedData.TimeNeededToLastLevel;
        TimerText.text = getTimerText();
        //*********hier muss eine Initialisierung der Leben rein
        setLifeCount();
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

        if (sharedData.PayedCoin)
        {
            if (sharedData.CoinCount > 0)
            {
                coinPayed();
            }
            else
            {
                Debug.Log("No more coins left");
            }
            sharedData.PayedCoin = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {        
        // Erhöht die Münzenanzahl um 1
        if (other.CompareTag("Coin"))
        {
            other.gameObject.SetActive(false);
            sharedData.CoinCount++;
            setText();
        }
    }

    private void coinPayed()
    {
        sharedData.CoinCount--;
        setText();
    }

    private void setText()
    {
        CoinCountText.text = "Münzen " + sharedData.CoinCount;
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
        if (lifeCount >= 1)
        {
            disableLifeIcon(lifeCount);
        }
        if (lifeCount <= 0)
        {
            disableLifeIcon(lifeCount);
            sharedData.SetCursorVisible(true);
            GameOverCanvas.enabled = true;
            setLifeCount();
        }
    }

    private void disableLifeIcon(int n)
    {
        lifes[lifeCount].enabled = false;
    }

    private void setLifeCount()
    {
        lifeCount = lifes.Length;
    }
}
