using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WiimoteApi;

public class PlateController : MonoBehaviour {

    //GameObject die Position von Teller und Ball während des Spiels anzeigen; Vermutung: sind nicht zentiert
    public GameObject tellerPosSphere;
    public GameObject ballPosSphere;

    //public float angle = 25f;
    public int tellerSmoothinFactor = 3;    //glättet Messwerte über die angegebene Anzahl von Frames
    public int minAenderungswinkel = 3;     //nur wenn sich die Neigung, um mindestens diesen Winkel aendert wird die Tellerneigung veraendert

    Rigidbody myRigidbody;
    public static Wiimote wiiRemote;

    //public Transform plateTransform;
    private Transform plateTransform;
    private Transform ballTransform; //für debugzwecke -> zeige Pos von teller und Ball

    private float y;
    private float z;

    private bool mouseSteering = true;

    private float mouseX;
    private float mouseY;
    private GameObject wiiRemoteRef;
    private wiiKalibrierung wiiDaten;
    private Boolean playerControl;
    private SharedFields sharedData = SharedFields.Instance;

    // Use this for initialization
    void Start () {
        plateTransform = GetComponent<Transform>();
        ballTransform = GameObject.Find("Golfball_G").GetComponent<Transform>();
        sharedData.assistantPlateXAchse = 0.5f;
        Debug.Log(sharedData.assistantPlateXAchse + "   ist xWert der Platte bei Start");
        //sharedData.assistantPlateZAchse = 0.5f;

        //playerControl = GetComponent<AlternateCarController>().getPlayerControl();
        if (GameObject.Find("wiiMote") != null) //beim debuggen ist sonst wiiMote null
        {
            wiiDaten = GameObject.Find("wiiMote").GetComponent<wiiKalibrierung>();
            wiiRemote = wiiDaten.wiiRemote;
        }
        else
        {
            wiiDaten = null;
            wiiRemote = null;
        }

        //wiiDaten = GetComponent<wiiKalibrierung>();
        //wiiRemote = wiiDaten.wiiRemote;
    }

    //Debug Objekte um im Editor die Ball und Tellerposition anzuzeigen
    /*private void Update()
    {
        ballPosSphere.transform.position = ballTransform.position;
        //Debug.Log("Ball Position:" + ballTransform.position);
        tellerPosSphere.transform.position = plateTransform.position;
    }*/


    //GG Zittern:
    //Anstatt die Neigung des Tellers jeden Frame zu ändern bestimmt tellerSmoothinFactor die Häufigkeit
    private int frameCount = 0; //Zähler für Frames -> Neigungsänderung wenn tellerSmoothinFactor erreicht
    private float frameSummeZ = 0;
    private float frameSummeX = 0;

