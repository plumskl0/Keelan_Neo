using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;

public class ManageMenu : MonoBehaviour {

    public Transform mainMenu, optionsMenu, calibMenu, missionMenu;
    public GameObject mouseBorder, wiimoteBorder;
    public Text controllerText;
    private wiiKalibrierung wiiDaten;
    private GameObject[] menuArray;

    public void Start()
    {
        menuArray = GameObject.FindGameObjectsWithTag("Menu");

        //prüfe ob Wiimote angeschlossen ist, Maussteuerung als Voreinstellung
        wiiDaten = GameObject.Find("wiiMote").GetComponent<wiiKalibrierung>();
        Debug.Log("nach if");
        wiiDaten.findWiimote();
        if (wiiDaten.getWiimoteCount() == 0)
        {
            mouseBorder.GetComponent<Image>().color = Color.yellow;
            wiimoteBorder.GetComponent<Image>().color = Color.black;
            controllerText.text = "Maussteuerung ausgewählt! Schließe eine Wiimote an um zu wechseln.";
        }
        else
        {
            Debug.Log("in else");
            mouseBorder.GetComponent<Image>().color = Color.yellow;
            wiimoteBorder.GetComponent<Image>().color = Color.green;
            controllerText.text = "Maussteuerung ausgewählt! Aber eine Wiimote wurde gefunden. Klicken sie hier zur Aktivierung.";
        }
    }

    public void switchToWiimoteControl(bool clicked)
    {
        if (clicked == true)
        {
            calibMenu.gameObject.SetActive(clicked);
            mainMenu.gameObject.SetActive(false);
            wiiDaten.setFindWiimoteText();
        }
        else
        {
            optionsMenu.gameObject.SetActive(clicked);
            mainMenu.gameObject.SetActive(true);
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

    public void MissionMenu(bool clicked)
    {
        if (clicked == true)
        {
            missionMenu.gameObject.SetActive(clicked);
            mainMenu.gameObject.SetActive(false);
        }
        else
        {
            missionMenu.gameObject.SetActive(clicked);
            mainMenu.gameObject.SetActive(true);
        }
    }

    
    public void BackToMainMenu()
    {
        foreach (GameObject ob in menuArray)  {
            ob.gameObject.SetActive(false);
            mainMenu.gameObject.SetActive(true);
        }
    }


}
