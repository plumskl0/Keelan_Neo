﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using WiimoteApi;

public class AlternateCarController : MonoBehaviour
{
    public float maxWheelAngle = 30f;
    public float maxTorque = 300f;
    public float brakeTorque = 30000f;

    //public float maxSpeed = 30f;

    public Transform[] wheels;

    private const int FRONT_LEFT = 0;
    private const int FRONT_RIGHT = 1;
    private const int REAR_RIGHT = 2;
    private const int REAR_LEFT = 3;

    private float criticalSpeed = 5f;
    private int stepsBelow = 5;
    private int stepsAbove = 1;
    public int frameCountThisTrainingRoute = 0;
    public int frameDurationThisRoute = 0;

    string dirPathTrainingRoute = "Assets/TrainingRoutes/"; //Ordner in dem die Trainingsrouten liegen
    int dirFileCount;
    private PlateAgent myPlateAgent;
    List<FileInfo> trainingFiles = new List<FileInfo>();
    private Dictionary<int, Vector3> trainingsFahrroute = new Dictionary<int, Vector3>();
    private Dictionary<int, Vector3> trainingsFahrrouteSaved = new Dictionary<int, Vector3>();

    private StringBuilder stringBuilder = new StringBuilder();

    private Rigidbody rb;

    private Boolean playerControl;
    private GameObject wiiMoteRef;
    private wiiKalibrierung wiiDaten;
    public Wiimote wiiRemote;
    private ResetCar resetCarScript;
    private SharedFields sharedData = SharedFields.Instance;

    //Training und Debug Möglichkeiten per Editor einschalten:
    public bool activateTrainingMode = false;
    public bool activateDebugMode = false;
    public float maxSpeed = 30f;


    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        myPlateAgent = gameObject.GetComponent<PlateAgent>();
        resetCarScript = GetComponent<ResetCar>();

        //Sollte nur von Player Auto übernommen werden
        if (!myPlateAgent.isTrainingCar)
        {
            sharedData.TrainingMode = activateTrainingMode;
            sharedData.debugMode = activateDebugMode;
            sharedData.currentMaxSpeed = maxSpeed;
            sharedData.maxSpeed = maxSpeed;
        }



        if (!myPlateAgent.isTrainingCar)
        {
            sharedData.SetCursorVisible(false);
            sharedData.SetPlayerControl(true);

            if (GameObject.Find("wiiMote") != null) //beim debuggen ist sonst wiiMote nullReferenz
            {
                wiiDaten = GameObject.Find("wiiMote").GetComponent<wiiKalibrierung>();
                wiiRemote = wiiDaten.wiiRemote;
            }
            else
            {
                wiiDaten = null;
                wiiRemote = null;
            }
        }

        DirectoryInfo dir = new DirectoryInfo(dirPathTrainingRoute);
        dirFileCount = dir.GetFiles().Length - dir.GetFiles("*.meta").Length;
        Debug.Log("init anzahl files: " + dirFileCount);
        foreach (FileInfo file in dir.EnumerateFiles())
        {
            if (!(file.Name.Contains("meta")))
            {
                Debug.Log("Füge File hinzu:" + file.Name);
                trainingFiles.Add(file);
                Debug.Log("Jetztiger File Count: " + trainingFiles.Count);
            }
        }

