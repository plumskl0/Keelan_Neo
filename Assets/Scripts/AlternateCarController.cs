﻿using System;
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

                //nutze die Wiimote, falls eine gefunden wurde
                //if (wiiRemote != null)
                {
                //int ret;
                //do
                //{
                //    ret = wiiRemote.ReadWiimoteData();

                //} while (ret > 0);


                //if (wiiRemote.Button.d_down)
                //{
                //    //Debug.Log("dDown");
                //    moveHorizontal = -1;
                //}
                //if (wiiRemote.Button.d_up)
                //{
                //    //Debug.Log("d_up");
                //    moveHorizontal = 1;
                //}

                //if (wiiRemote.Button.d_left)
                //{
                //    //Debug.Log("dLeft");
                //    moveVertical = 1;
                //}

                //if (wiiRemote.Button.d_right)
                //{
                //    //Debug.Log("dRight");
                //    moveVertical = -1;
                //}

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

    private Vector4 GetKeyboardButtons()
    {
        //Bestimme Tastenwerte
        float moveHorizontal = 0;
        float moveVertical = 0;
        float brake = 0;
        float reset = 0;
        if (Input.GetKey(sharedData.TDownKey))
        {
            //Debug.Log("dDown");
            moveVertical = -1;
        }
        if (Input.GetKey(sharedData.TUpKey))
        {
            //Debug.Log("d_up");
            moveVertical = 1;
        }

        if (Input.GetKey(sharedData.TLeftKey))
        {
            //Debug.Log("dLeft");
            moveHorizontal = -1;
        }

        if (Input.GetKey(sharedData.TRightKey))
        {
            //Debug.Log("dRight");
            moveHorizontal = 1;
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
