using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WiimoteApi;

public class AlternateCarController : MonoBehaviour {

    public float maxAngle = 30f;
    public float maxTorque = 300f;
    public float brakeTorque = 30000f;

    public float maxSpeed = 30f;

    public Transform[] wheels;

    private const int FRONT_LEFT = 0;
    private const int FRONT_RIGHT = 1;
    private const int REAR_RIGHT = 2;
    private const int REAR_LEFT = 3;

    private float criticalSpeed = 5f;
    private int stepsBelow = 5;
    private int stepsAbove = 1;

    private Rigidbody rb;

    private Boolean playerControl;
    private GameObject wiiMoteRef;
    private wiiKalibrierung wiiDaten;
    public Wiimote wiiRemote;
    private SharedFields sharedData = SharedFields.Instance;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        
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
        //Debug.Log("ENDE ------- Übertrage WiimoteReferenz");
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        getCollider(FRONT_LEFT).ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);

        float angle;
        float torque;
        float handBrake;
        
        if (sharedData.GetPlayerControl())
        {
            if (sharedData.SelectedControl == SharedFields.WiiControl && wiiRemote != null)
                {
                Vector2 buttonMovement = wiiDaten.getButtons();
                float moveHorizontal = buttonMovement.x;
                float moveVertical = buttonMovement.y;

                torque = moveVertical * maxTorque;
                    angle = moveHorizontal * maxAngle;
                    handBrake = wiiRemote.Button.a ? brakeTorque : 0;

                }

                //ansonsten nutzen die Tastatursteuerung
                else
                {
                //angle = maxAngle * Input.GetAxis("Horizontal");
                //torque = maxTorque * Input.GetAxis("Vertical");
                //handBrake = Input.GetKey(KeyCode.Space) ? brakeTorque : 0;
                Vector2 keyboardMovement = GetKeyboardButtons();
                angle = maxAngle * keyboardMovement.x;
                //Debug.Log("horizontal = " +keyboardMovement.x);
                torque = maxTorque * keyboardMovement.y;
                //Debug.Log("vertical = " + keyboardMovement.y);
                handBrake = Input.GetKey(sharedData.TBrakeKey) ? brakeTorque : 0;
            }

        } 
        else   //stellt die Reifen neutral wenn keine playerControll gegeben wird
        {
            angle = 0;
            torque = 0;
            handBrake = brakeTorque;
        }


        // Höchstgeschwindigkeit des Autos
        if (rb.velocity.magnitude >= maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
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

    public const String no = "None";
    public const String up = "UpWasPressedLastFrame";
    public const String down = "DownWasPressedLastFrame";
    public const String left = "LeftWasPressedLastFrame";
    public const String right = "RightWasPressedLastFrame";

    private String lastVerticalAxisButtonPressend = no;
    private String lastHorizontalAxisButtonPressend = no;
    float moveHorizontal = 0;
    float moveVertical = 0;
    public float stepsToAxisMax= 0.01f;

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
                moveVertical = 0;
            }

            else if (moveVertical > -1)
            {
               // Debug.Log("move")
                moveVertical -= stepsToAxisMax;
            }
            else
            {
                Debug.Log("dDown");
                moveVertical = -1;
            }
            lastVerticalAxisButtonPressend = down;
            axisButtonPressedThisFrame = true;
            Debug.Log("moveVertical ist jetzt: " + moveVertical);
        }
        if (Input.GetKey(sharedData.TUpKey))
        {
            if (lastVerticalAxisButtonPressend != up)
            {
                moveVertical = 0;
            }

            else if (moveVertical < 1)
            {
                // Debug.Log("move")
                moveVertical += stepsToAxisMax;
            }
            else
            {
                Debug.Log("dUp");
                moveVertical = 1;
            }
            lastVerticalAxisButtonPressend = up;
            axisButtonPressedThisFrame = true;
            Debug.Log("moveVertical ist jetzt: " + moveVertical);
        }

        if (Input.GetKey(sharedData.TLeftKey))
        {
            if (lastHorizontalAxisButtonPressend != left)
            {
                moveHorizontal = 0;
            }

            else if (moveHorizontal > -1)
            {
                // Debug.Log("move")
                moveHorizontal -= stepsToAxisMax;
            }
            else
            {
                Debug.Log("dleft");
                moveHorizontal = -1;
            }
            lastHorizontalAxisButtonPressend = left;
            axisButtonPressedThisFrame = true;
            Debug.Log("moveHorizontal ist jetzt: " + moveHorizontal);
        }

        if (Input.GetKey(sharedData.TRightKey))
        {
            if (lastHorizontalAxisButtonPressend != right)
            {
                moveHorizontal = 0;
            }

            else if (moveHorizontal < 1)
            {
                // Debug.Log("move")
                moveHorizontal += stepsToAxisMax;
            }
            else
            {
                Debug.Log("dleft");
                moveHorizontal = 1;
            }
            lastHorizontalAxisButtonPressend = right;
            axisButtonPressedThisFrame = true;
            Debug.Log("moveHorizontal ist jetzt: " + moveHorizontal);
        }

        if(!axisButtonPressedThisFrame)
        {
            Debug.Log("Setze Achsen zurück");
            lastVerticalAxisButtonPressend = no;
            moveHorizontal = 0;
            moveVertical = 0;
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

        return new Vector4(moveHorizontal, moveVertical, brake, reset);
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
