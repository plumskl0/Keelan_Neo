using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;

public class ManageMenu : MonoBehaviour {

    public GameObject mouseBorder, wiimoteBorder;
    public Text controllerText;
    private wiiKalibrierung wiiDaten;
    public Transform mainMenu, optionsMenu, calibMenu, missionMenu, keysMenu, wiimoteNotCalibratedPanel, changeButtonPanel;
    private SharedFields sharedData = SharedFields.Instance;

    public void Start()
    {
        //prüfe ob Wiimote angeschlossen ist, Maussteuerung als Voreinstellung
        wiiDaten = GameObject.Find("wiiMote").GetComponent<wiiKalibrierung>();
        Debug.Log("nach if");
        wiiDaten.findWiimote();
        SetControlerImages();
        //manageInstance = GameObject.Find("LevelManger").GetComponent<ManageMenu>();

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


    

    /*public void ChangeKeyTemporary(int directionCode)   //int definiert welche Steuertaste überschrieben werden soll
    {
        Boolean keyPressed = false;
        Debug.Log("InMethode");
        KeyCode tmpKey = KeyCode.A; //Standardinitialisierung 
        Debug.Log("setze Panel");
        changeButtonPanel.gameObject.SetActive(true);
        while (!keyPressed)
        {
            Debug.Log("Starte Schleife");
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
            {
                Debug.Log("Durchlaufe!");
                if (Input.GetKey(vKey))
                {
                    Debug.Log("Taste erkannt");
                    Debug.Log("key detected");
                    tmpKey = vKey;
                    keyPressed = true;
                }
            }
        }
        tmpMouseControls[directionCode] = tmpKey;
    }*/

    static KeyCode tmpKey;
    static int keyStrokeCount = 0;

    public static int KeyStrokeCount
    {
        get
        {
            return keyStrokeCount;
        }

        set
        {
            keyStrokeCount = value;
        }
    }

    public static KeyCode TmpKey
    {
        get
        {
            return tmpKey;
        }

        set
        {
            tmpKey = value;
        }
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey)
        {
            //Debug.Log("Detected key code: " + e.keyCode);
            //Debug.Log("keystrokecount: " + KeyStrokeCount);
            TmpKey = e.keyCode;
            KeyStrokeCount++;
            if (changeButtonPanel.gameObject.activeSelf == true)
            {
                changeButtonPanel.gameObject.SetActive(false);
            }
        }

    }

  /*  public static void ChangeKeyTestEnde()   //int definiert welche Steuertaste überschrieben werden soll
    {
        
        Debug.Log("Beende Tastensetzen");
       levelManager.GetComponent<ManageMenu>.changeButtonPanel.gameObject.SetActive(false);
        manageInstance.tmpMouseControls[0] = manageInstance.tmpKey;
        Debug.Log("Egebnis = " + manageInstance.tmpMouseControls[0]);
    }*/

    public void ChangeKeyTemporary(int directionCode)   //int definiert welche Steuertaste überschrieben werden soll
    {
        int lastKeyStrokeCount = KeyStrokeCount;
        Debug.Log("setze Panel");
        changeButtonPanel.gameObject.SetActive(true);
        Debug.Log("Detecting Keystroke");

        ChangeKeyWorker workerObject = new ChangeKeyWorker();
        Thread workerThread = new Thread(workerObject.DoWork);
        workerThread.Start((object)new KeySettingInfomation(directionCode, lastKeyStrokeCount));
    }

    public void ChangeMouseSensitivity()
    {
        sharedData.sensitivity = GameObject.Find("MouseSensitivitySlider").GetComponent<Slider>().value;
        Debug.Log("Sensitivity = " + sharedData.sensitivity);
    }


    public void SaveControllerSettings()
    {
        //übertrage tmpArray in die Speicherwerte
        for (int i = 0; i < 5; i++)
        {
            if (sharedData.TmpMouseControls[i]!= KeyCode.None)
            {
                switch (i)
                {
                    case 0:
                        sharedData.TUpKey = sharedData.TmpMouseControls[i];
                        break;
                    case 1:
                        sharedData.TDownKey = sharedData.TmpMouseControls[i];
                        break;
                    case 2:
                        sharedData.TUpKey = sharedData.TmpMouseControls[i];
                        break;
                    case 3:
                        sharedData.TUpKey = sharedData.TmpMouseControls[i];
                        break;
                    case 4:
                        sharedData.TResetKey = sharedData.TmpMouseControls[i];
                        break;
                    default:
                        Debug.Log("Da lief was schief.");
                        break;
                }
            }
        }
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
