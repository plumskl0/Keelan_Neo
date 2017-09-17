using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;
using System.Text;
using System;

public class wiiKalibrierung : MonoBehaviour {

    public Wiimote wiiRemote;
    private Vector2 buttonMovement;
    private Vector3 accelData;

    public Canvas menu;
    public int calibStep = 0;
    public Text findWiimoteText;
    public Text calibText;
    public Text accelPlaceholder;
    private SharedFields sharedData = SharedFields.Instance;

    private void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }
    // Use this for initialization
    void Start () {
		
	}
    public Vector3 GetAccelVector()
    {
        return accelData;
    }

    //liefert moveVertical und moveHorizontal
    public Vector2 getButtons()
    {
        return buttonMovement;
    }

    public const String no = "None";
    public const String up = "UpWasPressedLastFrame";
    public const String down = "DownWasPressedLastFrame";
    public const String left = "LeftWasPressedLastFrame";
    public const String right = "RightWasPressedLastFrame";


    private String lastVerticalAxisButtonPressend = no;
    private String lastHorizontalAxisButtonPressend = no;
    float moveHorizontal = 0;
    float moveVertical = 0;
    public float stepsToAxisMax = 0.01f;

    // Update is called once per frame
    void FixedUpdate () {
        if (wiiRemote != null)
        {
            //Debug.Log("WiiRemote = " + wiiRemote);
            int ret;
            do
            {
                ret = wiiRemote.ReadWiimoteData();

            } while (ret > 0);


            //Bestimme Tastenwerte
            //float moveHorizontal = 0;
            //float moveVertical = 0;
            bool axisButtonPressedThisFrame = false;



            if (wiiRemote.Button.d_down)
            {
                if (lastVerticalAxisButtonPressend != down)
                {
                    moveVertical = 0;
                }

                else if (moveVertical > -1)
                {
                    // Debug.Log("move")
                    moveVertical -= stepsToAxisMax;
                }
                else
                {
                    Debug.Log("dDown");
                    moveVertical = -1;
                }
                lastVerticalAxisButtonPressend = down;
                axisButtonPressedThisFrame = true;
                Debug.Log("moveVertical ist jetzt: " + moveVertical);

            }
            if (wiiRemote.Button.d_up)
            {
                if (lastVerticalAxisButtonPressend != up)
                {
                    moveVertical = 0;
                }

                else if (moveVertical < 1)
                {
                    // Debug.Log("move")
                    moveVertical += stepsToAxisMax;
                }
                else
                {
                    Debug.Log("dUp");
                    moveVertical = 1;
                }
                lastVerticalAxisButtonPressend = up;
                axisButtonPressedThisFrame = true;
                Debug.Log("moveVertical ist jetzt: " + moveVertical);
            }

            if (wiiRemote.Button.d_left)
            {
                if (lastHorizontalAxisButtonPressend != left)
                {
                    moveHorizontal = 0;
                }

                else if (moveHorizontal > -1)
                {
                    // Debug.Log("move")
                    moveHorizontal -= stepsToAxisMax;
                }
                else
                {
                    Debug.Log("dleft");
                    moveHorizontal = -1;
                }
                lastHorizontalAxisButtonPressend = left;
                axisButtonPressedThisFrame = true;
                Debug.Log("moveHorizontal ist jetzt: " + moveHorizontal);
            }

            if (wiiRemote.Button.d_right)
            {
                if (lastHorizontalAxisButtonPressend != right)
                {
                    moveHorizontal = 0;
                }

                else if (moveHorizontal < 1)
                {
                    // Debug.Log("move")
                    moveHorizontal += stepsToAxisMax;
                }
                else
                {
                    Debug.Log("dleft");
                    moveHorizontal = 1;
                }
                lastHorizontalAxisButtonPressend = right;
                axisButtonPressedThisFrame = true;
                Debug.Log("moveHorizontal ist jetzt: " + moveHorizontal);
            }
            if (!axisButtonPressedThisFrame)
            {
                Debug.Log("Setze Achsen zurück");
                lastVerticalAxisButtonPressend = no;
                moveHorizontal = 0;
                moveVertical = 0;
            }

            buttonMovement = new Vector2(moveHorizontal, moveVertical);

            //bestimme Accel Daten
            float accel_x;
            float accel_y;
            float accel_z;

            float[] accel = wiiRemote.Accel.GetCalibratedAccelData();
            accel_x = accel[0];
            accel_y = accel[2];
            accel_z = accel[1];
            accelData = new Vector3(accel_x, accel_y, accel_z);


            if (menu != null)
            {
                accelPlaceholder.text = wiiRemote.Accel.GetCalibratedAccelData().ToString();

                Debug.Log("Wiimote Daten: " + accelData.ToString());
                accelPlaceholder.text = accelData.ToString();
                //Debug.Log("x: " + accel.x);
                //Debug.Log(accel.x + ";" + accel.y + ";" + accel.z);


                //Debug.Log(this.GetAccelVector());
            }
        }
    }

    public static int count = 0;

    public int CalibStep
    {
        get
        {
            return calibStep;
        }

        set
        {
            calibStep = value;
        }
    }

    public void findWiimote()
    {
        WiimoteManager.FindWiimotes(); // Poll native bluetooth drivers to find Wiimotes

        foreach (Wiimote remote in WiimoteManager.Wiimotes)
        {
            // Do stuff.
            //Console.WriteLine("Gefunden");

            //LEDs markieren, dass WiiRemote verbunden ist
            wiiRemote = remote;
            //remote.RumbleOn = true;
            wiiRemote.SendStatusInfoRequest();
            wiiRemote.SendPlayerLED(true, false, false, false);
            //Thread.Sleep(1000);
            //remote.RumbleOn = false;
            wiiRemote.SendStatusInfoRequest();
            wiiRemote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL);
            count++;
        }
        if (count != 0)
        {
            /*if (GameObject.Find("CalibMenu").gameObject.activeSelf)
            {
                setFindWiimoteText();
                //Debug.Log("START ---------- Wiimote gefunden");
                //Debug.Log("Wiimote Daten: " + wiiRemote.Accel.GetCalibratedAccelData().ToString());
            }*/
        }
            /*return true;
        }
        else
        {
            return false;
        }*/
    }

    public int getWiimoteCount()
    {
        return count;
    }

    public void setFindWiimoteText()
    {
        if (count > 0)
        {
            findWiimoteText.text = ("Habe folgende Anzahl Wiimotes gefunden: " + count + ". \nFahren sie mit der Kalibrierung fort.");
        }
        else
        {
            findWiimoteText.text = ("Habe folgende Anzahl Wiimotes gefunden: " + count + ". \nDrücken sie den Find Wiimote Button, um erneut zu suchen.");
        }
    }

    public void calibWiimote()
    {
        StringBuilder calibInfo = new StringBuilder();
        if (CalibStep <= 3)
        {
            AccelCalibrationStep step = (AccelCalibrationStep)CalibStep;
            wiiRemote.Accel.CalibrateAccel(step);

            //Je nachdem, welcher Step gerade ausgeführt wurde wird ein
            //  entsprechender Infotext angezeigt
            calibInfo.Append(step.ToString() + " ausgeführt. ");
            switch (CalibStep)
            {
                case 0:
                    calibInfo.Append("Stellen sie den IR Sensor der Wiimote nun auf den Tischen\n, sodass der Extenstion Port nach oben zeigt.");
                    break;
                case 1:
                    calibInfo.Append("Im letzten Schritt legen sie die Wiimote auf die Seite. \nDie linke Seite zeigt dabei nach oben. ");
                    break;
                case 2:
                    calibInfo.Append("Wir sind fertig! Das Spiel kann nun gestartet werden.");
                    sharedData.SelectedControl = SharedFields.WiiControl;
                    break;
            }
            CalibStep++;
            calibText.text = calibInfo.ToString();
        }
    }

    public void QuitGame()
    {
        if (wiiRemote != null)
        {
            WiimoteManager.Cleanup(wiiRemote);
            wiiRemote = null;
        }
        Application.Quit();
    }

    public void loadGame()
    {
        Application.LoadLevel(1);
    }
}
