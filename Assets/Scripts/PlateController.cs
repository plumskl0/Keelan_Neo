﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WiimoteApi;

public class PlateController : MonoBehaviour {

    public float angle = 25f;
    public int tellerSmoothinFactor = 3;    //glättet Messwerte über die angegebene Anzahl von Frames
    public int minAenderungswinkel = 3;     //nur wenn sich die Neigung, um mindestens diesen Winkel aendert wird die Tellerneigung veraendert
    Rigidbody myRigidbody;
    public static Wiimote wiiRemote;

    //public Transform plateTransform;
    private Transform plateTransform;

    private float y;
    private float z;

    // Use this for initialization
    void Start () {
        plateTransform = GetComponent<Transform>();
        wiiRemote = CarController.wiiRemote;
	}


    //GG Zittern:
    //Anstatt die Neigung des Tellers jeden Frame zu ändern bestimmt tellerSmoothinFactor die Häufigkeit
    private int frameCount = 0; //Zähler für Frames -> Neigungsänderung wenn tellerSmoothinFactor erreicht
    private float frameSummeZ = 0;
    private float frameSummeX = 0;

    private void FixedUpdate()
    {
        if (wiiRemote != null)
        {
            int ret;
            do
            {
                ret = wiiRemote.ReadWiimoteData();

            } while (ret > 0);

            float accel_x;
            float accel_y;
            float accel_z;

            float[] accel = wiiRemote.Accel.GetCalibratedAccelData();
            accel_x = accel[0];
            accel_y = -accel[2];
            accel_z = accel[1];

            //Transform cage = transform.Find("Cage");
            //Transform plateTransform = this;

            //Ueberspringe #Tiltaenderungen definiert durch tellerSmoothingFactor
            //Berechne den Mittelwert der uebersprungenen Werte

            frameSummeZ += -accel_z;
            frameSummeX += -accel_x;
            Debug.Log("frameSummeZ: " + frameSummeZ);
            Debug.Log("frameSummeX: " + frameSummeX);
            frameCount++;


            //Ansonsten ändere Tilt
            if (!(frameCount < tellerSmoothinFactor))
            {
                //Cage nach links und rechts kippen
                float z = (frameSummeZ / tellerSmoothinFactor) * 90;
                //cage.localRotation = Quaternion.Euler(0f, 0f, z);

                //Cage nach vorne und hinten kippen
                float x = (frameSummeX / tellerSmoothinFactor) * 90;
                Debug.Log("Verändere Tellerwinkel");
                //cage.localRotation = Quaternion.Euler(0f, 0f, z);

                //Ignoriere Ausreiser indem nur Neigungen berücksichtigt werden, welche den Winkel um mindestens x Grad veraendert
                float aktuelleNeigungX = plateTransform.eulerAngles.x -360f;
                float aktuelleNeigungZ = plateTransform.eulerAngles.z- 360f; 

                if (Math.Abs(aktuelleNeigungX - x) > minAenderungswinkel || Math.Abs(aktuelleNeigungZ - z) > minAenderungswinkel)
                {
                    plateTransform.localRotation = Quaternion.Euler(x, 0f, z);
                    Debug.Log("habe versucht Neigung zu aendern");
                    //Debug.Log(wiiKalibrierung.wiiRemote.GetAccelVector());
                }

                //Setze Mittelwert und Frame Count auf 0 fuer naechste Runde auf 0
                frameSummeX = 0;
                frameSummeZ = 0;
                frameCount = 0;
            }
        }

        //Ansonsten steuere Neigung ueber Tastatur
        else
        {
            y = Input.GetAxis("VerticalPlate") * angle;
            z = Input.GetAxis("HorizontalPlate") * angle;

            plateTransform.localRotation = Quaternion.Euler(y, 0f, z);
        }

    }

}

