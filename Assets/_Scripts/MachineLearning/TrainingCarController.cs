using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WiimoteApi;

public class TrainingCarController : MonoBehaviour
{
    public float maxAngle = 30f;
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
    private int frameCountThisTrainingRoute = 0;
    private int frameDurationThisRoute = 0;

    string dirPathTrainingRoute = "Assets/TrainingRoutes/"; //Ordner in dem die Trainingsrouten liegen
    int dirFileCount;
    List<FileInfo> trainingFiles = new List<FileInfo>();
    private Dictionary<int, Vector3> trainingsFahrroute = new Dictionary<int, Vector3>();
    private PlateAgentForTrainingCars myPlateAgent;


    private Rigidbody rb;

    private Boolean playerControl;
    private SharedFields sharedData = SharedFields.Instance;


    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        myPlateAgent = gameObject.GetComponent<PlateAgentForTrainingCars>();

        //Lade Files für Trainingsstrecke
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
                    moveHorizontal = 0;
                    moveVertical = 0;
                    handBrake = brakeTorque;
                }
                else
                {
                    Debug.LogError("Liste unvollständig");
                }
            }
            else
            {
                Vector3 controls = trainingsFahrroute[frameCountThisTrainingRoute];
                moveHorizontal = controls.x;
                moveVertical = controls.y;
                handBrake = controls.z;
            }
            angle = maxAngle * moveVertical;
            torque = maxTorque * moveHorizontal;
        }
        else   //stellt die Reifen neutral wenn nicht im Training Mode
        {
            angle = 0;
            torque = 0;
            handBrake = brakeTorque;
        }



        // Höchstgeschwindigkeit des Autos
        if (rb.velocity.magnitude >= sharedData.currentMaxSpeed)    //Die Länge des Richtungsvektors dient als Geschwindigkeitsindikator
        {
            rb.velocity = rb.velocity.normalized * sharedData.currentMaxSpeed;
        }
        //sharedData.currentSpeed = rb.velocity.magnitude;

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

}
