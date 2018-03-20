using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;

public class zielbereichErreicht : MonoBehaviour {
    public Canvas endbildschirm;
    public int coinCount;
    private float timeNeeded;

    public Text coinCountText;
    public Text timeNeededText;

    private AlternateCarController carControl;
    private SharedFields sharedData = SharedFields.Instance;
    private DatabaseManager db;
    private PickupLogic pul;

    private void Start()
    {
        carControl = GetComponent<AlternateCarController>();
        db = GameObject.Find("Endbild").GetComponent<DatabaseManager>();
        pul = GetComponent<PickupLogic>();
        
    }

    int anzahlGewonnen = 0;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag.ToString());
        if(other.CompareTag("zielbereich"))
        {
            //Debug.Log("LadeBild");
            anzahlGewonnen++;

            sharedData.SetPlayerControl(false);
            //carControl.setPlayerControl(false);

            PickupLogic pul = this.GetComponent<PickupLogic>();
            pul.stopTimer();
            coinCount = sharedData.CoinCount;
            timeNeeded = pul.time;

            coinCountText.text = coinCount.ToString();
            //timeNeededText.text = timeNeeded.ToString();
            timeNeededText.text = pul.TimerText.text;
            if (anzahlGewonnen == 1)
            {
                db.InsertNewScore(timeNeeded);
            }
            endbildschirm.enabled = true;
            sharedData.SetCursorVisible(true);
            Debug.Log("Habe so oft gewonnen: " +anzahlGewonnen);
        }

        if (other.CompareTag("zielLevel1"))
        {
            Debug.Log("Lade Level2");
            sharedData.TimeNeededToLastLevel = pul.time;
            Debug.Log("Setze time auf: " + pul.time);
            Application.LoadLevel("Level2");
        }

        if (other.CompareTag("zielLevel2"))
        {
            sharedData.TimeNeededToLastLevel = pul.time;
            Debug.Log("Setze time auf: " + pul.time);
            Debug.Log("Lade Level3");
            Application.LoadLevel("Level3");

        }


    }

    public void QuitGame()
    {
        if(PlateController.wiiRemote != null)
        {
            WiimoteManager.Cleanup(PlateController.wiiRemote);
            PlateController.wiiRemote = null;
        }
        Application.Quit();
    }

    public void RestartGame(String levelInDasGesprungenWird)
    {
        Application.LoadLevel(levelInDasGesprungenWird);
        sharedData.CarResetNeeded = false;
        if (levelInDasGesprungenWird == "Level1")
        {
            sharedData.CoinCount = 0;
        }
        sharedData.SetCursorVisible(false);
    }
}
