using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedFields : MonoBehaviour {

    private bool playerControl = false;


    public bool GetPlayerControl()
    {
        return playerControl;
    }

    public void SetPlayerControl(bool b)
    {
        playerControl = b;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
