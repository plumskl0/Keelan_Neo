using System.Collections;
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
    
    private SharedFields()
    {
        LostLife = false;
        CarReset = false;
        BallReset = false;
        PayedCoin = false;
        CoinCount = 0;
    }


    private float timeNeededToLastLevel = 0;
    public const float maxSpeed = 30f;  //der Sprachassistent manipuliert im Autopilot die maximale Geschwindigkeit -> das ist das Backup wenn er die Kontrolle wieder abgibt
    public float currentMaxSpeed = 30f;
    public float currentSpeed = 0;
 


    // Hat der Ball den Boden berührt -> ein Leben verlieren
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
    public bool CarReset { get; set; }

    // Hat der Teller den Boden berührt -> ein Leben verlieren aber ball nicht reseten
    public bool BallReset { get; set; }

    // Wurde für ein Special mit einer Münze bezahlt
    public bool PayedCoin { get; set; }

    // Kosten eines Specials
    private int cost = 1;


    // Anzahl der gesammelten Münzen
    public int CoinCount { get; set; }


    public float TimeNeededToLastLevel
    {
        get
        {
            return timeNeededToLastLevel;
        }

        set
        {
            timeNeededToLastLevel = value;
        }
    }




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
    public const string VoiceAssistantControl = "FahrtrichtungUeberDenSprachassistentenSteuern";
    private string selectedControl = MTControl;
    private bool playerControl = false;
    //Vom Sprachassistenten simulierte Movement Achsenbelegung
    private float assistantXAchse = 0;
    private float assistantYAchse = 0;

    public float AssistantXAchse
    {
        get
        {
            return assistantXAchse;
        }

        set
        {
            if (value > 1f)
            {
                assistantXAchse = 1;
            }
            else if (value < -1f)
            {
                assistantXAchse = -1;
            }
            else
            {
                assistantXAchse = value;
            }
        }
    }

    public float AssistantYAchse
    {
        get
        {
            return assistantYAchse;
        }

        set
        {
            if (value > 1f)
            {
                assistantYAchse = 1;
            }
            else if (value < -1f)
            {
                assistantYAchse = -1;
            }
            else
            {
                assistantYAchse = value;
            }
        }
    }


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

    public void payCoins()
    {
        cost = 1;
    }

    public void payedCoins(int amount)
    {
        cost = amount;
    }

    public int getPayedCoins()
    {
        return cost;
    }
}
