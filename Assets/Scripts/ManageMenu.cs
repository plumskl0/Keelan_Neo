using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;

public class ManageMenu : MonoBehaviour {

    public GameObject mouseBorder, wiimoteBorder;
    public Text controllerText;
    private wiiKalibrierung wiiDaten;
    public Transform mainMenu, optionsMenu, calibMenu, missionMenu, keysMenu, wiimoteNotCalibratedPanel;
    private SharedFields sharedData = SharedFields.Instance;

    public void Start()
    {
        //prüfe ob Wiimote angeschlossen ist, Maussteuerung als Voreinstellung
        wiiDaten = GameObject.Find("wiiMote").GetComponent<wiiKalibrierung>();
        Debug.Log("nach if");
        wiiDaten.findWiimote();
        SetControlerImages(); 

    }

    public void SetControlerImages() //prüft welche Steuerung gewählt ist und setzt die Images auf dem Panel dementsprechend
    {
        mouseBorder = GameObject.Find("MouseBorder");
        wiimoteBorder = GameObject.Find("WiimoteBorder");
        if (wiiDaten.getWiimoteCount() == 0)
        {
            mouseBorder.GetComponent<Image>().color = Color.yellow;
            wiimoteBorder.GetComponent<Image>().color = Color.black;
            sharedData.SelectedControl = SharedFields.MTControl;
            controllerText.text = "Maussteuerung ausgewählt! Schließe eine Wiimote an um zu wechseln.";
        }
        else
        {
            if (sharedData.SelectedControl == SharedFields.MTControl)
            {
                mouseBorder.GetComponent<Image>().color = Color.yellow;
                wiimoteBorder.GetComponent<Image>().color = Color.green;
                controllerText.text = "Maussteuerung ausgewählt! Aber eine Wiimote wurde gefunden. Klicken sie hier zur Aktivierung.";
            }
            else //-> Wiimote vorhanden und ausgewählt
            {
                mouseBorder.GetComponent<Image>().color = Color.black;
                wiimoteBorder.GetComponent<Image>().color = Color.yellow;
                if (wiiDaten.CalibStep <= 1)    //prüfe ob alle Calib Schritte ausgeführt wurden
                {
                    controllerText.text = "Wiimote Steuerung ausgewählt. Aber die Kalibrierung wurde noch nicht abgeschlossen. Klicke hier, um sie abzuschließen oder auf die Maus um zur Tastatur und Maus Steuerung zu wechseln.";
                }
                else
                {
                    controllerText.text = "Wiimote Steuerung ausgewählt und kalibriert. Sie können das Spiel nun starten.";
                }
            }
        }
    }

    public void SaveControllerSettings()
    {

    }

    public void SwitchToWiimoteControl()
    {
            sharedData.SelectedControl = SharedFields.WiiControl;
            DisableCurrentMenu();
            calibMenu.gameObject.SetActive(true);
            wiiDaten.setFindWiimoteText();
    }

    public void SwitchToMTControl()
    {
        sharedData.SelectedControl = SharedFields.MTControl;
        if (wiiDaten.getWiimoteCount() == 0)
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

    public void SwitchToMTAndLoadGame()
    {
        SwitchToMTControl();
        LoadScene("Level1");
    }

    public void SwitchToKeysMenu()
    {
        DisableCurrentMenu();
        keysMenu.gameObject.SetActive(true);
        SetControlerImages();
    }

    public void LoadScene(string name)
    {
        //starte das Spiel nicht, wenn Wiimote ausgewählt wurde, aber nicht komplett kalibriert
        if (sharedData.SelectedControl == SharedFields.WiiControl && wiiDaten.CalibStep <= 1)
        {
            wiimoteNotCalibratedPanel.gameObject.SetActive(true);
        }
        //hier muss eine Pause rein bis geladen wird
        else
        {
            Application.LoadLevel(name);
        }
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


    private void DisableCurrentMenu()
    {
        foreach (GameObject ob in GameObject.FindGameObjectsWithTag("Menu"))
        {
            ob.gameObject.SetActive(false);
        }
    }
    public void BackToMainMenu()
    {
        DisableCurrentMenu();
        mainMenu.gameObject.SetActive(true);
        SetControlerImages();
    }


}
