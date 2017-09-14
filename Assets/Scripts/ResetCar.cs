using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetCar : MonoBehaviour {

    public Text resetCarText;

    public Rigidbody ball;
    public BallInZoneCheck ballOnPlateZone;

    public Transform ballResetPosition;

    public static bool debug = true;

    private bool reset;

    private AlternateCarController carControl;
    private SharedFields sharedData = SharedFields.Instance;

    private void Start()
    {
        carControl = GetComponent<AlternateCarController>();
        clearResetText();
    }

    void Update()
    {
        if (sharedData.CarReset && !debug)
        {
            setResetText();

            sharedData.SetPlayerControl(false);

            reset = isCarResetButtonPressed();
        }
    }

    private void FixedUpdate()
    {
        if (reset)
        {
            // Resets Car
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

            sharedData.SetPlayerControl(true);

            resetBall();

            clearResetText();
            reset = false;
            sharedData.CarReset = false;
        }
    }

    private void resetBall()
    {
        float radius = ball.GetComponent<SphereCollider>().radius;

        // Ball unter beachtung des Radius auf dem Fahrzeug positionieren
        Vector3 pos = ballResetPosition.position;
        pos.Set(pos.x, pos.y + radius, pos.z);
        ball.transform.position = pos;

        // Falls der Ball noch rollt die Geschwindigkeit entfernen
        ball.velocity = Vector3.zero;
    }

    private bool isCarResetButtonPressed()
    {
        return (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            && Input.GetKey(KeyCode.R);
    }

    private void setResetText()
    {
        resetCarText.text = "Reset mit Shift+R";
    }

    private void clearResetText()
    {
        resetCarText.text = " ";
    }
}
