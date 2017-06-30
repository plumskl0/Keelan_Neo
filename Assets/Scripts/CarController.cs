using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using WiimoteApi;

public class CarController : MonoBehaviour
{

    private const int FRONT_RIGHT = 1;
    private const int FRON_LEFT = 0;

    public Transform[] wheels;
    public float motorPower = 150.0f;
    public float maxTurn = 25.0f;

    float torque = 0.0f;
    float brake = 0.0f;
    float wheelTurn = 0.0f;

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

        myRigidbody = this.gameObject.GetComponent<Rigidbody>();
        myRigidbody.centerOfMass = new Vector3(0, 0.0f, 0.0f);
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

            torque = moveVertical * motorPower;
            wheelTurn = moveHorizontal* maxTurn;
            brake = wiiRemote.Button.a ? myRigidbody.mass * 0.5f : 0.0f;


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
			torque = Input.GetAxis("Vertical") * motorPower;
			wheelTurn = Input.GetAxis("Horizontal") * maxTurn;
			brake = Input.GetKey("space") ? myRigidbody.mass * 0.5f : 0.0f;
        }
		
		
        //turn collider
        getCollider(0).steerAngle = wheelTurn;
        getCollider(1).steerAngle = wheelTurn;
        
        //turn wheels
        wheels[0].localEulerAngles = new Vector3(wheels[0].localEulerAngles.x,
            getCollider(0).steerAngle - wheels[0].localEulerAngles.z + 90,
            wheels[0].localEulerAngles.z);
        wheels[1].localEulerAngles = new Vector3(wheels[1].localEulerAngles.x,
            getCollider(1).steerAngle - wheels[1].localEulerAngles.z + 90,
            wheels[1].localEulerAngles.z);

        //spin wheels
        wheels[0].Rotate(0, -getCollider(0).rpm / 60 * 360 * Time.deltaTime, 0);
        wheels[1].Rotate(0, -getCollider(1).rpm / 60 * 360 * Time.deltaTime, 0);
        wheels[2].Rotate(0, -getCollider(2).rpm / 60 * 360 * Time.deltaTime, 0);
        wheels[3].Rotate(0, -getCollider(3).rpm / 60 * 360 * Time.deltaTime, 0);

        //brakes
        if (brake > 0.0f)
        {
            getCollider(0).brakeTorque = brake;
            getCollider(1).brakeTorque = brake;
            getCollider(2).brakeTorque = brake;
            getCollider(3).brakeTorque = brake;
            getCollider(2).motorTorque = 0.0f;
            getCollider(3).motorTorque = 0.0f;
        }
        else
        {
            getCollider(0).brakeTorque = 0.0f;
            getCollider(1).brakeTorque = 0.0f;
            getCollider(2).brakeTorque = 0.0f;
            getCollider(3).brakeTorque = 0.0f;
            getCollider(2).motorTorque = torque;
            getCollider(3).motorTorque = torque;
        }
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
        void InitWiimotes()
    {
        WiimoteManager.FindWiimotes(); // Poll native bluetooth drivers to find Wiimotes

        foreach (Wiimote remote in WiimoteManager.Wiimotes)
        {
            // Do stuff.
            //Console.WriteLine("Gefunden");

            //LEDs markieren, dass WiiRemote verbunden ist
            wiiRemote = remote;
            //remote.RumbleOn = true;
            remote.SendStatusInfoRequest();
            wiiRemote.SendPlayerLED(true, false, false, false);
            //Thread.Sleep(1000);
            //remote.RumbleOn = false;
            remote.SendStatusInfoRequest();
            remote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL);
        }
    }

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