        //Lade eine Strecke, falls der Trainingsmodus aktiv ist
        if (sharedData.TrainingMode)
        {
            LoadTrainingRoute();
        }
    }

    private void LoadTrainingRoute()
    {
        trainingsFahrroute.Clear();
        foreach (int i in trainingsFahrroute.Keys)
        {
            Debug.LogFormat("Vorhandener Key: {0}", i);
        }
        if (dirFileCount == 0)
        {
            throw new Exception("Für das Training wurden keine Trainingsrouten gefunden");
        }
        int randomFileNumber = UnityEngine.Random.Range(0, dirFileCount);
        Debug.Log("Nehmen File Nummer: " + randomFileNumber + " mit Namen: " + trainingFiles[randomFileNumber].Name);
        //StreamReader read = new StreamReader(dirPathTrainingRoute + randomFileNumber);
        StreamReader read = new StreamReader(dirPathTrainingRoute + trainingFiles[randomFileNumber].Name);
        string s = read.ReadToEnd();
        string[] keyValueArray = s.Split(';');
        foreach (string t in keyValueArray)
        {
            if (!t.Equals(""))
            {
                int start = t.IndexOf("[");
                int laenge = t.IndexOf("]") - 1 - t.IndexOf("[");
                //Debug.LogFormat("string: {0} wird entnommen startindex: {1} und laenge {2}", t, start, laenge);

                string ohneKlammern = t.Substring(t.IndexOf("[") + 1, t.IndexOf("]") - 1 - t.IndexOf("["));
                //Debug.LogFormat("ohne Klammern: {0}", ohneKlammern);
                string[] keyAndValue = ohneKlammern.Split('|');
                string[] floatValues = keyAndValue[1].Substring(keyAndValue[1].IndexOf('(') + 1, keyAndValue[1].IndexOf(')') - 1 - keyAndValue[1].IndexOf('(')).Split('.');
                //Debug.LogFormat("float Values: {0}", floatValues);
                Vector3 finalValues = new Vector3(float.Parse(floatValues[0]), float.Parse(floatValues[1]), float.Parse(floatValues[2]));
                //Debug.Log("floatStrings converted to floats: " + finalValues);

                //string valueVector = 
                trainingsFahrroute.Add(Int32.Parse(keyAndValue[0]), finalValues);
            }
        }
        read.Close();
        int currentMax = 0;
        foreach (int n in trainingsFahrroute.Keys)
        {
            if (n > currentMax)
            {
                currentMax = n;
            }
        }
        frameDurationThisRoute = currentMax;
        Debug.Log("max Frame: " + frameDurationThisRoute);
    }

    public void Update()
    {/*
        if (!myPlateAgent.isTrainingCar)
        {
            //Debug.Log("****Füge neue Trainingsdaten Hinzu");
            //trainingsFahrrouteSaved.Add(Time.frameCount, new Vector3(moveHorizontal, moveVertical, handBrake));
            Debug.Log("Füge für Frame hinzu: " + frameCountThisTrainingRoute);
            trainingsFahrrouteSaved.Add(frameCountThisTrainingRoute, new Vector3(moveHorizontal, moveVertical, handBrake));
            //Debug.LogFormat("***FrameCountThisTrainingRoute: {0}, FrameCount: {1}", frameCountThisTrainingRoute, Time.frameCount);
            //sharedData.trainingsFahrroute.Add(Time.frameCount, new Vector3(moveHorizontal, moveVertical, handBrake));
        }*/
    }

    float moveHorizontal;   //die ausgewählte Steuerung setzt diese Werte
    float moveVertical;
    float handBrake;
    // Update is called once per frame
    void FixedUpdate()
    {
        frameCountThisTrainingRoute++;
        getCollider(FRONT_LEFT).ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);




        float angle;
        float torque;


        if (sharedData.GetPlayerControl())
        {
            // im Trainingsmodus verhalten sich alle Autos gleich, im Debug Mode soll aber nur das Hauptauto ein Training simulieren
            if (sharedData.TrainingMode)
            {
                if (!trainingsFahrroute.ContainsKey(frameCountThisTrainingRoute))
                {
                    //teile Auto ggf. mit, dass die Route fertig ist:
                    if (frameCountThisTrainingRoute > frameDurationThisRoute)
                    {
                        Debug.Log("Frame Count this Training Route = " + frameCountThisTrainingRoute);
                        Debug.Log("Traingsstrecke beendet... Agent Reset");

                        myPlateAgent.TrainingRouteFinished = true;
                        frameCountThisTrainingRoute = 0;
                        LoadTrainingRoute();
                        resetCarScript.CarReset(95.39f, 1.08926f, 30.4274f, false);
                        moveHorizontal = 0;
                        moveVertical = 0;
                        handBrake = brakeTorque;
                    }
                    else
                    {
                        Debug.LogError("Liste unvollständig");
                    }
                }
                else //ansonsten weise die gespeicherten Keystrokes zu:
                {
                    Vector3 controls = trainingsFahrroute[frameCountThisTrainingRoute];
                    moveHorizontal = controls.x;
                    moveVertical = controls.y;
                    handBrake = controls.z;
                }
            }

            //Car Autopilot Modus: -> nur Hauptauto fährt
            else if (sharedData.CarAutopilot && !myPlateAgent.isTrainingCar)
            {
                //Debug.LogFormat("Voice Assistant Y- Achse: {0}", sharedData.AssistantYAchse);
                moveHorizontal = sharedData.AssistantXAchse;
                moveVertical = sharedData.AssistantYAchse;
                handBrake = Input.GetKey(sharedData.TBrakeKey) ? brakeTorque : 0;   //todo: Assistant muss auch einen Bremswert setzen
            }

            // -> Fall: auto wird vom User gesteuert -> nur Hauptauto
            else
            {
                if (!myPlateAgent.isTrainingCar)    //Trainigsauto dürfen die geteilten Steuerungsdaten nicht ändern
                {
                    if (sharedData.SelectedControl == SharedFields.WiiControl && wiiRemote != null)
                    {
                        Vector2 buttonMovement = wiiDaten.getButtons();
                        moveHorizontal = buttonMovement.x;
                        moveVertical = buttonMovement.y;
                        handBrake = wiiRemote.Button.a ? brakeTorque : 0;

                    }

                    //...ansonsten nutzen die Tastatursteuerung
                    else if (sharedData.SelectedControl == SharedFields.MTControl)
                    {

                        Vector2 keyboardMovement = GetKeyboardButtons();
                        moveVertical = keyboardMovement.x;
                        moveHorizontal = keyboardMovement.y;
                        handBrake = Input.GetKey(sharedData.TBrakeKey) ? brakeTorque : 0;
                    }

                    //...bzw. den Autopiloten des Sprachassistenten
                    /* else if (sharedData.SelectedControl == SharedFields.VoiceAssistantControl)
                     {
                         //Debug.LogFormat("Voice Assistant Y- Achse: {0}", sharedData.AssistantYAchse);
                         angle = maxAngle * sharedData.AssistantXAchse;
                         torque = maxTorque * sharedData.AssistantYAchse;
                         handBrake = Input.GetKey(sharedData.TBrakeKey) ? brakeTorque : 0;
                     }*/
                    else
                    {
                        Debug.LogError("Die Player Control wurde auf einen ungültigen Wert gelegt.");
                        moveVertical = 0;
                        moveHorizontal = 0;
                        handBrake = brakeTorque;
                    }
                }
            }
            //Collect keystrokes for training:
            //Debug.Log(new Vector3(moveHorizontal, moveVertical, handBrake).ToString());
            /*if (!sharedData.TrainingMode)
            {
                Debug.Log("****Füge neue Trainingsdaten Hinzu");
                sharedData.trainingsFahrroute.Add(Time.frameCount, new Vector3(moveHorizontal, moveVertical, handBrake));
            }*/
            //Debug.Log("Test");
            angle = maxWheelAngle * moveVertical;
            torque = maxTorque * moveHorizontal;
            stringBuilder.AppendFormat("{0}| {1} {2}", frameCountThisTrainingRoute, angle, torque);
        }
        else   //stellt die Reifen neutral wenn keine playerControll gegeben wird
        {
            angle = 0;
            torque = 0;
            handBrake = brakeTorque;
        }

        //Trainingsdaten hinzufügen
        if (!myPlateAgent.isTrainingCar)
        {
            //Debug.Log("****Füge neue Trainingsdaten Hinzu");
            //trainingsFahrrouteSaved.Add(Time.frameCount, new Vector3(moveHorizontal, moveVertical, handBrake));
            Debug.Log("Füge für Frame hinzu: " + frameCountThisTrainingRoute);
            trainingsFahrrouteSaved.Add(frameCountThisTrainingRoute, new Vector3(moveHorizontal, moveVertical, handBrake));
            //Debug.LogFormat("***FrameCountThisTrainingRoute: {0}, FrameCount: {1}", frameCountThisTrainingRoute, Time.frameCount);
            //sharedData.trainingsFahrroute.Add(Time.frameCount, new Vector3(moveHorizontal, moveVertical, handBrake));
        }



        // Höchstgeschwindigkeit des Autos
        if (rb.velocity.magnitude >= sharedData.currentMaxSpeed)    //Die Länge des Richtungsvektors dient als Geschwindigkeitsindikator
        {
            rb.velocity = rb.velocity.normalized * sharedData.currentMaxSpeed;
        }
        if (!myPlateAgent.isTrainingCar)
        {
            sharedData.currentSpeed = rb.velocity.magnitude;
        }

        // Vordere Reifen lenken
        getCollider(FRONT_LEFT).steerAngle = angle;
        getCollider(FRONT_RIGHT).steerAngle = angle;

        // Bremsen mit allen Rädern
        fullBrake(handBrake);

        // Antrieb auf alle Räder?
        getCollider(FRONT_LEFT).motorTorque = torque;
        getCollider(FRONT_RIGHT).motorTorque = torque;
        getCollider(REAR_RIGHT).motorTorque = torque;
        getCollider(REAR_LEFT).motorTorque = torque;

        // Räder bewegen
        moveWheels(getCollider(FRONT_LEFT));
        moveWheels(getCollider(FRONT_RIGHT));
        moveWheels(getCollider(REAR_RIGHT));
        moveWheels(getCollider(REAR_LEFT));

    }


    //Simuliert das Aufladen der Achsen bei längeren Drücken
    public const String no = "None";
    public const String up = "UpWasPressedLastFrame";
    public const String down = "DownWasPressedLastFrame";
    public const String left = "LeftWasPressedLastFrame";
    public const String right = "RightWasPressedLastFrame";

    private String lastVerticalAxisButtonPressend = no;
    private String lastHorizontalAxisButtonPressend = no;
    float sumMoveHorizontal = 0;
    float sumMoveVertical = 0;
    public float stepsToAxisMax = 0.01f;

    private Vector4 GetKeyboardButtons()
    {
        //Bestimme Tastenwerte
        //float moveHorizontal = 0;
        //float moveVertical = 0;
        float brake = 0;
        float reset = 0;
        bool axisButtonPressedThisFrame = false;

        if (Input.GetKey(sharedData.TDownKey))
        {
            if (lastVerticalAxisButtonPressend != down)
            {
                sumMoveVertical = 0;
            }

            else if (sumMoveVertical > -1)
            {
                // Debug.Log("move")
                sumMoveVertical -= stepsToAxisMax;
            }
            else
            {
                // Debug.Log("dDown");
                sumMoveVertical = -1;
            }
            lastVerticalAxisButtonPressend = down;
            axisButtonPressedThisFrame = true;
            //Debug.Log("moveVertical ist jetzt: " + moveVertical);
        }
        if (Input.GetKey(sharedData.TUpKey))
        {
            if (lastVerticalAxisButtonPressend != up)
            {
                sumMoveVertical = 0;
            }

            else if (sumMoveVertical < 1)
            {
                // Debug.Log("move")
                sumMoveVertical += stepsToAxisMax;
            }
            else
            {
                //Debug.Log("dUp");
                sumMoveVertical = 1;
            }
            lastVerticalAxisButtonPressend = up;
            axisButtonPressedThisFrame = true;
            //Debug.Log("moveVertical ist jetzt: " + moveVertical);
        }

        if (Input.GetKey(sharedData.TLeftKey))
        {
            if (lastHorizontalAxisButtonPressend != left)
            {
                sumMoveHorizontal = 0;
            }

            else if (sumMoveHorizontal > -1)
            {
                // Debug.Log("move")
                sumMoveHorizontal -= stepsToAxisMax;
            }
            else
            {
                //Debug.Log("dleft");
                sumMoveHorizontal = -1;
            }
            lastHorizontalAxisButtonPressend = left;
            axisButtonPressedThisFrame = true;
            //Debug.Log("moveHorizontal ist jetzt: " + moveHorizontal);
        }

        if (Input.GetKey(sharedData.TRightKey))
        {
            if (lastHorizontalAxisButtonPressend != right)
            {
                sumMoveHorizontal = 0;
            }

            else if (sumMoveHorizontal < 1)
            {
                // Debug.Log("move")
                sumMoveHorizontal += stepsToAxisMax;
            }
            else
            {
                //Debug.Log("dleft");
                sumMoveHorizontal = 1;
            }
            lastHorizontalAxisButtonPressend = right;
            axisButtonPressedThisFrame = true;
            //Debug.Log("moveHorizontal ist jetzt: " + moveHorizontal);
        }

        if (!axisButtonPressedThisFrame)
        {
            //Debug.Log("Setze Achsen zurück");
            lastVerticalAxisButtonPressend = no;
            sumMoveHorizontal = 0;
            sumMoveVertical = 0;
        }

        if (Input.GetKey(sharedData.TBrakeKey))
        {
            //Debug.Log("dRight");
            brake = 1;
        }


        if (Input.GetKeyDown(sharedData.TResetKey))
        {
            //Debug.Log("dRight");
            reset = 1;
        }

        return new Vector4(sumMoveHorizontal, sumMoveVertical, brake, reset);
    }

    public void setPlayerControl(bool b)
    {
        playerControl = b;
    }

    public Boolean getPlayerControl()
    {
        return playerControl;
    }


    public void fullBrake(float handBrake)
    {
        getCollider(FRONT_LEFT).brakeTorque = handBrake;
        getCollider(FRONT_RIGHT).brakeTorque = handBrake;
        getCollider(REAR_RIGHT).brakeTorque = handBrake;
        getCollider(REAR_LEFT).brakeTorque = handBrake;
    }

    private void moveWheels(WheelCollider wheel)
    {
        Quaternion q;
        Vector3 p;
        wheel.GetWorldPose(out p, out q);

        // Assume that the only child of the wheelcollider is the wheel shape.
        Transform shapeTransform = wheel.transform.GetChild(0);
        shapeTransform.position = p;
        shapeTransform.rotation = q;
    }

    WheelCollider getCollider(int n)
    {
        return wheels[n].gameObject.GetComponent<WheelCollider>();
    }

    public void OnApplicationQuit()
    {
        if (trainingsFahrrouteSaved.Count != 0 && !myPlateAgent.isTrainingCar)
        {
            //Speichere die aufgezeichnete Trainingsroute in einer Datei
            int nextFreeFileNumber = dirFileCount;
            StreamWriter writer = new StreamWriter(dirPathTrainingRoute + nextFreeFileNumber, true);
            foreach (KeyValuePair<int, Vector3> item in trainingsFahrrouteSaved)
            {
                //Debug.Log(item.Value.ToString("G9"));
                String valueString = string.Format("({0}.{1}.{2})", item.Value.x, item.Value.y, item.Value.z);
                String concat = string.Format("[{0}|{1}]", item.Key, valueString);
                //KeyValuePair<int, String> n = new KeyValuePair<int, string>(item.Key, valueString);
                //Debug.Log(concat);
                if (item.Key < trainingsFahrrouteSaved.Count)
                {
                    writer.Write(concat);
                    //Debug.Log(sharedData.trainingsFahrroute.Count);
                    writer.Write(";");
                }
                else if (item.Key == trainingsFahrrouteSaved.Count)
                {
                    writer.Write(concat);
                }
            }
            writer.Close();
        }

    }
}
