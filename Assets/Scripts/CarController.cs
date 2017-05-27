using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using WiimoteApi;

public class CarController : MonoBehaviour
{

    private const int FRONT_RIGHT = 0;
    private const int FRON_LEFT = 1;

    public Transform[] wheels;
    public float motorPower = 150.0f;
    public float maxTurn = 25.0f;

    float torque = 0.0f;
    float brake = 0.0f;
    float wheelTurn = 0.0f;

    Rigidbody myRigidbody;
    private Wiimote wiiRemote;
    private float horicontal_tilt;
    private float vertical_tilt;
    private float rotate_horicontal = 0;
    private bool rotate_init = false;
    private int startup = 0; //die ersten Accel Werte der Wiimote weichen stark von den folgenden ab -> überspringe rotation für die ersten 10 Frames

    // Use this for initialization
    void Start()
    {
        // Hiermit kann man evtl. die beschleunigung etwas besser anpassen, damit das
        // Gefühl etwas besser wird beim fahren. Einfach in Doku nachlesen für genauere Infos.
        // m_Wheels[0].ConfigureVehicleSubsteps(criticalSpeed, stepsBelow, stepsAbove);

        myRigidbody = this.gameObject.GetComponent<Rigidbody>();
        myRigidbody.centerOfMass = new Vector3(0, 0.0f, 0.0f);
        wiiRemote = wiiKalibrierung.wiiRemote;
        //InitWiimotes();
        /*if (wiiRemote != null)
        {
            float accel_x;
            float accel_y;
            float accel_z;

            float[] accel = wiiRemote.Accel.GetCalibratedAccelData();
            accel_x = accel[0];
            accel_y = -accel[2];
            accel_z = -accel[1];
            Debug.Log(accel[1]);
            Debug.Log(accel_z);
            horicontal_tilt = accel_z;
            Debug.Log(horicontal_tilt);
            Debug.Log(this.GetAccelVector());
            //vertical_tilt = 
        }*/
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //nutze die Wiimote, falls eine gefunden wurde
        if (wiiRemote != null)
        {
            float moveHorizontal = 0;
            float moveVertical = 0;
            float moveUp = 0;

            /*      -> Code für Tilt
            float [] wiiSteuerkreuz = wiiRemote.Accel.GetCalibratedAccelData();
            moveHorizontal = wiiSteuerkreuz[0];
            moveVertical = wiiSteuerkreuz[2];
            Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
            */

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
			
            instantePower = moveVertical * motorPower;
            wheelTurn = moveHorizontal* maxTurn;
            brake = wiiRemote.Button.a ? myRigidbody.mass * 0.5f : 0.0f;

            if (startup < 10)
            {
                startup++;
                Debug.Log("startup");
            }

            else
            {

                float accel_x;
                float accel_y;
                float accel_z;

                float[] accel = wiiRemote.Accel.GetCalibratedAccelData();
                accel_x = accel[0];
                accel_y = -accel[2];
                accel_z = accel[1];
                /*
                Debug.Log("x: = " + accel_x);
                Debug.Log("y: = " + accel_y);
                Debug.Log("z: = " + accel_z);
                */

                //Init Accel
                /*if(rotate_init == false)
                {
                    horicontal_tilt = accel_z;
                    rotate_init = true;
                }*/

                Transform cage = transform.Find("Cage");
                //die y-Achse der Wiimote muss auf die Z-Achse des Cages gemapt werden
                //float horicontal_tiltNew = accel_z;
                //Debug.Log("newTilt: " + horicontal_tiltNew);
                //Debug.Log("oldTilt: " + horicontal_tilt);
                //float horicontal_diff = horicontal_tilt - horicontal_tiltNew;
                //horicontal_tilt = horicontal_tiltNew;
                //Debug.Log("diff: " + horicontal_diff);
                //rotate_horicontal += horicontal_tilt;
                //Debug.Log("rotate: " + rotate_horicontal);
                //cage.Rotate(0, 0, 5 * horicontal_diff);

                cage.Rotate(0, 0, -accel_z);
                Debug.Log(this.GetAccelVector());


                /*// die x-Achse auf die x-Achse
                vertical_tilt = 0.1f * accel_x;
                //cage.Rotate(0.05f, 0, 0);
                //cage.Rotate(horicontal_tilt, 0,0 );
                Debug.Log(this.GetAccelVector());
                //cage.Rotate(this.GetAccelVector());
                */
            }
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
