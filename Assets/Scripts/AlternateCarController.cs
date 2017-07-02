using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private bool playerControl;

    // Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        playerControl = true;
	}
    
    // Update is called once per frame
    void FixedUpdate()
    {
        getCollider(FRONT_LEFT).ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);

        //Debug.Log("COM: " + rb.centerOfMass);

        //rb.centerOfMass = new Vector3(0.1f, 0.4f, 0.1f);

        float angle;
        float torque;

        float handBrake;

        if (playerControl)
        {
            angle = maxAngle * Input.GetAxis("Horizontal");
            torque = maxTorque * Input.GetAxis("Vertical");
            handBrake = Input.GetKey(KeyCode.Space) ? brakeTorque : 0;
        } 
        else
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

    public void setPlayerControl(bool b)
    {
        playerControl = b;
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
