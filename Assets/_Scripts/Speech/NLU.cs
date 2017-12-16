using ApiAiSDK.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NLU : MonoBehaviour {

    private DialogflowConnection dialogflowConnection;

    private void Awake()
    {
        dialogflowConnection = gameObject.AddComponent<DialogflowConnection>();

    }

    public AIResponse UnderstandRequest (object query)
    {
        EventMessageObject queryMessage = (EventMessageObject) query;
        return dialogflowConnection.SendVoiceText(queryMessage.MessageBody);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
