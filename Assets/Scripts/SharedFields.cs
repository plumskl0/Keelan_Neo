using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
