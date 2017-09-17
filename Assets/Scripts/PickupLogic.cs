using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        setLifeCount();
        checkIfLevel1();

        //CoinCountText.text = " ";
        setCoinText();
        TimerText.text = "00:00:000";
        
        startTimer();
    }
    
    private void checkIfLevel1()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "Level1")
        {
            for (int i = 0; i < 3; i++)
            {
                removeLife();
            }
        }
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
            setCoinText();

            // Leben auffüllen
            for (int i = 0; i < lifes.Length; i++)
            {
                addLife();
            }
        }
    }

    private void coinPayed()
    {
        sharedData.CoinCount -= sharedData.getPayedCoins();
        setCoinText();
    }

    private void setCoinText()
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
            disableLifeIcon();
        }
        if (lifeCount <= 0)
        {
            disableLifeIcon();
            sharedData.SetCursorVisible(true);
            GameOverCanvas.enabled = true;
            setLifeCount();
        }
    }

    public void addLife()
    {
        if (lifeCount < lifes.Length)
        {
            enableLifeIcon();
            lifeCount++;
        }
    }

    private void disableLifeIcon()
    {
        lifes[lifeCount].enabled = false;
    }

    private void enableLifeIcon()
    {
        lifes[lifeCount].enabled = true;
    }

    private void setLifeCount()
    {
        lifeCount = lifes.Length;
    }
}
