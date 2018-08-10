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
        CarResetNeeded = false;
        BallResetNeeded = false;
        PayedCoin = false;
        CoinCount = 0;
    }

    //Start/Reset Positionen der Level:
    public float [] level1ResetPosition = new float[] {95.39f, 0.38f, 30.4274f, 58.077f};


    //Debug Mode Verhalten: 
    // keine Leben verlieren, automatischer Ball Reset, simuliert Bestrafungen des Trainingsmode, Teller und Autosteuerung gemäß Autopilot Werten
    public bool debugMode = false;
    public int currentFrameCount = 0;   //frameCount des Hauptautos auf der gerade gefahrenen Trainingsstrecke -> im Debug Mode für Checkpoints genutzt

    //Vars für Erfassung und Bewertung von Trainingsrouten
    public bool trainingRouteRecordingStopped = false;
    public string trainingRouteDifficulty = "";
    public int checkpointFrameCount = 0;


    public bool nonMovingCar = false;

    private float timeNeededToLastLevel = 0;
    public float maxSpeed = 30f;  //der Sprachassistent manipuliert im Autopilot die maximale Geschwindigkeit -> das ist das Backup wenn er die Kontrolle wieder abgibt
    public float currentMaxSpeed = 30f;  //die zurzeit gesetzte Maximalgeschwindigkeit
    public float currentSpeed = 0;
    public float maxWheelAngle;
    public float maxTorque;
    public float brakeTorque;

    //Controllerauswahl
    public const string MTControl = "MausUndTastaturKontrolle";
    public const string WiiControl = "WiimoteKontrolle";
    private string selectedControl = MTControl;
    public bool plateAutopilot = true; //true falls der Sprachassistent steuert
    private bool carAutopilot = false;
    private bool trainingMode = false; //für TensorFlowTraining muss das Auto sich selbstständig bewegen
    //public bool trainingRouteNeedsUpdate = false;

    //public Dictionary<int, Vector3> trainingsFahrroute = new Dictionary<int, Vector3>();

    private bool playerControl = false;

    //Vom Sprachassistenten simulierte Movement Achsenbelegung
    private float assistantXAchse = 0;
    private float assistantZAchse = 0;
    private float assistantBrake = 0;

    //...simulierte Tellerbewegungen/neigungen
    public float assistantPlateXAchse = 0;
    public float assistantPlateZAchse = 0.5f;
    //Maximaler Neigungswinkel des Tellers:
    public float plateMaxAngle = 85f;

    //Belohnungen für PlateAgents, (Achtung: werden von Editor Einstellungen beim Player Auto überschrieben):
    public float incentiveLostLife = -5f;
    public float incentiveFinishedRoute = 5f;
    public float incentiveBallStillOnPlate = 0.01f;
    public float incentiveFactorDistanceBallToPlateCenter = 0.01f;
    public int delayFactor = 50;


    //Hilfsmethode um das Spiel mit Verzögerung zu schließen
    public static IEnumerator DelayedQuit(float secondsToWait)
    {
        yield return new WaitForSeconds(secondsToWait);
        Application.Quit();
    }


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
    public bool CarResetNeeded { get; set; }

    // Hat der Teller den Boden berührt -> ein Leben verlieren aber ball nicht reseten
    public bool BallResetNeeded { get; set; }

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
            return assistantZAchse;
        }

        set
        {
            if (value > 1f)
            {
                assistantZAchse = 1;
            }
            else if (value < -1f)
            {
                assistantZAchse = -1;
            }
            else
            {
                assistantZAchse = value;
            }
        }
    }

    public bool TrainingMode
    {
        get
        {
            return trainingMode;
        }

        set
        {
            if (CarAutopilot)
            {
                throw new System.Exception("Versuch den Trainingsmodus einzuschalten, während Autopilot aktiv ist");
            }
            else
            {
                trainingMode = value;
            }
        }
    }

    public bool CarAutopilot
    {
        get
        {
            return carAutopilot;
        }

        set
        {
            if (TrainingMode)
            {
                throw new System.Exception("Versuch den Autopiloten einzuschalten, während Trainingsmodus aktiv ist");
            }
            else
            {
                carAutopilot = value;
            }
        }
    }

    public float AssistantBrake
    {
        get
        {
            return assistantBrake;
        }

        set
        {
            assistantBrake = value;
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
