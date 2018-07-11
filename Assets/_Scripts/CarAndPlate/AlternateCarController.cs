using System;
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
    public float maxTrainingRouteDiff = 0.01f;

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
    private Dictionary<int, Vector3> trainingsFahrroutePosition = new Dictionary<int, Vector3>();
    private Dictionary<int, Vector3> trainingsFahrroutePositionSaved = new Dictionary<int, Vector3>();
    private Dictionary<int, Vector3>  trainingsFahrrouteVelocitySaved = new Dictionary<int, Vector3>();
    private Dictionary<int, Vector3> trainingsFahrrouteVelocity = new Dictionary<int, Vector3>();
    private Dictionary<int, Vector3> trainingsFahrrouteRotation = new Dictionary<int, Vector3>();
    private Dictionary<int, Vector3> trainingsFahrrouteRotationSaved = new Dictionary<int, Vector3>();

    private StringBuilder angleTorqueStringBuilder = new StringBuilder();
    private StringBuilder positionStringBuilder = new StringBuilder();
    private StringBuilder wheelColliderStringBuilder = new StringBuilder();
    private StringBuilder finalTorqueAngleStringBuilder = new StringBuilder();

    private Rigidbody rb;

    private Boolean playerControl;
    private GameObject wiiMoteRef;
    private wiiKalibrierung wiiDaten;
    public Wiimote wiiRemote;
    private ResetCar resetCarScript;
    private SharedFields sharedData = SharedFields.Instance;

    //
    private Quaternion trainingsQ;
    private Vector3 trainingsP;
    public GameObject oldPositionSphere;  //zeigt an wo das Auto im letzten Durchlauf war

    //Training und Debug Möglichkeiten per Editor einschalten:
    public bool activateTrainingMode = false;
    public bool activateDebugMode = false;
    public bool nonMovingCar = false;
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
            sharedData.maxTorque = maxTorque;
            sharedData.maxWheelAngle = maxWheelAngle;
            sharedData.nonMovingCar = nonMovingCar;


            if (sharedData.nonMovingCar)
            {
                sharedData.maxSpeed = 0f;
                Debug.Log("***Habe Fahrzeug bewegungsunfähig gemacht");
            }
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

        LoadTrainingFiles();

        //Lade eine Strecke, falls der Trainingsmodus aktiv ist
        if (sharedData.TrainingMode)
        {
            LoadTrainingRoute();
        }
    }

    private void LoadTrainingFiles()
    {
        DirectoryInfo dir = new DirectoryInfo(dirPathTrainingRoute);
        dirFileCount = dir.GetFiles().Length - dir.GetFiles("*.meta").Length - dir.GetFiles("*Position").Length;
        Debug.Log("init anzahl files: " + dirFileCount);
        foreach (FileInfo file in dir.EnumerateFiles())
        {
            if (!((file.Name.Contains("meta")) || (file.Name.Contains("Position"))))
            {
                Debug.Log("Füge File hinzu:" + file.Name);
                trainingFiles.Add(file);
                Debug.Log("Jetztiger File Count: " + trainingFiles.Count);
            }
        }
    }

    private void LoadTrainingRoute()
    {
        //Setze alle Routenspeicher zurück, um sie wieder füllen zu können
        trainingsFahrroute.Clear();
        trainingsFahrroutePosition.Clear();
        trainingsFahrrouteVelocity.Clear();
        trainingsFahrrouteRotation.Clear();
        
        if (trainingsFahrrouteSaved.Count != 0)
        {
            WriteTrainingsRouteToFile();
            //LoadTrainingFiles();
        }
        trainingsFahrrouteSaved.Clear();
        trainingsFahrroutePositionSaved.Clear();
        trainingsFahrrouteVelocitySaved.Clear();
        trainingsFahrrouteRotationSaved.Clear();

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
        MapFileToDict(dirPathTrainingRoute + trainingFiles[randomFileNumber].Name, trainingsFahrroute);
        MapFileToDict(dirPathTrainingRoute + trainingFiles[randomFileNumber].Name+ "Position", trainingsFahrroutePosition);
        MapFileToDict(dirPathTrainingRoute + "debugLogs/" + trainingFiles[randomFileNumber].Name + "velocity", trainingsFahrrouteVelocity);
        MapFileToDict(dirPathTrainingRoute + "debugLogs/" + trainingFiles[randomFileNumber].Name + "rotation", trainingsFahrrouteRotation);

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

    //Hilfsmethode um aus gespeicherten Daten Dict zu erstellen
    private void MapFileToDict(string filePath, Dictionary<int, Vector3> dict)
    {
        StreamReader read = new StreamReader(filePath);
        string s = read.ReadToEnd();
        string[] keyValueArray = s.Split(';');
        foreach (string t in keyValueArray)
        {
            if (!t.Equals(""))
            {
                int start = t.IndexOf("[");
                int laenge = t.IndexOf("]") - 1 - t.IndexOf("[");
                string ohneKlammern = t.Substring(t.IndexOf("[") + 1, t.IndexOf("]") - 1 - t.IndexOf("["));
                string[] keyAndValue = ohneKlammern.Split('|');
                string[] floatValues = keyAndValue[1].Substring(keyAndValue[1].IndexOf('(') + 1, keyAndValue[1].IndexOf(')') - 1 - keyAndValue[1].IndexOf('(')).Split('.');
                Vector3 finalValues = new Vector3(float.Parse(floatValues[0]), float.Parse(floatValues[1]), float.Parse(floatValues[2]));
                dict.Add(Int32.Parse(keyAndValue[0]), finalValues);
            }
        }
        read.Close();
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
        //Verzögerter Start der Autos im Trainingsmodus; sonst direkt
        if (sharedData.TrainingMode)
        {

            if (myPlateAgent.delay > 0)
            {
                myPlateAgent.delay--;
                //Debug.Log(myPlateAgent.delay);
                if (myPlateAgent.delay == 0 && !sharedData.nonMovingCar) //Sobald die Verzögerung rum ist setze das Fahrzeug an die Startposition
                {
                    resetCarScript.CarReset(sharedData.level1ResetPosition[0], sharedData.level1ResetPosition[1], sharedData.level1ResetPosition[2], false, sharedData.level1ResetPosition[3]);
                }
            }
            else
            {

                frameCountThisTrainingRoute++;
            }
        }


        else
        {
            frameCountThisTrainingRoute++;
        }
        sharedData.currentFrameCount = frameCountThisTrainingRoute;

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
                        Debug.Log("beende Strecke bei Frame Count = " + frameCountThisTrainingRoute);
                        Debug.Log("Traingsstrecke beendet... Agent Reset");

                        myPlateAgent.TrainingRouteFinished = true;
                        frameCountThisTrainingRoute = 1;
                        LoadTrainingRoute();


                        //Nehme die Steuerungsdaten der neuen Route
                        try
                        {
                            Vector3 controls = trainingsFahrroute[frameCountThisTrainingRoute];
                            moveHorizontal = controls.x;
                            moveVertical = controls.y;
                            handBrake = controls.z;
                        }
                        catch (Exception)
                        {
                            Debug.LogErrorFormat("Fehler beim lesen des Keys: {0}, frameMax in Dict: {1}", frameCountThisTrainingRoute, frameDurationThisRoute);
                            moveHorizontal = 0f;
                            moveVertical = 0f;
                            handBrake = 0f;
                            throw;
                        }
                        //probe -> setze pro frame velocity und rotation direkt
                        try
                        {
                            transform.eulerAngles = trainingsFahrrouteRotation[frameCountThisTrainingRoute];
                            rb.velocity = trainingsFahrrouteVelocity[frameCountThisTrainingRoute];
                            

                            
                        }
                        catch (Exception)
                        {
                            Debug.LogErrorFormat("Fehler beim lesen des Keys: {0}, frameMax in Dict: {1} beim setzen der Velocity", frameCountThisTrainingRoute, frameDurationThisRoute);
                            throw;
                        }


                        /*moveHorizontal = 0;
                        moveVertical = 0;
                        handBrake = brakeTorque;*/
                    }
                    else if (frameCountThisTrainingRoute == 0)
                    {
                        //Debug.Log("Warte auf Start");
                    }

                    else
                    {
                        Debug.LogError("Liste unvollständig");
                    }
                }
                else //ansonsten weise die gespeicherten Keystrokes zu:
                {
                    //probe -> setze pro frame velocity direkt
                    try
                    {
                        transform.eulerAngles = trainingsFahrrouteRotation[frameCountThisTrainingRoute];
                        rb.velocity = trainingsFahrrouteVelocity[frameCountThisTrainingRoute];
                        //


                    }
                    catch (Exception)
                    {
                        Debug.LogErrorFormat("Fehler beim lesen des Keys: {0}, frameMax in Dict: {1} beim setzen der Velocity", frameCountThisTrainingRoute, frameDurationThisRoute);
                        throw;
                    }

                    Vector3 controls = trainingsFahrroute[frameCountThisTrainingRoute];
                    moveHorizontal = controls.x;
                    moveVertical = controls.y;
                    handBrake = controls.z;


                    if (!myPlateAgent.isTrainingCar)
                    {
                        //Debug.LogFormat("Abstand zu Sollroute: {0}", Mathf.Abs((trainingsFahrroutePosition[frameCountThisTrainingRoute].normalized - gameObject.transform.position.normalized).magnitude));
                        oldPositionSphere.transform.position = trainingsFahrroutePosition[frameCountThisTrainingRoute];
                    }

                    //Setze Auto auf Position der Trainingsroute -> gleicht die kleinen Abweichungen aus
                   gameObject.transform.position = trainingsFahrroutePosition[frameCountThisTrainingRoute];

                    //über leichte Abweichgungen der Routen soll die Fahrroute sich nicht ändern -> auf Route zurücksetzen falls Grenzwert überschritten
                    if (Mathf.Abs((trainingsFahrroutePosition[frameCountThisTrainingRoute].normalized - gameObject.transform.position.normalized).magnitude) > maxTrainingRouteDiff) 
                    {
                        if (!myPlateAgent.isTrainingCar)
                        {
                            //Debug.LogError("War zu weit weg von Zielroute -> Rübersetzen");
                            //Debug.LogErrorFormat("Weil: {0}", Mathf.Abs((trainingsFahrroutePosition[frameCountThisTrainingRoute].normalized - gameObject.transform.position.normalized).magnitude));
                        }

                        //gameObject.transform.position = trainingsFahrroutePosition[frameCountThisTrainingRoute];
                    }
                    else
                    {
                        //Debug.LogFormat("{0} ist kleiner als {1}", Mathf.Abs((trainingsFahrroutePosition[frameCountThisTrainingRoute].normalized - gameObject.transform.position.normalized).magnitude), maxTrainingRouteDiff);
                    }
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
                        //moveVertical = keyboardMovement.x;
                        //moveHorizontal = keyboardMovement.y;
                        moveVertical = keyboardMovement.y;
                        moveHorizontal = keyboardMovement.x;
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
            //***Bei den folgenden beiden Zeilen treten Rundungsfehler auf
            angle = sharedData.maxWheelAngle * moveHorizontal;
            ///if(!myPlateAgent.isTrainingCar)
                //Debug.LogError("Angle: " + angle);
            torque = sharedData.maxTorque * moveVertical;


            if (!myPlateAgent.isTrainingCar)
            {
                angleTorqueStringBuilder.AppendFormat("{0}| {1} {2} {3};", frameCountThisTrainingRoute, angle, torque, handBrake);
                positionStringBuilder.AppendFormat("{0}| {1} {2} {3}, angle: {4}, torque: {5}, handBrake: {6};", frameCountThisTrainingRoute, gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z, angle, torque, handBrake);            }
            //Debug.LogFormat("Eigener Frame Count {0}, tatsächlich: {1} \n ", frameCountThisTrainingRoute, Time.frameCount);
        }

        else   //stellt die Reifen neutral wenn keine playerControll gegeben wird
        {
            Debug.LogError("keine player controll -> bleibe stehen");
            angle = 0;
            torque = 0;
            handBrake = brakeTorque;
        }

        //Strecke in Form von Keystrokes hinzufügen, falls kein Trainings Modus
        //if (!sharedData.TrainingMode && !myPlateAgent.isTrainingCar)
        if (!myPlateAgent.isTrainingCar)      //option zu oben: zeichne auch im Trainingsmode die Bewegungen auf
        {
            //Debug.Log("****Füge neue Trainingsdaten Hinzu");
            //trainingsFahrrouteSaved.Add(Time.frameCount, new Vector3(moveHorizontal, moveVertical, handBrake));
            //Debug.Log("Füge für Frame hinzu: " + frameCountThisTrainingRoute);
            //Debug.LogErrorFormat("{0}| {1} {2} {3}; ", frameCountThisTrainingRoute, moveHorizontal, moveVertical, handBrake);
  

            //Stoppe Aufzeichnung sobald der Spieler das Signal gibt (Sprachsteuerung)
            if (!sharedData.trainingRouteRecordingStopped)
            {
                trainingsFahrrouteSaved.Add(frameCountThisTrainingRoute, new Vector3(moveHorizontal, moveVertical, handBrake));
                trainingsFahrroutePositionSaved.Add(frameCountThisTrainingRoute, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z));
                trainingsFahrrouteVelocitySaved.Add(frameCountThisTrainingRoute, rb.velocity);
                trainingsFahrrouteRotationSaved.Add(frameCountThisTrainingRoute, transform.eulerAngles);
            }

            else
            {
                //Warte bis Schwierigkeitsgrad durch User definiert; dann speichere Route entsprechend
                if(sharedData.trainingRouteDifficulty!="")
                {
                    //******Jeweils in Ordner gegliedert....
                }
            }

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
        wheelColliderStringBuilder.Append(";");

        if(!myPlateAgent.isTrainingCar)
        {
            finalTorqueAngleStringBuilder.AppendFormat("{0} | MotorTorque: {1} SteerAngle: {2};", frameCountThisTrainingRoute, getCollider(FRONT_LEFT).motorTorque, getCollider(FRONT_LEFT).steerAngle);
        }

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
            //Debug.LogError("moveHorizontal ist jetzt: " + moveHorizontal);
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
        //Probe Steuerung der Räder nur außerhalb Trainingsmodus -> sonst direkte velocity zuweisung
        if (!sharedData.TrainingMode)
        {
            Quaternion q;
            Vector3 p;

            wheel.GetWorldPose(out p, out q);

            // Assume that the only child of the wheelcollider is the wheel shape.
            Transform shapeTransform = wheel.transform.GetChild(0);
            shapeTransform.position = p;
            shapeTransform.rotation = q;
        }
        else
        {
            Quaternion q;
            Vector3 p;

            wheel.GetWorldPose(out p, out q);

            // Assume that the only child of the wheelcollider is the wheel shape.
            Transform shapeTransform = wheel.transform.GetChild(0);
            shapeTransform.position = p;
            shapeTransform.rotation = q;
        }
    }

    WheelCollider getCollider(int n)
    {
        return wheels[n].gameObject.GetComponent<WheelCollider>();
    }

    public void OnApplicationQuit()
    {
        //if (trainingsFahrrouteSaved.Count != 0 && !sharedData.TrainingMode && !myPlateAgent.isTrainingCar)
        if (trainingsFahrrouteSaved.Count != 0 && !myPlateAgent.isTrainingCar)
        {
            WriteTrainingsRouteToFile();
        }

    }

    private void WriteTrainingsRouteToFile()
    {
        //Speichere die aufgezeichnete Trainingsroute in einer Datei
        int nextFreeFileNumber = dirFileCount;
        if (!sharedData.TrainingMode)
        {
            WriteRouteDictToFile(dirPathTrainingRoute + nextFreeFileNumber, trainingsFahrrouteSaved);
            WriteRouteDictToFile(dirPathTrainingRoute + nextFreeFileNumber+ "Position", trainingsFahrroutePositionSaved);
            WriteRouteDictToFile(dirPathTrainingRoute + "debugLogs/" + nextFreeFileNumber + "velocity", trainingsFahrrouteVelocitySaved);
            WriteRouteDictToFile(dirPathTrainingRoute + "debugLogs/" + nextFreeFileNumber + "rotation", trainingsFahrrouteRotationSaved);
        }


        //Log zu jedem Lauf:
        StreamWriter debugWriter = new StreamWriter(dirPathTrainingRoute + "debugLogs/" + nextFreeFileNumber, false);
        foreach (KeyValuePair<int, Vector3> item in trainingsFahrrouteSaved)
        {
            //Debug.Log(item.Value.ToString("G9"));
            String valueString = string.Format("({0}.{1}.{2})", item.Value.x, item.Value.y, item.Value.z);
            String concat = string.Format("[{0}|{1}]", item.Key, valueString);
            //KeyValuePair<int, String> n = new KeyValuePair<int, string>(item.Key, valueString);
            //Debug.Log(concat);
            debugWriter.WriteLine(concat);
        }
        debugWriter.Close();

        //Schreibe immer Logdateien: 1.Winkel der Räder und Torque, 2. Position
        StreamWriter writer2 = new StreamWriter(dirPathTrainingRoute + "debugLogs/" + nextFreeFileNumber + "angleTorque", false);
        string[] stringarray = angleTorqueStringBuilder.ToString().Split(';');
        foreach (string s in stringarray)
        {
            writer2.WriteLine(s);
        }
        writer2.Close();

        //Und immer Logdatei: Frame -> Autposition
        StreamWriter writer3 = new StreamWriter(dirPathTrainingRoute + "debugLogs/" + nextFreeFileNumber + "position", false);
        string[] stringarray2 = positionStringBuilder.ToString().Split(';');
        foreach (string s in stringarray2)
        {
            writer3.WriteLine(s);
        }
        writer3.Close();

        StreamWriter writer4 = new StreamWriter(dirPathTrainingRoute + "debugLogs/" + nextFreeFileNumber + "colliderData", false);
        string[] stringarray3 = wheelColliderStringBuilder.ToString().Split(';');
        foreach (string s in stringarray3)
        {
            writer4.WriteLine(s);
        }
        writer4.Close();

        StreamWriter writer5 = new StreamWriter(dirPathTrainingRoute + "debugLogs/" + nextFreeFileNumber + "finalAngleTorque", false);
        string[] stringarray4 = finalTorqueAngleStringBuilder.ToString().Split(';');
        foreach (string s in stringarray4)
        {
            writer5.WriteLine(s);
        }
        writer5.Close();
    }

    public void WriteRouteDictToFile(string filePath, Dictionary<int, Vector3> sourceDict)
    {
        StreamWriter writer = new StreamWriter(filePath, false);

        foreach (KeyValuePair<int, Vector3> item in sourceDict)
        {
            //Debug.Log(item.Value.ToString("G9"));
            String valueString = string.Format("({0}.{1}.{2})", item.Value.x, item.Value.y, item.Value.z);
            String concat = string.Format("[{0}|{1}]", item.Key, valueString);
            //KeyValuePair<int, String> n = new KeyValuePair<int, string>(item.Key, valueString);
            //Debug.Log(concat);
            if (item.Key < trainingsFahrrouteSaved.Count)
            {
                writer.Write(concat);//Todo: schaue ob laden der Route noch richtung klappt -> jetzt mit Zeilenumbruhc
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
