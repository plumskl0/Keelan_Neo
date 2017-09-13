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

    private SharedFields() {}

    public const string MTControl = "MausUndTastaturKontrolle";
    public const string WiiControl = "WiimoteKontrolle";

    private string selectedControl = MTControl;
    private bool playerControl = false;

    // Mausempfindlichkeit
    public float sensitivity = 1.25f;

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
