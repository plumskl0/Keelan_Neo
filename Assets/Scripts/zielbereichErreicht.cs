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

    private void Start()
    {
        carControl = GetComponent<AlternateCarController>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("zielbereich"))
        {
            //Debug.Log("LadeBild");

            sharedData.SetPlayerControl(false);
            //carControl.setPlayerControl(false);

            PickupLogic pul = this.GetComponent<PickupLogic>();
            pul.stopTimer();
            coinCount = this.GetComponent<PickupLogic>().CoinCount;
            timeNeeded = pul.time;

            coinCountText.text = coinCount.ToString();
            //timeNeededText.text = timeNeeded.ToString();
            timeNeededText.text = pul.TimerText.text;
            endbildschirm.enabled = true;
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
        Application.LoadLevel(0);
    }
}
