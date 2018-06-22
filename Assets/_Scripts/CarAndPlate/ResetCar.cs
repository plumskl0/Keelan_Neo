using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetCar : MonoBehaviour {

    public Text resetCarText;

    private Rigidbody ballRGBody;
    private Rigidbody carRGBody;

    public Transform ballResetPosition;

    //public static bool debug = true;

    private bool reset;

    private AlternateCarController carControl;
    private PlateController pc;
    private SharedFields sharedData = SharedFields.Instance;

    private void Start()
    {
        carControl = GetComponent<AlternateCarController>();
        carRGBody = GetComponent<Rigidbody>();
        //ballRGBody = GameObject.Find("Golfball_G").GetComponent<Rigidbody>();
        if(gameObject.CompareTag("Player"))
        {
            ballRGBody = gameObject.transform.parent.Find("Golfball_G").GetComponent<Rigidbody>();
        }
        else if(gameObject.CompareTag("Trainingsfahrzeug"))
        {
            ballRGBody = gameObject.transform.parent.Find("Trainingsball").GetComponent<Rigidbody>();
        }
        
        else
        {
            Debug.LogError("Reset Script: Ich habe keinen Ball gefunden");
        }
        clearResetText();
    }

    void Update()
    {
        if (sharedData.CarResetNeeded && !sharedData.debugMode &&!sharedData.TrainingMode)
        {
            setResetText();

            sharedData.SetPlayerControl(false);

            reset = isCarResetButtonPressed();
        } else if (sharedData.debugMode)
        {
            reset = isCarResetButtonPressed();
        }
    }

    private void FixedUpdate()
    {
        if (reset)
        {
            // Resets Car
            CarReset();

        }
    }



    
    public void CarReset (float _x=0f, float _y=0f, float _z= 0f, bool _currentPostionReset = true, float _resetRotationY = 0f)
    {
        Vector3 carResetPosition = new Vector3(0f,0f,0f);
        if (_currentPostionReset)   //gleiche Position und yRotation
        {
            carResetPosition = transform.position;
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
            if (!sharedData.TrainingMode)    //Im Trainingsmode darf die Geschwindigkeit nicht vom Auto, da die Keystrokefolge sonst zu einer anderen Strecke führt
            {
                carRGBody.velocity = Vector3.zero;
            }
        }
        else
        {
            carResetPosition.x = _x;
            carResetPosition.y = _y;
            carResetPosition.z = _z;
            carRGBody.velocity = Vector3.zero;
            transform.rotation = Quaternion.Euler(0f, _resetRotationY, 0f);
        }

        carResetPosition.y = carResetPosition.y * transform.localScale.y;

        transform.position = carResetPosition;
        //transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        

        
        if (!sharedData.TrainingMode && transform.CompareTag("Player")) //belasse die zentralen Informationen, falls Trainingsfahrzeuge die Funktion nutzen
        {
            sharedData.SetPlayerControl(true);
            if (sharedData.BallResetNeeded)
                ResetBall();
            reset = false;

            clearResetText();
            sharedData.CarResetNeeded = false;
        }


    }



    public void ResetBall()
    {
        float radius = ballRGBody.GetComponent<SphereCollider>().radius;

        // Ball unter beachtung des Radius auf dem Fahrzeug positionieren
        Vector3 pos = ballResetPosition.position;
        pos.Set(pos.x, pos.y + radius, pos.z);
        ballRGBody.transform.position = pos;

        // Falls der Ball noch rollt die Geschwindigkeit entfernen
        //ballRGBody.velocity = Vector3.zero;
        ballRGBody.velocity = carRGBody.velocity;   //führt dazu, dass ein stehendes Auto einen stehenden Ball bekommt, aber auch ein fahrendes Auto einen erfolgreichen Reset macht

        if (transform.CompareTag("Player"))
        {
            sharedData.BallResetNeeded = false;
        }
    }

    private bool isCarResetButtonPressed()
    {
        return (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            && Input.GetKey(sharedData.TResetKey);
    }

    private void setResetText()
    {
        resetCarText.text = "Reset mit Shift+" +sharedData.TResetKey.ToString();
    }

    private void clearResetText()
    {
        resetCarText.text = " ";
    }
}
