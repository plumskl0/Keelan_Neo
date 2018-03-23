using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlateAgent : Agent {
    private Transform carTransform;
    private ResetCar resetCarScript;
    private Rigidbody carRgBody;
    private Rigidbody ballRgBody;
    private Transform ballTransform;
    private Transform plateTransform;
    private SharedFields sharedData = SharedFields.Instance;



	// Use this for initialization
	void Start () {
        carTransform = GetComponent<Transform>();
        resetCarScript = GetComponent<ResetCar>();
        carRgBody = GetComponent<Rigidbody>();

        GameObject ball = GameObject.Find("Golfball_G");
        ballRgBody = ball.GetComponent<Rigidbody>();
        ballTransform = ball.GetComponent<Transform>();
        plateTransform = GameObject.Find("Teller").GetComponent<Transform>();

	}

    public override void AgentReset()
    {
        //to do: Startposition für jedes Level einfügen
        Scene myScene = SceneManager.GetActiveScene();
        Debug.Log("*********reset Car");
        switch (myScene.name)
        {
            case "Level1":
                resetCarScript.CarReset(95.39f, 1.08926f, 30.4274f, false); //Level1
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

    List<float> obeservation = new List<float>();
    public override void CollectObservations()
    {
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


    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        float abstand = plateTransform.position.y - ballTransform.position.y;
        bool ballHeruntergefallen;
        if( plateTransform.position.y > ballTransform.position.y)  {
            ballHeruntergefallen = true;
        }
        else 
        {
            ballHeruntergefallen = false;
        }

        Debug.LogFormat("plate: {0}, ball: {1}____ abstand: {2}", plateTransform.position.y, ballTransform.position.y, abstand);


        //Rewards...:
        //...harte Strafe für herunterfallen des Balles
        if(ballHeruntergefallen) //evtl. in separaten Check umwandeln -> paralle Fahrzeuge Training, würde einen Reset auslösen sobald irgendein Auto Mist baut
            //müsste im debug Modus ausführen damit das Spiel nicht schnell zu ende ist wenn die leben leer sind
        {
            Done();
            AddReward(-1.0f);
        }
        else
        {
            AddReward(0.1f);
        }

        //Actions -> lenke die Plattform:
        sharedData.assistantPlateXAchse = Mathf.Clamp(vectorAction[0], -1, 1);
        sharedData.assistantPlateZAchse = Mathf.Clamp(vectorAction[1], -1, 1);
    }

}
