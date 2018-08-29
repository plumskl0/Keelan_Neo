using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetNamePerKeyboard : MonoBehaviour {

    SharedFields sharedData = SharedFields.Instance;

    public void SetName(string playerName)
    {
        sharedData.playerName = playerName;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
