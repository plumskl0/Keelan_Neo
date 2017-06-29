using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WiimoteApi;
using System.Text;

public class wiiKalibrierung : MonoBehaviour {

    public static Wiimote wiiRemote;

    public Canvas menu;
    private int calibStep = 0;
    public Text findWiimoteText;
    public Text calibText;
    public Text accelPlaceholder;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (wiiRemote != null)
        {
            int ret;
            do
            {
                ret = wiiRemote.ReadWiimoteData();

            } while (ret > 0);
            accelPlaceholder.text = wiiRemote.Accel.GetCalibratedAccelData().ToString();

            //Debug.Log(wiiRemote.Accel.GetCalibratedAccelData().ToString());
            Vector3 accel = this.GetAccelVector();
            accelPlaceholder.text = this.GetAccelVector().ToString();
            //Debug.Log("x: " + accel.x);
            Debug.Log(accel.x + ";" + accel.y + ";" + accel.z);


            //Debug.Log(this.GetAccelVector());
        }
    }
    private Vector3 GetAccelVector()
    {
        float accel_x;
        float accel_y;
        float accel_z;

        float[] accel = wiiRemote.Accel.GetCalibratedAccelData();
        accel_x = accel[0];
        accel_y = accel[2];
        accel_z = accel[1];
        //debug.log("x: " + accel_x);
        //debug.log("y: " + accel_y);
        //debug.log("z: " + accel_z);
        //return new Vector3(accel_x, accel_y, accel_z).normalized;
        return new Vector3(accel_x, accel_y, accel_z);
    }

    public void findWiimote()
    {
        WiimoteManager.FindWiimotes(); // Poll native bluetooth drivers to find Wiimotes
        int count = 0;
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
        if (count !=0)
        {
            findWiimoteText.text = ("Habe folgende Anzahl Wiimotes gefunden: " + count + ". \nFahren sie mit der Kalibrierung fort.");
        }
    }

    public void calibWiimote()
    {
        StringBuilder calibInfo = new StringBuilder();
        if (calibStep <= 3)
        {
            AccelCalibrationStep step = (AccelCalibrationStep)calibStep;
            wiiRemote.Accel.CalibrateAccel(step);

            //Je nachdem, welcher Step gerade ausgeführt wurde wird ein
            //  entsprechender Infotext angezeigt
            calibInfo.Append(step.ToString() + " ausgeführt. ");
            switch (calibStep)
            {
                case 0:
                    calibInfo.Append("Stellen sie den IR Sensor der Wiimote nun auf den Tischen\n, sodass der Extenstion Port nach oben zeigt.");
                    break;
                case 1:
                    calibInfo.Append("Im letzten Schritt legen sie die Wiimote auf die Seite. \nDie linke Seite zeigt dabei nach oben. ");
                    break;
                case 2:
                    calibInfo.Append("Wir sind fertig! Das Spiel kann nun gestartet werden.");
                    break;
            }
            calibStep++;
            calibText.text = calibInfo.ToString();
        }
        /*for (int x = 0; x < 3; x++)
        {

            AccelCalibrationStep step = (AccelCalibrationStep)x;

            //folgendes erzeugt drei Buttons (-> einen pro Calibration Step)
            //  -> liefern true wenn der Nutzer darauf drückt
            //  -> daraufhin startet die ausgewählte Kalibrierung
            if (GUILayout.Button(step.ToString(), GUILayout.Width(100)))
            {
                wiiRemote.Accel.CalibrateAccel(step);
            }
        }*/
    }

    public void loadGame()
    {
        Application.LoadLevel(1);
    }
}
