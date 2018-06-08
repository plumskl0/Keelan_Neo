using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlateAgent : Agent {
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
    void Start () {
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
                    break;
                case "Level1Training":
                    resetCarScript.CarReset();
                    break;
                default:
                    Debug.LogError("Beim Trainieren des PLate Controllers wurde für das aktuelle Level kein Reset Verhalten definiert");
                    break;
            }
            sharedData.assistantPlateXAchse = 0;
            sharedData.assistantPlateZAchse = 0;
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
        AddVectorObs(carRgBody.velocity);
        AddVectorObs(plateTransform.rotation.eulerAngles);
        AddVectorObs(ballTransform.position);


    }


    float negativeRewards = 0;
    float positiveRewards = 0;
    float positiveRewardsThisRound = 0;
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (sharedData.TrainingMode || sharedData.plateAutopilot)
        {
            if (sharedData.trainingRouteNeedsUpdate)
            {
                AddReward(5.0f);
                Done();
                sharedData.trainingRouteNeedsUpdate = false;
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
            if (sharedData.LostLife)
            {
                Debug.Log("habe Leben verloren");
                Done();
                AddReward(-1.0f);
                negativeRewards -= 1.0f;
                positiveRewardsThisRound = 0;
                sharedData.LostLife = false;

            }
            else
            {
                /* nur vorübergehend raus, unten ist ähnlicher code zu unity bsp
                Debug.Log(textAction);
                float x = Mathf.Clamp(vectorAction[0], -1, 1);
                float z = Mathf.Clamp(vectorAction[1], -1, 1);
                float achsenaenderung = 0.03f;

                if(x< sharedData.assistantPlateXAchse)
                {
                    sharedData.assistantPlateXAchse -= achsenaenderung; 
                }

                else if (x > sharedData.assistantPlateXAchse)
                {
                    sharedData.assistantPlateXAchse += achsenaenderung;
                }

                if (z < sharedData.assistantPlateZAchse)
                {
                    sharedData.assistantPlateZAchse -= achsenaenderung;
                }

                else if (z > sharedData.assistantPlateZAchse)
                {
                    sharedData.assistantPlateZAchse += achsenaenderung;
                }
                */


                //Actions -> lenke die Plattform:
                //sharedData.assistantPlateXAchse = Mathf.Clamp(vectorAction[0], -1, 1);
                //sharedData.assistantPlateZAchse = Mathf.Clamp(vectorAction[1], -1, 1);



                float action_z = 2f * Mathf.Clamp(vectorAction[0], -1f, 1f);
                if ((plateTransform.rotation.z < 0.25f && action_z > 0f) ||
                    (plateTransform.rotation.z > -0.25f && action_z < 0f))
                {
                    plateTransform.Rotate(new Vector3(0, 0, 1), action_z);
                }
                float action_x = 2f * Mathf.Clamp(vectorAction[1], -1f, 1f);
                if ((plateTransform.rotation.x < 0.25f && action_x > 0f) ||
                    (plateTransform.rotation.x > -0.25f && action_x < 0f))
                {
                    plateTransform.Rotate(new Vector3(1, 0, 0), action_x);
                }

                //SetReward(0.1f);
                positiveRewards += 0.01f;
                positiveRewardsThisRound += 0.01f;

                AddReward(0.01f);
            }

            if (positiveRewardsText != null && positiveRewardsThisRoundText != null && negativeRewardsText != null)
            {
                positiveRewardsText.text = positiveRewards.ToString();
                positiveRewardsThisRoundText.text = positiveRewardsThisRound.ToString();
                negativeRewardsText.text = negativeRewards.ToString();
            }
        }
    }



}
