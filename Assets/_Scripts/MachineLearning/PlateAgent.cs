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



    private Transform carTransform;
    private ResetCar resetCarScript;
    private Rigidbody carRgBody;
    private Rigidbody ballRgBody;
    private Transform ballTransform;
    private Transform plateTransform;
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
    }

    // Use this for initialization
    void Start()
    {
        carTransform = GetComponent<Transform>();
        resetCarScript = GetComponent<ResetCar>();
        carRgBody = GetComponent<Rigidbody>();


        //Transform playerObjectsTransform = gameObject.GetComponentInParent<Transform>();
        GameObject ball = GameObject.Find("Golfball_G");
        //GameObject ball = gameObject.GetComponentInParent<Transform>().Find("")
        ballRgBody = ball.GetComponent<Rigidbody>();
        ballTransform = ball.GetComponent<Transform>();

        /*GameObject ball = playerObjectsTransform.Find("Golfball_G").gameObject;
        ballRgBody = ball.GetComponent<Rigidbody>();
        ballTransform = ball.GetComponent<Transform>();
        plateTransform = transform.Find("CarModel").Find("Teller").GetComponent<Transform>();*/

        plateTransform = GameObject.Find("Teller").GetComponent<Transform>();

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
            float randomX = Random.Range(-0.5f, 0.5f);
            float randomZ = Random.Range(-0.5f, 0.5f);
            sharedData.assistantPlateXAchse = randomX;
            sharedData.assistantPlateZAchse = randomZ;

            //sharedData.assistantPlateXAchse = 0;
            //sharedData.assistantPlateZAchse = 0;
            resetCarScript.ResetBall();
            Debug.Log("Ball reseted");

        }
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


        AddVectorObs(plateTransform.rotation.eulerAngles);
        AddVectorObs(ballTransform.position);


        //AddVectorObs(verbindungsvektor);



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
            positiveRewards += incentiveFinishedRoute;
            positiveRewardsThisRound += incentiveFinishedRoute;

            if (takeAktion)
            {
                AddReward(incentiveFinishedRoute);
                Done();
                sharedData.trainingRouteNeedsUpdate = false;
            }
        }

        float abstand = plateTransform.position.y - ballTransform.position.y;
        bool ballHeruntergefallen;
        if (plateTransform.position.y > ballTransform.position.y)
        {
            ballHeruntergefallen = true;
        }
        else
        {
            ballHeruntergefallen = false;
        }

        //Debug.LogFormat("plate: {0}, ball: {1}____ abstand: {2}", plateTransform.position.y, ballTransform.position.y, abstand);


        //Rewards...:
        //...harte Strafe für herunterfallen des Balles
        //if(ballHeruntergefallen) //evtl. in separaten Check umwandeln -> paralle Fahrzeuge Training, würde einen Reset auslösen sobald irgendein Auto Mist baut
        //müsste im debug Modus ausführen damit das Spiel nicht schnell zu ende ist wenn die leben leer sind
        Debug.Log("******LostLife =" + sharedData.LostLife);
        if (sharedData.LostLife)    //Leben verloren -> Reset
        {
            negativeRewards += incentiveLostLife;
            positiveRewardsThisRound = 0;
            negativeRewardsThisRound = 0;
            Debug.Log("habe Leben verloren");

            if (sharedData.debugMode)   //simuliere Ball Reset im Debug Mode
            {
                resetCarScript.ResetBall(); //TODO: könnte in Konflikt treten mit Strecken Ermittlung für Trainingsdaten -> Testspieler fährt anders wenn Ball immer wieder kommt
                sharedData.LostLife = false;

            }

            if (takeAktion)
            {
                Done();
                AddReward(incentiveLostLife);
                sharedData.LostLife = false;
            }

        }
        else //Ball noch auf Teller -> weiter
        {
            if (takeAktion)
            {
                RotatePlateByMiniSteps(vectorAction[0], vectorAction[1]);
                //RotatePlateLikeUnityExample(vectorAction[0], vectorAction[1]);
            }



            //Actions -> lenke die Plattform:
            //sharedData.assistantPlateXAchse = Mathf.Clamp(vectorAction[0], -1, 1);
            //sharedData.assistantPlateZAchse = Mathf.Clamp(vectorAction[1], -1, 1);




            //SetReward(0.1f);
            positiveRewards += incentiveBallStillOnPlate;
            positiveRewardsThisRound += incentiveBallStillOnPlate;

            //Abstand zwischen Ball und Tellermittelpunkt berechnen
            Vector3 tellermitte = plateTransform.position;
            Vector3 ballposition = ballTransform.position;
            Vector3 verbindungsvektor = ballposition - tellermitte; //kommmt noch raus -> debug
            float ballAbstandZuTellermitte = DistanceBetweenTwoPoints(tellermitte, ballposition);
            abstandBallzuTellermitte.text = ballAbstandZuTellermitte.ToString();
            //Debug.Log("Abstand Ball zu Tellermite: " + ballAbstandZuTellermitte);
            //todo: füge Bestrafung hinzu je weiter der Ball von der Mitte weg ist
            float abstandbestrafung = -incentiveFactorDistanceBallToPlateCenter * Mathf.Clamp(1f * ballAbstandZuTellermitte, 0, 1);
            //Debug.Log("**Abstandsbestrafung: " + abstandbestrafung);
            negativeRewards += abstandbestrafung;
            negativeRewardsThisRound += abstandbestrafung;

            if (takeAktion)
            {
                //Belohnung, dass der Ball noch auf auf dem Teller ist, vermindert je weiter er von der Mitte entfernt ist
                AddReward(incentiveBallStillOnPlate);
                AddReward(abstandbestrafung);
            }


        }

        UpdateTrainingLogs();

    }

    //Unterschiedliche Strategien, um die Agent Action in Tellerneigung umzusetzen:
    private void RotatePlateByMiniSteps(float actionX, float actionZ)
    {
        float x = Mathf.Clamp(actionX, -1, 1);
        float z = Mathf.Clamp(actionZ, -1, 1);
        float achsenaenderung = 0.03f;

        if (actionX < sharedData.assistantPlateXAchse)
        {
            sharedData.assistantPlateXAchse -= achsenaenderung;
        }

        else if (actionX > sharedData.assistantPlateXAchse)
        {
            sharedData.assistantPlateXAchse += achsenaenderung;
        }

        if (actionZ < sharedData.assistantPlateZAchse)
        {
            sharedData.assistantPlateZAchse -= achsenaenderung;
        }

        else if (actionZ > sharedData.assistantPlateZAchse)
        {
            sharedData.assistantPlateZAchse += achsenaenderung;
        }
        Debug.LogFormat("x-Achse: {0}  und y-Achse: {1}", sharedData.assistantPlateXAchse, sharedData.assistantPlateZAchse);
    }

    private void RotatePlateLikeUnityExample(float actionX, float actionZ)
    {

        float action_z = 2f * Mathf.Clamp(actionZ, -1f, 1f);
        if ((plateTransform.rotation.z < 0.25f && action_z > 0f) ||
        (plateTransform.rotation.z > -0.25f && action_z < 0f))
        {
            float zielwinkel = plateTransform.rotation.eulerAngles.z + action_z;
            Debug.Log(zielwinkel + " -----> ist zielwinkel");
            //zahl * winkelmax = zielwinkel -> zielwinkel/winkelmax = zahl
            Debug.Log("ist neuer zWert: " + zielwinkel / sharedData.plateMaxAngle);
            sharedData.assistantPlateZAchse = zielwinkel / sharedData.plateMaxAngle;
            //plateTransform.Rotate(new Vector3(0, 0, 1), action_z);
        }
        float action_x = 2f * Mathf.Clamp(actionX, -1f, 1f);
        if ((plateTransform.rotation.x < 0.25f && action_x > 0f) ||
        (plateTransform.rotation.x > -0.25f && action_x < 0f))
        {
            float zielwinkel = plateTransform.rotation.eulerAngles.x + action_x;
            //zahl * winkelmax = zielwinkel -> zielwinkel/winkelmax = zahl
            sharedData.assistantPlateXAchse = zielwinkel / sharedData.plateMaxAngle;
            //plateTransform.Rotate(new Vector3(1, 0, 0), action_x);
        }

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
        if (positiveRewardsText != null && positiveRewardsThisRoundText != null && negativeRewardsText != null && negativeRewardsThisRoundText != null)
        {
            positiveRewardsText.text = positiveRewards.ToString();
            positiveRewardsThisRoundText.text = positiveRewardsThisRound.ToString();
            negativeRewardsText.text = negativeRewards.ToString();
            negativeRewardsThisRoundText.text = negativeRewardsThisRound.ToString();
        }
        else
        {
            Debug.LogError("Es fehlt mindestens ein Verweis auf Trainingslog Objekte");
        }
    }





}
