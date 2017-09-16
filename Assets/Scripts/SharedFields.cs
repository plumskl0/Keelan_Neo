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
    
    private SharedFields()
    {
        LostLife = false;
        CarReset = false;
        PayedCoin = false;
        CoinCount = 0;
    }

    // Hat der Ball den Boden berührt -> ein leben verlieren
    public bool LostLife { get; set; }

    // Hat der Ball den Boden berührt -> Fahrzeug reset
    public bool CarReset { get; set; }

    // Wurde für ein Special mit einer Münze bezahlt
    public bool PayedCoin { get; set; }

    // Anzahl der gesammelten Münzen
    public int CoinCount { get; set; }

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
