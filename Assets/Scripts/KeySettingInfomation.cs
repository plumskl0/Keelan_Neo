using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Dient als Speicher, welche Taste geändert werden soll + Übergabe des aktuellen Keystrokecounts
public class KeySettingInfomation : ScriptableObject {

    public KeySettingInfomation(int dirCode, int lKeyStroke)
    {
        DirectionCode = dirCode;
        LastKeyStrokeCount = lKeyStroke;
    }

    private int directionCode;
    private int lastKeyStrokeCount;

    public int DirectionCode
    {
        get
        {
            return directionCode;
        }

        set
        {
            directionCode = value;
        }
    }

    public int LastKeyStrokeCount
    {
        get
        {
            return lastKeyStrokeCount;
        }

        set
        {
            lastKeyStrokeCount = value;
        }
    }
}
