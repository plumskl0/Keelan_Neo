using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlateAgent : Agent
{

    //Trainingsanreize
    public float incentiveLostLife = -5f;
    public float incentiveFinishedRoute = 5f;
    public float incentiveBallStillOnPlate = 0.01f;
    public float incentiveFactorDistanceBallToPlateCenter = 0.01f;

    //Trainingsautos haben eigene Variablen für die Steuerung
    private float plateXAxis;
    private float plateZAxis;
    private bool lostLife;
    private bool trainingRouteFinished;
    public string autoname, ballname, tellername;
    public bool isTrainingCar;	//zeigt ob Trainings oder Player Auto

    float ballAbstandZuTellermitte;

    

    private Transform carTransform;
    private ResetCar resetCarScript;
    private Rigidbody carRgBody;
    private Rigidbody ballRgBody;
    private Transform ballTransform;
    private Transform plateTransform;
    private Transform playerObjectsTransform;
    private SharedFields sharedData = SharedFields.Instance;

    public Text positiveRewardsText;
    public Text positiveRewardsThisRoundText;
    public Text negativeRewardsText;
    public Text negativeRewardsThisRoundText;
    public Text abstandBallzuTellermitte;
    public Text xVel, yVel, zVel;

    private void Awake()
    {
        //to do:später wieder einschalten -> für debugging besser selbst setzen
        /*   if (brain.brainType.Equals(BrainType.External))
           {
               sharedData.TrainingMode = true;
           }
           else if (brain.brainType.Equals(BrainType.Internal))
           {
               sharedData.TrainingMode = false;
           }*/

        switch (gameObject.tag)
        {
            case "Player":
                isTrainingCar = false;
                autoname = "Car Prototype v8";
                ballname = "Golfball_G";
                tellername = "Teller";

                break;
            case "Trainingsfahrzeug":
                isTrainingCar = true;
                autoname = "TrainingCarv8";
                ballname = "Trainingsball";
                tellername = "Trainingsteller";
                break;
            default:
                Debug.LogError("Das Fahrzeug hat keinen gültigen Tag.");
                autoname = ballname = tellername = string.Empty;
                break;
        }
    }

    // Use this for initialization
    void Start()
    {
        carTransform = GetComponent<Transform>();
        resetCarScript = GetComponent<ResetCar>();
        carRgBody = GetComponent<Rigidbody>();


        

        //Suche für das Auto die dazugehörenden: PlayerObjects, Ball, Teller:
        playerObjectsTransform = gameObject.transform.parent;
        //Debug.Log("*****Mein Name ist: " + playerObjectsTransform.name);
        GameObject ball = playerObjectsTransform.Find(ballname).gameObject;
        //Debug.Log("*****Mein Name ist: "+ ball.name);
        ballRgBody = ball.GetComponent<Rigidbody>();
        ballTransform = ball.GetComponent<Transform>();

        /*GameObject ball = playerObjectsTransform.Find("Golfball_G").gameObject;
        ballRgBody = ball.GetComponent<Rigidbody>();
        ballTransform = ball.GetComponent<Transform>();
        plateTransform = transform.Find("CarModel").Find("Teller").GetComponent<Transform>();*/

        plateTransform = gameObject.transform.Find("CarModel").Find(tellername).GetComponent<Transform>();
        //Debug.Log("*****Mein Name ist: " + plateTransform.name);


        //Im Editor veränderbare Belohnungen in sharedData schreiben
        sharedData.incentiveLostLife = incentiveLostLife;
        sharedData.incentiveFinishedRoute = incentiveFinishedRoute;
        sharedData.incentiveBallStillOnPlate = incentiveBallStillOnPlate;
        sharedData.incentiveFactorDistanceBallToPlateCenter = incentiveFactorDistanceBallToPlateCenter;

}

    public override void AgentReset()
    {
        if (sharedData.TrainingMode)
        {
            //to do: Startposition für jedes Level einfügen
            Scene myScene = SceneManager.GetActiveScene();
            Debug.Log("*********reset Car");
            switch (myScene.name)
            {
                case "Level1":
                    resetCarScript.CarReset(95.39f, 1.08926f, 30.4274f, false); //Level1
                    break;
                case "Level1Debug":
                    resetCarScript.CarReset(95.39f, 1.08926f, 30.4274f, false);
                    carTransform.localRotation = Quaternion.Euler(0f, 58.077f, 0f);
                    //plateTransform.rotation = Quaternion.Euler(75f, 0f, 0f); //todo: zurückstellen auf neutral | Schrägstellung der Plate ist für Übung ohne Autobewegung
                    break;
                case "Level1Training":
                    resetCarScript.CarReset();
                    break;
                default:
                    Debug.LogError("Beim Trainieren des PLate Controllers wurde für das aktuelle Level kein Reset Verhalten definiert");
                    break;
            }
            //Zufallswerte für Tellerneigung:
            float randomX = Random.Range(-1f, 1f);
            float randomZ = Random.Range(-1f, 1f);
            plateXAxis = randomX;
            plateZAxis = randomZ;
            if (!isTrainingCar)
            {
                sharedData.assistantPlateXAchse = randomX;
                sharedData.assistantPlateZAchse = randomZ;
            }

            //sharedData.assistantPlateXAchse = 0;
            //sharedData.assistantPlateZAchse = 0;
            resetCarScript.ResetBall();
            Debug.Log("Ball reseted");

        }
    }

    public void ObserveLikeUnityExample()
    {
        //Wie Unity 3D Ball Beispiel:
        AddVectorObs(plateTransform.rotation.z);
        AddVectorObs(plateTransform.rotation.x);
        AddVectorObs(ballTransform.position - plateTransform.position);
        AddVectorObs(ballRgBody.velocity);
    }

    List<float> obeservation = new List<float>();
    public override void CollectObservations()
    {
        /*
        //Auto...:

        AddVectorObs(carTransform.position.x);           //...Position
        AddVectorObs(carTransform.position.y);
        AddVectorObs(carTransform.position.z);
        AddVectorObs(carRgBody.velocity.x);    //...Richtungsvektor
        AddVectorObs(carRgBody.velocity.y);
        AddVectorObs(carRgBody.velocity.z);
        AddVectorObs(sharedData.currentSpeed);          //...Geschwindigkeit
        //...

        //Teller...:
        AddVectorObs(plateTransform.rotation.eulerAngles.x);         //...Neigung
        AddVectorObs(plateTransform.rotation.eulerAngles.z);

        //Ball...:
        AddVectorObs(ballTransform.position.x);     //...Position relativ zur Tellermitte (außer Höhe)
        AddVectorObs(ballTransform.position.z);
        AddVectorObs(ballRgBody.velocity.x);//...Richtungsvektor
        AddVectorObs(ballRgBody.velocity.y);
        AddVectorObs(ballRgBody.velocity.z);
        AddVectorObs(ballRgBody.velocity.sqrMagnitude);        //...Geschwindigkeit
        */


        //AddVectorObs(carRgBody.velocity);
        xVel.text = carRgBody.velocity.x.ToString();
        yVel.text = carRgBody.velocity.y.ToString();
        zVel.text = carRgBody.velocity.z.ToString();


        //AddVectorObs(plateTransform.rotation.eulerAngles);
        //AddVectorObs(ballTransform.position);

        //Wie Unity 3D Ball Beispiel:
        ObserveLikeUnityExample();


        //AddVectorObs(verbindungsvektor);



    }

    //LostLife und TrainingRouteFinished sind für Trainingsautos:
    public bool LostLife
    {
        get
        {
            return lostLife;
        }

        set
        {
            lostLife = value;
        }
    }

    public bool TrainingRouteFinished
    {
        get
        {
            return trainingRouteFinished;
        }

        set
        {
            trainingRouteFinished = value;
        }
    }


    float negativeRewards = 0;
    float positiveRewards = 0;
    float negativeRewardsThisRound = 0;
    float positiveRewardsThisRound = 0;
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        bool takeAktion = sharedData.TrainingMode || sharedData.plateAutopilot; //reine Simulation der Belohnungen falls Autopilot den Teller nicht steuern soll


        if (sharedData.trainingRouteNeedsUpdate)    //Route zu Ende geschafft -> Reset
        {
            positiveRewards += sharedData.incentiveFinishedRoute;
            positiveRewardsThisRound += sharedData.incentiveFinishedRoute;

            if (takeAktion)
            {
                AddReward(sharedData.incentiveFinishedRoute);
                Done();
                sharedData.trainingRouteNeedsUpdate = false;
            }
        }

  
        //Rewards...:
        //...harte Strafe für herunterfallen des Balles
        //if(ballHeruntergefallen) //evtl. in separaten Check umwandeln -> paralle Fahrzeuge Training, würde einen Reset auslösen sobald irgendein Auto Mist baut
        //müsste im debug Modus ausführen damit das Spiel nicht schnell zu ende ist wenn die leben leer sind
        Debug.Log("******LostLife =" + LostLife);
        if (LostLife)    //Leben verloren -> Reset
        {
            negativeRewards += sharedData.incentiveLostLife;
            positiveRewardsThisRound = 0;
            negativeRewardsThisRound = 0;
            Debug.Log("habe Leben verloren");

            if (sharedData.debugMode)   //simuliere Ball Reset im Debug Mode
            {
                resetCarScript.ResetBall(); //TODO: könnte in Konflikt treten mit Strecken Ermittlung für Trainingsdaten -> Testspieler fährt anders wenn Ball immer wieder kommt
                LostLife = false;
                if (!isTrainingCar) //PickUp Logic überwacht LostLife
                {
                    sharedData.LostLife = false;
                }

            }

            if (takeAktion)
            {
                Done();
                AddReward(sharedData.incentiveLostLife);
                LostLife = false;
                if (!isTrainingCar) //PickUp Logic überwacht LostLife
                {
                    sharedData.LostLife = false;
                }
            }

        }
        else //Ball noch auf Teller -> weiter
        {
            if (takeAktion)
            {
                /*float[] alteNeigungsvariablen = new float[] { plateXAxis, plateZAxis };
                RotatePlateByMiniSteps(vectorAction[0], vectorAction[1]);
                Debug.LogFormat("Mein Name ist: {6} Die alten Neigungsvars: {0} {1} \n Aktionen: {2} {3} \n Die neuen: {4} {5}", alteNeigungsvariablen[0], alteNeigungsvariablen[1], vectorAction[0], vectorAction[1], plateXAxis, plateZAxis, playerObjectsTransform.name);*/

                RotatePlateLikeUnityExample(vectorAction[0], vectorAction[1]);

                //RotatePlatePerDirectFloat(vectorAction[0], vectorAction[1]);

                if (!isTrainingCar) //Das Hauptauto muss die Steuerungsdaten zentral vermerken, damit bei einem Moduswechsel übernommen werden kann
                {
                    sharedData.assistantPlateXAchse = plateXAxis;
                    sharedData.assistantPlateZAchse = plateZAxis;
                }
            }








            //SetReward(0.1f);
            positiveRewards += sharedData.incentiveBallStillOnPlate;
            positiveRewardsThisRound += sharedData.incentiveBallStillOnPlate;

            //Abstand zwischen Ball und Tellermittelpunkt berechnen
            Vector3 tellermitte = plateTransform.position;
            Vector3 ballposition = ballTransform.position;
            Vector3 verbindungsvektor = ballposition - tellermitte; //kommmt noch raus -> debug
            ballAbstandZuTellermitte = DistanceBetweenTwoPoints(tellermitte, ballposition);
            //Debug.Log("Abstand Ball zu Tellermite: " + ballAbstandZuTellermitte);
            float abstandbestrafung = -sharedData.incentiveFactorDistanceBallToPlateCenter * Mathf.Clamp(1f * ballAbstandZuTellermitte, 0, 1);
            //Debug.Log("**Abstandsbestrafung: " + abstandbestrafung);
            negativeRewards += abstandbestrafung;
            negativeRewardsThisRound += abstandbestrafung;

            if (takeAktion)
            {
                //Belohnung, dass der Ball noch auf auf dem Teller ist, vermindert je weiter er von der Mitte entfernt ist
                AddReward(sharedData.incentiveBallStillOnPlate);
                AddReward(abstandbestrafung);
            }


        }

        if (!isTrainingCar)
        {
            UpdateTrainingLogs();
        }

    }

    //Unterschiedliche Strategien, um die Agent Action in Tellerneigung umzusetzen:
    private void RotatePlateByMiniSteps(float x, float z)
    {
        float actionX = Mathf.Clamp(x, -1, 1);
        float actionZ = Mathf.Clamp(z, -1, 1);
        float achsenaenderung = 0.03f;

        if (actionX < plateXAxis)
        {
            plateXAxis -= achsenaenderung;
        }

        else if (actionX > plateXAxis)
        {
            plateXAxis += achsenaenderung;
        }

        if (actionZ < plateZAxis)
        {
            plateZAxis -= achsenaenderung;
        }

        else if (actionZ > plateZAxis)
        {
            plateZAxis += achsenaenderung;
        }
        Debug.LogFormat("x-Achse: {0}  und y-Achse: {1}", plateXAxis, plateZAxis);
        plateTransform.localRotation = Quaternion.Euler(plateXAxis * sharedData.plateMaxAngle, 0f, plateZAxis * sharedData.plateMaxAngle);
    }

    private void RotatePlateLikeUnityExample(float actionX, float actionZ)
    {

        float action_z = 2f * Mathf.Clamp(actionZ, -1f, 1f);
        if ((plateTransform.rotation.z < 0.25f && action_z > 0f) ||
        (plateTransform.rotation.z > -0.25f && action_z < 0f))
        {
            /*float zielwinkel = plateTransform.rotation.eulerAngles.z + action_z;
            Debug.Log(zielwinkel + " -----> ist zielwinkel");
            //zahl * winkelmax = zielwinkel -> zielwinkel/winkelmax = zahl
            Debug.Log("ist neuer zWert: " + zielwinkel / sharedData.plateMaxAngle);
            plateZAxis = zielwinkel / sharedData.plateMaxAngle;*/
            plateTransform.Rotate(new Vector3(0, 0, 1), action_z);
        }
        float action_x = 2f * Mathf.Clamp(actionX, -1f, 1f);
        if ((plateTransform.rotation.x < 0.25f && action_x > 0f) ||
        (plateTransform.rotation.x > -0.25f && action_x < 0f))
        {
            /*float zielwinkel = plateTransform.rotation.eulerAngles.x + action_x;
            //zahl * winkelmax = zielwinkel -> zielwinkel/winkelmax = zahl
            plateXAxis = zielwinkel / sharedData.plateMaxAngle;*/
            plateTransform.Rotate(new Vector3(1, 0, 0), action_x);
        }
    }

    private void RotatePlatePerDirectFloat(float actionX, float actionZ)
    {
        //Actions -> lenke die Plattform:
        plateXAxis = Mathf.Clamp(actionX, -1, 1);
        plateZAxis = Mathf.Clamp(actionZ, -1, 1);

        plateTransform.localRotation = Quaternion.Euler(plateXAxis * sharedData.plateMaxAngle, 0f, plateZAxis * sharedData.plateMaxAngle);
    }


    //Hilfsmethoden:
    private float DistanceBetweenTwoPoints(Vector3 x, Vector3 y)
    {
        Vector3 verbindungsvektor = x - y;
        float ballAbstandZuTellermitte = verbindungsvektor.magnitude;
        return ballAbstandZuTellermitte;
    }

    private void UpdateTrainingLogs()
    {
        if (positiveRewardsText != null && positiveRewardsThisRoundText != null && negativeRewardsText != null && negativeRewardsThisRoundText != null && abstandBallzuTellermitte != null)
        {
            positiveRewardsText.text = positiveRewards.ToString();
            positiveRewardsThisRoundText.text = positiveRewardsThisRound.ToString();
            negativeRewardsText.text = negativeRewards.ToString();
            negativeRewardsThisRoundText.text = negativeRewardsThisRound.ToString();
            abstandBallzuTellermitte.text = ballAbstandZuTellermitte.ToString();
        }
        else
        {
            Debug.LogError("Es fehlt mindestens ein Verweis auf Trainingslog Objekte");
        }
    }





}
