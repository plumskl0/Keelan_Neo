using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pauseMenu : MonoBehaviour {

    private AlternateCarController carControl;
    private PickupLogic pul;
    private Boolean pauseButtonPressed = false;
    public Boolean weiterspielen = false;
    public Canvas pauseMenuCanvas;
    private SharedFields sharedData = SharedFields.Instance;

    private void Start()
    {
        carControl = GetComponent<AlternateCarController>();
        pul = GetComponent<PickupLogic>();
    }

    // Update is called once per frame
    void Update () {
        pauseButtonPressed = Input.GetKey(KeyCode.Escape);
		if(pauseButtonPressed)
        {
            sharedData.SetPlayerControl(false);
            sharedData.SetCursorVisible(true);
            pul.stopTimer();
            pauseMenuCanvas.enabled = true;
        }
	}

    public void Continue()
    {
        pul.startTimer();

        sharedData.SetPlayerControl(true);
        sharedData.SetCursorVisible(false);
        pauseButtonPressed = false;
        pauseMenuCanvas.enabled = false;
    }
}
