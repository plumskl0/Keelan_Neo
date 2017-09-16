﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WiimoteApi;



public class SharedFields {

    private static SharedFields instance;

    public static SharedFields Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SharedFields();
            }
            return instance;
        }
    }
    
    private SharedFields() {}

    // Hat der Ball den Boden berührt -> ein leben verlieren
    private bool lostLife = false;
    public bool LostLife { get; set; }
    public string SelectedControl
    {
        get
        {
            return selectedControl;
        }

        set
        {
            selectedControl = value;
        }
    }

    public KeyCode TUpKey
    {
        get
        {
            return tUpKey;
        }

        set
        {
            tUpKey = value;
        }
    }

    public KeyCode TDownKey
    {
        get
        {
            return tDownKey;
        }

        set
        {
            tDownKey = value;
        }
    }

    public KeyCode TLeftKey
    {
        get
        {
            return tLeftKey;
        }

        set
        {
            tLeftKey = value;
        }
    }

    public KeyCode TRightKey
    {
        get
        {
            return tRightKey;
        }

        set
        {
            tRightKey = value;
        }
    }

    public KeyCode[] TmpMouseControls
    {
        get
        {
            return tmpMouseControls;
        }

        set
        {
            tmpMouseControls = value;
        }
    }

    public KeyCode TResetKey
    {
        get
        {
            return tResetKey;
        }

        set
        {
            tResetKey = value;
        }
    }

    public KeyCode TBrakeKey
    {
        get
        {
            return tBrakeKey;
        }

        set
        {
            tBrakeKey = value;
        }
    }

    // Hat der Ball den Boden berührt -> Fahrzeug reset
    private bool carReset = false;
    public bool CarReset { get; set; }

    // Wurde für ein Special mit einer Münze bezahlt
    private bool payedCoin = false;
    public bool PayedCoin { get; set; }

    // Zum übertragen der Münzenmenge in das nächste Level
    private int coinTransferCount = 0;
    public int CoinTransferCount { get; set; }

    // Spielersteuerung ein und abschalten
    //Tasteneinstellungen Speicher Tastatur
    private KeyCode[] tmpMouseControls = new KeyCode[6]; // 0 -> up, 1 -> down, 2-> left, 3 -> right, 4 -> reset, 5 -> brake
    private KeyCode tUpKey = KeyCode.W;
    private KeyCode tDownKey = KeyCode.S;
    private KeyCode tLeftKey = KeyCode.A;
    private KeyCode tRightKey = KeyCode.D;
    private KeyCode tResetKey = KeyCode.R;
    private KeyCode tBrakeKey = KeyCode.Space;

    //Tasteneinstellungen Speicher Wiimote
    /*private ButtonData wUpKey = ButtonData.a;
    private ButtonData wDownKey = KeyCode.S;
    private ButtonData wLeftKey = ButtonData.;
    private ButtonData wRightKey = KeyCode.D;*/

    //Controllerauswahl
    public const string MTControl = "MausUndTastaturKontrolle";
    public const string WiiControl = "WiimoteKontrolle";
    private string selectedControl = MTControl;
    private bool playerControl = false;

    // Mausempfindlichkeit
    public float sensitivity = 5f;

    public bool GetPlayerControl()
    {
        return playerControl;
    }

    public void SetPlayerControl(bool b)
    {
        playerControl = b;
    }

	public void SetCursorVisible(bool b)
    {
        Cursor.visible = b;
    }
}
