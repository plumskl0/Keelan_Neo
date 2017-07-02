using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetCar : MonoBehaviour {

    public Text resetCarText;
    public BallInZoneCheck ballOnPlateZone;

    public bool debug = false;

    private bool resetCar;

    private AlternateCarController carControl;

    private void Start()
    {
        carControl = GetComponent<AlternateCarController>();
        clearResetText();
    }

    void Update()
    {
        if (!ballOnPlateZone.isBallInZone && !debug)
        {
            setResetText();

            carControl.setPlayerControl(false);

            resetCar = isCarResetButtonPressed();
        }
        else
        {

        }
    }

    private void FixedUpdate()
    {
        Debug.Log( transform.rotation.y);
        if (resetCar)
        {
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

            carControl.setPlayerControl(true);

            clearResetText();
            resetCar = false;
        }
    }

    private bool isCarResetButtonPressed()
    {
        return (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            && Input.GetKey(KeyCode.R);
    }

    private void setResetText()
    {
        resetCarText.text = "Um das Auto wieder aufzurichten Shift+R drücken.";
    }

    private void clearResetText()
    {
        resetCarText.text = " ";
    }
}
