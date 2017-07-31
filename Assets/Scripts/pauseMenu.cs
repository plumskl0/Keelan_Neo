﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pauseMenu : MonoBehaviour {

    private AlternateCarController carControl;
    private PickupLogic pul;
    private Boolean pauseButtonPressed = false;
    public Boolean weiterspielen =false;
    public Canvas pauseMenuCanvas;
    private SharedFields sharedData;

    private void Start()
    {
        carControl = GetComponent<AlternateCarController>();
        pul = GetComponent<PickupLogic>();
        sharedData = GetComponent<SharedFields>();
    }

    // Update is called once per frame
    void Update () {
        pauseButtonPressed = Input.GetKey(KeyCode.Escape);
		if(pauseButtonPressed)
        {
            sharedData.SetPlayerControl(false);
            //carControl.setPlayerControl(false);
            pul.stopTimer();
            pauseMenuCanvas.enabled = true;
            //Debug.Log("Pause");
        }
	}

    public void Continue()
    {
        pul.startTimer();

        sharedData.SetPlayerControl(true);
        //carControl.setPlayerControl(false);
        pauseButtonPressed = false;
        pauseMenuCanvas.enabled = false;
    }
}
