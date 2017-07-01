using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using WiimoteApi;

public class CarController : MonoBehaviour
{
    //Variablen aus AlternateCarController
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
    //****Ende******


    

    //public int tellerSmoothinFactor = 5;    //glättet Messwerte über die angegebene Anzahl von Frames
    //public int minAenderungswinkel = 5;     //nur wenn sich die Neigung, um mindestens diesen Winkel aendert wird die Tellerneigung veraendert
    Rigidbody myRigidbody;
    public static Wiimote wiiRemote;
    private float horicontal_tilt;
    private float vertical_tilt;
    private float rotate_horicontal = 0;

    // Use this for initialization
    void Start()
    {
        // Hiermit kann man evtl. die beschleunigung etwas besser anpassen, damit das
        // Gefühl etwas besser wird beim fahren. Einfach in Doku nachlesen für genauere Infos.
        // m_Wheels[0].ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);

        rb = this.gameObject.GetComponent<Rigidbody>();
        wiiRemote = wiiKalibrierung.wiiRemote;
    }

    // Update is called once per frame

    ////GG Zittern:
    ////Anstatt die Neigung des Tellers jeden Frame zu ändern bestimmt tellerSmoothinFactor die Häufigkeit
    //private int frameCount = 0; //Zähler für Frames -> Neigungsänderung wenn tellerSmoothinFactor erreicht
    //private float frameSummeZ = 0;
    //private float frameSummeX = 0;

    void FixedUpdate()
    {
        float angle;
        float torque;
        float handBrake;
        //nutze die Wiimote, falls eine gefunden wurde
        if (wiiRemote != null)
        {
            float moveHorizontal = 0;
            float moveVertical = 0;

            int ret;
            do
            {
                ret = wiiRemote.ReadWiimoteData();

            } while (ret > 0);


            if (wiiRemote.Button.d_down)
            {
                //Debug.Log("dDown");
                moveHorizontal = -1;
            }
            if (wiiRemote.Button.d_up)
            {
                //Debug.Log("d_up");
                moveHorizontal = 1;
            }

            if (wiiRemote.Button.d_left)
            {
                //Debug.Log("dLeft");
                moveVertical = 1;
            }

            if (wiiRemote.Button.d_right)
            {
                //Debug.Log("dRight");
                moveVertical = -1;
            }

            torque = moveVertical * maxTorque;
            angle = moveHorizontal* maxAngle;
            handBrake = wiiRemote.Button.a ? brakeTorque : 0;
            


            //    float accel_x;
            //    float accel_y;
            //    float accel_z;

            //    float[] accel = wiiRemote.Accel.GetCalibratedAccelData();
            //    accel_x = accel[0];
            //    accel_y = -accel[2];
            //    accel_z = accel[1];

            //    Transform cage = transform.Find("Cage");

            //    //Ueberspringe #Tiltaenderungen definiert durch tellerSmoothingFactor
            //    //Berechne den Mittelwert der uebersprungenen Werte

            //    frameSummeZ += -accel_z;
            //    frameSummeX += -accel_x;
            //    Debug.Log("frameSummeZ: " + frameSummeZ);
            //    Debug.Log("frameSummeX: " + frameSummeX);
            //    frameCount++;


            //    //Ansonsten ändere Tilt
            //    if (!(frameCount < tellerSmoothinFactor))
            //    {
            //        //Cage nach links und rechts kippen
            //        float z = (frameSummeZ/tellerSmoothinFactor) * 90;
            //        //cage.localRotation = Quaternion.Euler(0f, 0f, z);

            //        //Cage nach vorne und hinten kippen
            //        float x = (frameSummeX / tellerSmoothinFactor) * 90;
            //        Debug.Log("Verändere Tellerwinkel");
            //        //cage.localRotation = Quaternion.Euler(0f, 0f, z);

            //        //Ignoriere Ausreiser indem nur Neigungen berücksichtigt werden, welche den Winkel um mindestens x Grad veraendert
            //        float aktuelleNeigungX = cage.eulerAngles.x;
            //        float aktuelleNeigungZ = cage.eulerAngles.z;

            //        if (Math.Abs(aktuelleNeigungX - x) > minAenderungswinkel || Math.Abs(aktuelleNeigungZ - z) > minAenderungswinkel)
            //        {
            //            cage.localRotation = Quaternion.Euler(x, 0f, z);
            //            Debug.Log(this.GetAccelVector());
            //        }

            //        //Setze Mittelwert und Frame Count auf 0 fuer naechste Runde auf 0
            //        frameSummeX = 0;
            //        frameSummeZ = 0;
            //        frameCount = 0;
            //    }

        }

        //ansonsten nutzen die Tastatursteuerung
        else
        {
            angle = maxAngle * Input.GetAxis("Horizontal");
            torque = maxTorque * Input.GetAxis("Vertical");
            handBrake = Input.GetKey(KeyCode.Space) ? brakeTorque : 0;
        }


        getCollider(FRONT_LEFT).ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);

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

    void OnApplicationQuit()
    {
        if (wiiRemote != null)
        {
            WiimoteManager.Cleanup(wiiRemote);
            wiiRemote = null;
        }
    }

        //wiiMoteHilfsmethoden
    //    void InitWiimotes()
    //{
    //    WiimoteManager.FindWiimotes(); // Poll native bluetooth drivers to find Wiimotes

    //    foreach (Wiimote remote in WiimoteManager.Wiimotes)
    //    {
    //        // Do stuff.
    //        //Console.WriteLine("Gefunden");

    //        //LEDs markieren, dass WiiRemote verbunden ist
    //        wiiRemote = remote;
    //        //remote.RumbleOn = true;
    //        remote.SendStatusInfoRequest();
    //        wiiRemote.SendPlayerLED(true, false, false, false);
    //        //Thread.Sleep(1000);
    //        //remote.RumbleOn = false;
    //        remote.SendStatusInfoRequest();
    //        remote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL);
    //    }
    //}

    private Vector3 GetAccelVector()
    {
        float accel_x;
        float accel_y;
        float accel_z;

        float[] accel = wiiRemote.Accel.GetCalibratedAccelData();
        accel_x = accel[0];
        accel_y = -accel[2];
        accel_z = -accel[1];
        //return new Vector3(accel_x, accel_y, accel_z).normalized;
        return new Vector3(accel_x, accel_y, accel_z);
    }
}
