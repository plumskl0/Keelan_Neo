using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChangeKeyWorker : ScriptableObject
{

    // This method will be called when the thread is started.
    ManageMenu menuManager;
    public GameObject levelManager;
    private SharedFields sharedData = SharedFields.Instance;

    public void DoWork(object o)
    {
        KeySettingInfomation info = (KeySettingInfomation)o;
        DateTime startzeit = DateTime.Now;
        DateTime endzeit = DateTime.Now;
        TimeSpan ts = endzeit - startzeit;
        Debug.Log("Warte auf Eingabe");
        while (info.LastKeyStrokeCount == ManageMenu.KeyStrokeCount && ts.Seconds < 5)
        {
            //Debug.Log("Warte auf Eingabe");
            endzeit = DateTime.Now;
            ts = endzeit - startzeit;
        }
        if(info.LastKeyStrokeCount != ManageMenu.KeyStrokeCount) //gab eine Eingabe vor Timeout
        {
            sharedData.TmpMouseControls[info.DirectionCode] = ManageMenu.TmpKey;


            Debug.Log("Key Set to : " + ManageMenu.TmpKey);
        }



    }
    public void RequestStop()
    {
        _shouldStop = true;
    }
    // Volatile is used as hint to the compiler that this data
    // member will be accessed by multiple threads.
    private volatile bool _shouldStop;

}
