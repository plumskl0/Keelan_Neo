using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetCar : MonoBehaviour {


    public Text resetCarText;

    private AlternateCarController carControl;

    private void Start()
    {
        carControl = GetComponent<AlternateCarController>();
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
