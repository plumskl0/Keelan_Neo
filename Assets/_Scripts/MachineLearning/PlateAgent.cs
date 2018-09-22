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
    public int delayFactor = 50;

    //Trainingsautos haben eigene Variablen für die Steuerung
    private float plateXAxis;
    private float plateZAxis;
    private bool lostLife;
    private bool trainingRouteFinished;
    public string autoname, ballname, tellername;
    public bool isTrainingCar;  //zeigt ob Trainings oder Player Auto

    // Verzögere den Start pro Trainingsauto unterschiedlich -> müssen von selber Position starten, aber wenn dies gleichzeitig passiert gibt es zu viel Kollisionen
    public int delay = 0;


    float ballAbstandZuTellermitte;



    private Transform carTransform;
    private ResetCar resetCarScript;
    private Rigidbody carRgBody;
    private Rigidbody ballRgBody;
    //private Transform ballTransform;
    public Transform ballTransform;
    private Transform plateTransform;
    public Transform playerObjectsTransform;
    private AlternateCarController carControllerScript;
    private SharedFields sharedData = SharedFields.Instance;

    public Text positiveRewardsText;
    public Text positiveRewardsThisRoundText;
    public Text negativeRewardsText;
    public Text negativeRewardsThisRoundText;
    public Text abstandBallzuTellermitte;
    public Text xVel, yVel, zVel;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Trainingsfahrzeug") || collision.transform.CompareTag("Trainingsball"))
        {
            //Debug.LogErrorFormat("Zusammenstoß von {0} mit {1} und Tag {2}", gameObject.transform.parent.name, collision.transform.parent.name, collision.transform.tag);
        }

    }

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


        if (!isTrainingCar)
        {
            //Im Editor veränderbare Belohnungen in sharedData schreiben
            sharedData.incentiveLostLife = incentiveLostLife;
            sharedData.incentiveFinishedRoute = incentiveFinishedRoute;
            sharedData.incentiveBallStillOnPlate = incentiveBallStillOnPlate;
            sharedData.incentiveFactorDistanceBallToPlateCenter = incentiveFactorDistanceBallToPlateCenter;
            sharedData.delayFactor = delayFactor;
            Debug.LogFormat("lostlife: {0}, finishedRoute: {1}, ballonPlate: {2}, center {3} , delayFactor: {4} ", sharedData.incentiveLostLife, sharedData.incentiveFinishedRoute, sharedData.incentiveBallStillOnPlate, sharedData.incentiveFactorDistanceBallToPlateCenter, sharedData.delayFactor);
        }

        carTransform = GetComponent<Transform>();
        resetCarScript = GetComponent<ResetCar>();
        carRgBody = GetComponent<Rigidbody>();
        carControllerScript = GetComponent<AlternateCarController>();



        //Suche für das Auto die dazugehörenden: PlayerObjects, Ball, Teller:
        playerObjectsTransform = gameObject.transform.parent;
        Debug.Log("*****Mein Name ist: " + playerObjectsTransform.name);


        GameObject ball = playerObjectsTransform.Find(ballname).gameObject;
        //Debug.Log("*****Mein Name ist: " + ball.name);
        ballRgBody = ball.GetComponent<Rigidbody>();
        ballTransform = ball.GetComponent<Transform>();

        plateTransform = gameObject.transform.Find("CarModel").Find(tellername).GetComponent<Transform>();
        //Debug.Log("*****Mein Name ist: " + plateTransform.name);
    }



    // Use this for initialization
    void Start()
    {
        if (isTrainingCar)
        {
            char[] chars = playerObjectsTransform.name.ToCharArray();
            try
            {
                delay = int.Parse(chars[chars.Length - 1].ToString()) * sharedData.delayFactor;
            }
            catch (System.Exception)
            {
                Debug.LogErrorFormat("Für das Trainingsfahrtzeugt {0} konnte das letzte Zeichen nicht zu int geparsed werden", playerObjectsTransform.name);
                throw;
            }
        }
    }

    /*private void Update()
    {
        if(!isTrainingCar)
    }*/

    public override void AgentReset()
    {
        if (sharedData.TrainingMode)
        {
            //to do: Startposition für jedes Level einfügen
            Scene myScene = SceneManager.GetActiveScene();
            //Debug.Log("*********reset Car");
            switch (myScene.name)
            {
                case "Level1":
                    resetCarScript.CarReset(sharedData.level1ResetPosition[0], sharedData.level1ResetPosition[1], sharedData.level1ResetPosition[2], false, sharedData.level1ResetPosition[3]); //Level1
                    break;
                case "Level1Debug":
                    resetCarScript.CarReset(sharedData.level1ResetPosition[0], sharedData.level1ResetPosition[1], sharedData.level1ResetPosition[2], false, sharedData.level1ResetPosition[3]);
                    //plateTransform.rotation = Quaternion.Euler(75f, 0f, 0f); //todo: zurückstellen auf neutral | Schrägstellung der Plate ist für Übung ohne Autobewegung
                    break;
                case "Level1Training":
                    //Reset der Belohnungen bis zum Lebensverlust bzw. Ende der Strecke, Statisik wurde bereit an der Stelle gefüllt, die Lostlife und TrainingsrouteFinished setzen
                    positiveRewardsThisRound = 0;
                    negativeRewardsThisRound = 0;
                    //resetCarScript.CarReset(95.39f, 1.08926f, 30.4274f, false);
                    if (sharedData.nonMovingCar)    //benötigt einen Reset an Ort und Stelle, damit die Autos nicht übereinander stehen
                    {
                        if (trainingRouteFinished)
                        {
                            resetCarScript.CarReset();
                            trainingRouteFinished = false;
                        }

                        else if (LostLife)
                        {
                            resetCarScript.ResetBall();
                            LostLife = false;
                            if (!isTrainingCar) //PickUp Logic überwacht LostLife
                            {
                                sharedData.LostLife = false;
                            }
                        }
                        else
                        {
                            Debug.Log("Erster Reset aller Agenten oder Nicht behandelter Done Zustand des Agenten.");
                            //resetCarScript.CarReset(sharedData.level1ResetPosition[0], sharedData.level1ResetPosition[1], sharedData.level1ResetPosition[2], false, sharedData.level1ResetPosition[3]);
                            resetCarScript.CarReset();
                        }
                    }
                    else    //Reset muss an selber Startposition aber zeitlich verzögert stattfinden
                    {
                        if (trainingRouteFinished)
                        {
                            resetCarScript.CarReset(sharedData.level1ResetPosition[0], sharedData.level1ResetPosition[1], sharedData.level1ResetPosition[2], false, sharedData.level1ResetPosition[3]);
                            trainingRouteFinished = false;
                        }

                        else if (LostLife)
                        {
                            resetCarScript.ResetBall();
                            LostLife = false;
                            if (!isTrainingCar) //PickUp Logic überwacht LostLife
                            {
                                sharedData.LostLife = false;
                            }

                        }
                        else
                        {
                            Debug.Log("Erster Reset aller Agenten oder Nicht behandelter Done Zustand des Agenten.");
                            //resetCarScript.CarReset(sharedData.level1ResetPosition[0], sharedData.level1ResetPosition[1], sharedData.level1ResetPosition[2], false, sharedData.level1ResetPosition[3]);
                            resetCarScript.CarReset();
                        }
                    }
                    break;
                default:
                    Debug.LogError("Beim Trainieren des PLate Controllers wurde für das aktuelle Level kein Reset Verhalten definiert");
                    break;
            }


            //Neige den Teller unabhängig vom gewählten Level: NonMoving -> Zufallswerte, sonst -> beibehalten
            if (sharedData.nonMovingCar) //Zufallsneigung des Tellers falls das Auto sich nicht bewegt
            {
                //Zufallswerte für Tellerneigung:
                float randomX = Random.Range(-0.4f, 0.4f);
                float randomZ = Random.Range(-0.4f, 0.4f);



                //Schwierige Testwerte, Einschalten um Gehirnperformance zu testen:
                /*randomX = Random.Range(0.2f, 0.3f);
                randomZ = Random.Range(0.2f, 0.4f);
                // Zufallsentscheidung ob Teller stark nach rechts/links bzw. vorne/hinten geneigt wird:
                int randomSignX = Random.Range(0, 1); 
                int randomSignZ = Random.Range(0, 1);
                if (randomSignX == 1)
                {
                    randomX = randomX * -1;
                }
                if(randomSignZ == 1)
                {
                    randomZ = randomZ * -1;
                }*/


                // Teller neigen:           
                plateXAxis = randomX;   //übertrage die Werte für nahtlosen Übergang bei Steuerwechsel
                plateZAxis = randomZ;
                if (!isTrainingCar)
                {
                    sharedData.assistantPlateXAchse = plateXAxis;
                    sharedData.assistantPlateZAchse = plateZAxis;
                }

                plateTransform.localRotation = Quaternion.Euler(plateXAxis * sharedData.plateMaxAngle, 0f, plateZAxis * sharedData.plateMaxAngle);

            }
            else
            {
                // Teller neigen: 
                /*plateXAxis = 0f;     //übertrage die Werte für nahtlosen Übergang bei Steuerwechsel
                plateZAxis = 0f;
                if (!isTrainingCar)
                {
                    sharedData.assistantPlateXAchse = plateXAxis;
                    sharedData.assistantPlateZAchse = plateZAxis;
                }
                plateTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);*/
            }



            //sharedData.assistantPlateXAchse = 0;
            //sharedData.assistantPlateZAchse = 0;
            resetCarScript.ResetBall();
            //Debug.Log("Ball reseted");

        }
    }

    public void ObserveLikeUnityExample()
    {
        AddVectorObs(carRgBody.velocity.normalized);

        //Wie Unity 3D Ball Beispiel:
        AddVectorObs(plateTransform.rotation.z);
        AddVectorObs(plateTransform.rotation.x);
        AddVectorObs(ballTransform.position - plateTransform.position);
        AddVectorObs(ballRgBody.velocity);
    }

    //Standardisiert die Variable auf das Intervall [0,1] mittels deren maximaler und minimaler Ausprägung
    private static float MinMaxScaleZeroToOne(float unscaledVar, float minValue, float maxValue)
    {
        float result;
        if (unscaledVar < minValue && Mathf.Abs(unscaledVar - minValue) < 0.01)
        {
            //Debug.LogFormat("Rundungsfehler?, Input {0} ist etwas kleiner als der min Wert: {1}", unscaledVar, minValue);
            result = 0;
        }
        else if (unscaledVar > maxValue && Mathf.Abs(unscaledVar - maxValue) < 0.01)
        {
            //Debug.LogFormat("Rundungsfehler?, Input {0} ist etwas größer als der max Wert: {1}", unscaledVar, maxValue);
            result = 1;
        }
        else if ((unscaledVar > maxValue && Mathf.Abs(unscaledVar - maxValue) > 0.01) || (unscaledVar < minValue && Mathf.Abs(unscaledVar - minValue) > 0.01))
        {
            Debug.LogErrorFormat("ACHTUNG: Rundungsfehler?, Input {0} ist DEUTLICH außerhalb Interhalb [min,max]: [{1},{2}]", unscaledVar, minValue, maxValue);
            result = (unscaledVar - minValue) / (maxValue - minValue);
        }

        else
        {
            result = (unscaledVar - minValue) / (maxValue - minValue);
        }
        if (result < 0 || result > 1)
        {

            Debug.LogError("***Min Max Scaler konnte den Wert nicht zwischen 0 und 1 legen: " + result);
            Debug.LogErrorFormat("unscaled = {0} - min = {1} / max = {2} - min", unscaledVar, minValue, maxValue);
        }
        return result;
    }

    public void ObserveLikeUnityExampleMod()
    {
        //Wie Unity 3D Ball Beispiel: alle Werte sollen zwischen 0 und 1 liegen

        //Quaternion gibt Euler Werte mit min=0 und max=360 aus
        AddVectorObs(MinMaxScaleZeroToOne((plateTransform.rotation.eulerAngles.z), 0f, 360f));
        AddVectorObs(MinMaxScaleZeroToOne((plateTransform.rotation.eulerAngles.x), 0f, 360f));
        Vector3 ballToTransformPositionVector = ballTransform.position - plateTransform.position;
        AddVectorObs(ballToTransformPositionVector.normalized);
        AddVectorObs(ballRgBody.velocity.normalized);
        AddVectorObs(carRgBody.velocity.normalized);    //Richtungsvektor des Autos  könnte ersetzt werden durch Steigung + motortorque
        AddVectorObs(MinMaxScaleZeroToOne(carControllerScript.angle, -sharedData.maxWheelAngle, sharedData.maxWheelAngle));
        AddVectorObs(carTransform.forward.normalized);
    }


    public void ObserveTorqueParametersNoVelocityVectors()
    {
        //Wie Unity 3D Ball Beispiel: alle Werte sollen zwischen 0 und 1 liegen

        //Quaternion gibt Euler Werte mit min=0 und max=360 aus
        AddVectorObs(MinMaxScaleZeroToOne((plateTransform.rotation.eulerAngles.z), 0f, 360f));
        AddVectorObs(MinMaxScaleZeroToOne((plateTransform.rotation.eulerAngles.x), 0f, 360f));
        Vector3 ballToTransformPositionVector = ballTransform.position - plateTransform.position;
        AddVectorObs(ballToTransformPositionVector.normalized);
        //AddVectorObs(ballRgBody.velocity.normalized);
        //AddVectorObs(carRgBody.velocity.normalized);    //Richtungsvektor des Autos  könnte ersetzt werden durch Steigung + motortorque
        AddVectorObs(MinMaxScaleZeroToOne(carControllerScript.angle, -sharedData.maxWheelAngle, sharedData.maxWheelAngle));
        AddVectorObs(MinMaxScaleZeroToOne(carControllerScript.torque, 0f, sharedData.maxTorque));
        AddVectorObs(MinMaxScaleZeroToOne(carControllerScript.handBrake, 0f, carControllerScript.brakeTorque));

        AddVectorObs(carTransform.forward.normalized);
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


        //AddVectorObs(carRgBody.velocity.normalized);
        /*xVel.text = carRgBody.velocity.x.ToString();
        yVel.text = carRgBody.velocity.y.ToString();
        zVel.text = carRgBody.velocity.z.ToString();*/

        //AddVectorObs(plateTransform.rotation.eulerAngles);
        //AddVectorObs(ballTransform.position);

        //Wie Unity 3D Ball Beispiel:
        ObserveLikeUnityExampleMod();
        //ObserveLikeUnityExample();


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
    public float negativeRewardsThisRound = 0;
    public float positiveRewardsThisRound = 0;
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        bool takeAktion = sharedData.TrainingMode || sharedData.plateAutopilot; //reine Simulation der Belohnungen falls Autopilot den Teller nicht steuern soll


        if (trainingRouteFinished)    //Route zu Ende geschafft -> Reset auf Ursprungsposition wird von AlternateCarController gemacht -> nachdem Done (unten) Reset an Stelle gemacht hat
        {
            positiveRewards += sharedData.incentiveFinishedRoute;
            positiveRewardsThisRound += sharedData.incentiveFinishedRoute;

            if (takeAktion)
            {
                AddReward(sharedData.incentiveFinishedRoute);
                Done();
                //trainingRouteFinished = false;
                //sharedData.trainingRouteNeedsUpdate = false;
            }
        }


        //Rewards...:
        //...harte Strafe für herunterfallen des Balles
        //if(ballHeruntergefallen) //evtl. in separaten Check umwandeln -> paralle Fahrzeuge Training, würde einen Reset auslösen sobald irgendein Auto Mist baut
        //müsste im debug Modus ausführen damit das Spiel nicht schnell zu ende ist wenn die leben leer sind
        //Debug.Log("******LostLife =" + LostLife);
        //(ballTransform.position.y < 0)
        if (LostLife || ((ballTransform.position.y < 0) && ballAbstandZuTellermitte > 3f))    //Leben verloren -> Reset
        {
            negativeRewards += sharedData.incentiveLostLife;
            //Aktualisiere Statistik bevor Belohnug bis zum Lebensverlust aktualisiert wird
            try
            {
               /* foreach (KeyValuePair<string, Vector2> item in sharedData.trainingsStatPerFile)
                {
                    Debug.LogFormat("Habe Key in Stat: {0}", item.Key);
                    Debug.LogFormat("Value: {0}", sharedData.trainingsStatPerFile[item.Key]);
                }*/

                string lastFileName = carControllerScript.trainingFiles[carControllerScript.lastFileNumber].FullName;
                //Debug.Log("Name des Files: " + lastFileName);
                Vector2 ChooseCountAndCumRewards = sharedData.trainingsStatPerFile[lastFileName];
                ChooseCountAndCumRewards.x += 1;
                ChooseCountAndCumRewards.y += positiveRewardsThisRound - negativeRewardsThisRound;
                sharedData.trainingsStatPerFile[carControllerScript.trainingFiles[carControllerScript.lastFileNumber].FullName] = ChooseCountAndCumRewards;


                Debug.LogFormat("Update der Statistik von File: {2} #Lebensverluste: {0} und rewards:{1}", ChooseCountAndCumRewards.x, ChooseCountAndCumRewards.y, lastFileName);

            }
            catch (System.Exception e)
            {
                Debug.LogErrorFormat("Konnte für Strecke {1} Statistik nicht updaten {2}. Fehler {0} ", e.Message, carControllerScript.trainingFiles[carControllerScript.lastFileNumber].FullName, carControllerScript.lastFileNumber);

            }


            LostLife = true;    //falls die zweite Bedinung auftritt soll das auch als Lebensverlust zählen
            //trainingRouteFinished = true;   //neue Route laden nach Lebensverlust ***geht so nicht -> siehe Zeile untendrunter -> macht CarControllerScript
            //carControllerScript.frameCountThisTrainingRoute = carControllerScript.frameDurationThisRoute + 1;
            if (sharedData.TrainingMode)
                Debug.LogFormat("Reset: Ich {0} habe díese Runde so viele Leben gewonnen: {1} \n und bin bei Framecount {2} gescheitert", playerObjectsTransform.name, positiveRewardsThisRound, carControllerScript.frameCountThisTrainingRoute);


            //Debug.Log("habe Leben verloren");

            //TODO: Verhalten ok für debug Mode + PlateAutopilot?
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

            }

        }
        else //Ball noch auf Teller -> weiter
        {
            if (takeAktion)
            {
                /*float[] alteNeigungsvariablen = new float[] { plateXAxis, plateZAxis };
                RotatePlateByMiniSteps(vectorAction[0], vectorAction[1]);
                Debug.LogFormat("Mein Name ist: {6} Die alten Neigungsvars: {0} {1} \n Aktionen: {2} {3} \n Die neuen: {4} {5}", alteNeigungsvariablen[0], alteNeigungsvariablen[1], vectorAction[0], vectorAction[1], plateXAxis, plateZAxis, playerObjectsTransform.name);*/

                //RotatePlateLikeUnityExample(vectorAction[0], vectorAction[1]);  //++++++Stand zuletzt auf ..ByMiniSteps
                RotatePlateByMiniSteps(vectorAction[0], vectorAction[1]);
                //RotatePlatePerDirectFloat(vectorAction[0], vectorAction[1]);

                if (!isTrainingCar) //Das Hauptauto muss die Steuerungsdaten zentral vermerken, damit bei einem Moduswechsel übernommen werden kann
                {
                    sharedData.assistantPlateXAchse = plateXAxis;
                    sharedData.assistantPlateZAchse = plateZAxis;
                }
            }



            //Abstand zwischen Ball und Tellermittelpunkt berechnen
            Vector3 tellermitte = plateTransform.position;
            Vector3 ballposition = ballTransform.position;
            Vector3 verbindungsvektor = ballposition - tellermitte; //kommmt noch raus -> debug
            ballAbstandZuTellermitte = DistanceBetweenTwoPoints(tellermitte, ballposition);
            //Debug.Log("Abstand Ball zu Tellermite: " + ballAbstandZuTellermitte);
            float abstandbestrafung = -sharedData.incentiveFactorDistanceBallToPlateCenter * Mathf.Clamp(1f * ballAbstandZuTellermitte, 0, 1);
            //Debug.Log("**Abstandsbestrafung: " + abstandbestrafung);
            //negativeRewards += abstandbestrafung;
            //negativeRewardsThisRound += abstandbestrafung;

            if (takeAktion)
            {
                //Belohnung, dass der Ball noch auf auf dem Teller ist, vermindert je weiter er von der Mitte entfernt ist
                AddReward(sharedData.incentiveBallStillOnPlate);
                //AddReward(abstandbestrafung);
            }
            positiveRewards += sharedData.incentiveBallStillOnPlate;
            positiveRewardsThisRound += sharedData.incentiveBallStillOnPlate;


        }

        if (!isTrainingCar)
        {
            UpdateTrainingLogs();
        }

    }

    //Unterschiedliche Strategien, um die Agent Action in Tellerneigung umzusetzen:
    private void RotatePlateByMiniStepsProbabilityMod(float x, float z)
    {
        float actionX = Mathf.Clamp(x, -1, 1);
        float actionZ = Mathf.Clamp(z, -1, 1);
        float achsenaenderung = 0.03f;

        //Zufallsfaktor bei Entscheidung ermöglicht auch bei finden einer guten Lösung noch breiter zu suchen:
        float prob = 1 - Random.Range(0, 1);  //Kehre Aktionen von 0 bis Aktionswert (zb 0.3) um

        if (actionX < plateXAxis)
        {
            if (Mathf.Abs(actionX) > prob)
                plateXAxis -= achsenaenderung;
            else
                plateXAxis += achsenaenderung;
        }

        else if (actionX > plateXAxis)
        {
            if (Mathf.Abs(actionX) > prob)
                plateXAxis += achsenaenderung;
            else
                plateXAxis -= achsenaenderung;
        }

        if (actionZ < plateZAxis)
        {
            if (Mathf.Abs(actionX) > prob)
                plateZAxis -= achsenaenderung;
            else
                plateXAxis += achsenaenderung;
        }

        else if (actionZ > plateZAxis)
        {
            if (Mathf.Abs(actionX) > prob)
                plateZAxis += achsenaenderung;
            else
                plateXAxis -= achsenaenderung;
        }
        //Debug.LogFormat("x-Achse: {0}  und y-Achse: {1}", plateXAxis, plateZAxis);
        plateTransform.localRotation = Quaternion.Euler(plateXAxis * sharedData.plateMaxAngle, 0f, plateZAxis * sharedData.plateMaxAngle);
    }

    //Unterschiedliche Strategien, um die Agent Action in Tellerneigung umzusetzen:
    private void RotatePlateByMiniSteps(float x, float z)
    {
        float actionX = Mathf.Clamp(x, -1, 1);
        float actionZ = Mathf.Clamp(z, -1, 1);
        float achsenaenderung = 0.09f;


        if (actionX < plateXAxis)
        {
            plateXAxis -= achsenaenderung * Mathf.Abs(actionX);
        }

        else if (actionX > plateXAxis)
        {
            plateXAxis += achsenaenderung * Mathf.Abs(actionX);

        }

        if (actionZ < plateZAxis)
        {
            plateZAxis -= achsenaenderung * Mathf.Abs(actionZ);
        }

        else if (actionZ > plateZAxis)
        {
            plateZAxis += achsenaenderung * Mathf.Abs(actionZ);
        }
        //Debug.LogFormat("x-Achse: {0}  und y-Achse: {1}", plateXAxis, plateZAxis);
        plateTransform.localRotation = Quaternion.Euler(Mathf.Clamp(plateXAxis, -1, 1) * sharedData.plateMaxAngle, 0f, Mathf.Clamp(plateZAxis, -1, 1) * sharedData.plateMaxAngle);
    }

    private void RotatePlateLikeUnityExample(float actionX, float actionZ)
    {
        if (!isTrainingCar)
            Debug.LogFormat("zAction: {0}, tellerRotation: {1}", actionZ, plateTransform.rotation.z);


        float action_z = 2f * Mathf.Clamp(actionZ, -1f, 1f);
        if ((plateTransform.rotation.z < 0.25f && action_z > 0f) ||
        (plateTransform.rotation.z > -0.25f && action_z < 0f))
        {
            if (!isTrainingCar)
                Debug.Log("RotateZ");
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
