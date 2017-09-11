using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;

public class ManageMenu : MonoBehaviour {

    public Transform mainMenu, optionsMenu, calibMenu;
    public GameObject mouseBorder, wiimoteBorder;
    public Text controllerText;

    public void Start()
    {
        //prüfe ob Wiimote angeschlossen ist, Maussteuerung als Voreinstellung
        if (WiimoteManager.FindWiimotes() != true)
        {
            mouseBorder.GetComponent<Image>().color = Color.yellow;
            wiimoteBorder.GetComponent<Image>().color = Color.black;
            controllerText.text = "Maussteuerung ausgewählt! Schließe eine Wiimote an um zu wechseln.";
        }

        else
        {
            mouseBorder.GetComponent<Image>().color = Color.yellow;
            wiimoteBorder.GetComponent<Image>().color = Color.green;
            controllerText.text = "Maussteuerung ausgewählt! Aber eine Wiimote wurde gefunden. Klicken sie hier zur Aktivierung.";
        }
    }

    public void LoadScene(string name)
    {
        Application.LoadLevel(name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OptionsMenu (bool clicked)
    {
        if (clicked == true)
        {
            optionsMenu.gameObject.SetActive(clicked);
            mainMenu.gameObject.SetActive(false);
        }
        else
        {
            optionsMenu.gameObject.SetActive(clicked);
            mainMenu.gameObject.SetActive(true);
        }
    }


}
