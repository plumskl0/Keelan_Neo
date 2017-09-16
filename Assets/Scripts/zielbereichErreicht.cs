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

    private void Start()
    {
        carControl = GetComponent<AlternateCarController>();
        db = GameObject.Find("Endbild").GetComponent<DatabaseManager>();
    }

    int anzahlGewonnen = 0;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("zielbereich"))
        {
            //Debug.Log("LadeBild");
            anzahlGewonnen++;

            sharedData.SetPlayerControl(false);
            //carControl.setPlayerControl(false);

            PickupLogic pul = this.GetComponent<PickupLogic>();
            pul.stopTimer();
            coinCount = this.GetComponent<PickupLogic>().CoinCount;
            timeNeeded = pul.time;

            coinCountText.text = coinCount.ToString();
            //timeNeededText.text = timeNeeded.ToString();
            timeNeededText.text = pul.TimerText.text;
            if (anzahlGewonnen == 1)
            {
                db.InsertNewScore(timeNeeded);
            }
            endbildschirm.enabled = true;
            Debug.Log("Habe so oft gewonnen: " +anzahlGewonnen);

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

    public void RestartGame()
    {
        Application.LoadLevel(1);
    }
}