    private void FixedUpdate()
    {
        //Debug.Log(sharedData.assistantPlateXAchse + "   ist xWert der Platte bei Update");
        if (sharedData.GetPlayerControl())
        {
            if (sharedData.plateAutopilot || sharedData.TrainingMode)
            {
                //wird direkt in Plate Agent gemacht, das Hauptauto schreibt seine Achsenbelgungen in sharedData für einen späteren Mode Wechsel
                //plateTransform.localRotation = Quaternion.Euler(sharedData.assistantPlateXAchse * sharedData.plateMaxAngle, 0f, sharedData.assistantPlateZAchse * sharedData.plateMaxAngle);
                //Debug.Log(plateTransform.localRotation.eulerAngles.x);
                
            }
            else    //falls kein Autopilot eingeschaltet ist, greift die im Menu gewählte Steuerung
            {
                if (sharedData.SelectedControl == SharedFields.WiiControl && wiiRemote != null)
                {
                    float accel_x;
                    float accel_y;
                    float accel_z;

                    Vector3 accel = wiiDaten.GetAccelVector();
                    accel_x = accel[0];
                    accel_z = accel[2];

                    accel_y = accel[1];


                    //Ueberspringe #Tiltaenderungen definiert durch tellerSmoothingFactor
                    //Berechne den Mittelwert der uebersprungenen Werte

                    frameSummeZ += -accel_z;
                    frameSummeX += -accel_x;
                    //Debug.Log("frameSummeZ: " + frameSummeZ);
                    //Debug.Log("frameSummeX: " + frameSummeX);
                    frameCount++;


                    //Ansonsten ändere Tilt
                    if (!(frameCount < tellerSmoothinFactor))
                    {
                        //Cage nach links und rechts kippen
                        float z = (frameSummeZ / tellerSmoothinFactor) * 90;
                        Debug.Log(z);
                        //cage.localRotation = Quaternion.Euler(0f, 0f, z);

                        //Cage nach vorne und hinten kippen
                        float x = (frameSummeX / tellerSmoothinFactor) * 90;
                        //Debug.Log("Verändere Tellerwinkel");
                        //cage.localRotation = Quaternion.Euler(0f, 0f, z);

                        //Ignoriere Ausreiser indem nur Neigungen berücksichtigt werden, welche den Winkel um mindestens x Grad veraendert
                        //float aktuelleNeigungX = plateTransform.eulerAngles.x -360f;
                        //float aktuelleNeigungZ = plateTransform.eulerAngles.z- 360f;
                        //float aktuelleNeigungX = plateTransform.
                        float aktuelleNeigungX = plateTransform.localEulerAngles.x;
                        float aktuelleNeigungZ = plateTransform.localEulerAngles.z;
                        //Debug.Log(aktuelleNeigungZ);

                        if (aktuelleNeigungX < 90)
                        {
                            aktuelleNeigungX = aktuelleNeigungX;
                        }
                        if (aktuelleNeigungX > 270)
                        {
                            aktuelleNeigungX = aktuelleNeigungX - 360;
                        }


                        if (aktuelleNeigungZ < 90)
                        {
                            aktuelleNeigungZ = aktuelleNeigungZ;
                        }
                        if (aktuelleNeigungZ > 270)
                        {
                            aktuelleNeigungZ = aktuelleNeigungZ - 360;
                        }



                        //Debug.Log(aktuelleNeigungX);
                        //Debug.Log(aktuelleNeigungZ);
                        if (Math.Abs(aktuelleNeigungX - x) > minAenderungswinkel || Math.Abs(aktuelleNeigungZ - z) > minAenderungswinkel)
                        {
                            plateTransform.localRotation = Quaternion.Euler(x, 0f, z);
                            //Debug.Log("habe versucht Neigung zu aendern");
                            //Debug.Log(wiiKalibrierung.wiiRemote.GetAccelVector());
                        }

                        //Setze Mittelwert und Frame Count auf 0 fuer naechste Runde auf 0
                        frameSummeX = 0;
                        frameSummeZ = 0;
                        frameCount = 0;
                    }
                }
                else if (sharedData.SelectedControl == SharedFields.MTControl)
                {
                    // Mausbewegung
                    mouseX += Input.GetAxis("Mouse X") * sharedData.sensitivity;
                    mouseY += Input.GetAxis("Mouse Y") * sharedData.sensitivity;

                    mouseY = Mathf.Clamp(mouseY, -sharedData.plateMaxAngle, sharedData.plateMaxAngle);
                    mouseX = Mathf.Clamp(mouseX, -sharedData.plateMaxAngle, sharedData.plateMaxAngle);

                    // Winkel min und max einstellen mit Clamp
                    y = mouseY;
                    z = mouseX;

                    // Teller bewegen
                    plateTransform.localRotation = Quaternion.Euler(y, 0f, z);
                }
                /*else if (sharedData.SelectedControl == SharedFields.VoiceAssistantControl)
                {
                    //plateTransform.localRotation = Quaternion.Euler(sharedData.assistantPlateXAchse* angle, 0f, -sharedData.assistantPlateZAchse*angle);
                }*/

                else
                {
                    //Ansonsten steuere Neigung ueber Tastatur
                    y = Input.GetAxis("VerticalPlate") * sharedData.plateMaxAngle;
                    z = Input.GetAxis("HorizontalPlate") * sharedData.plateMaxAngle;

                    plateTransform.localRotation = Quaternion.Euler(y, 0f, z);
                }

                // Evtl. einbauen aber muss noch besprochen werden
                //if (sharedData.CarReset)
                //    resetPlateRotation();
            }
        }
    }

    private void resetPlateRotation()
    {
         
        mouseX = 0;
        mouseY = 0;
        y = 0;
        z = 0;
    }

}

