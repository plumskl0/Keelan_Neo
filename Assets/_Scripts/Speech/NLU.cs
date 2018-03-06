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

    public void UnderstandRequest (string query)
    {
        AIResponse dialogflowAnswer = dialogflowConnection.SendVoiceText(query);
        Debug.Log("Trigger NLUAnswerDetected Event");
        EventManager.TriggerEvent(EventManager.nluAnswerDetectedEvent, new EventMessageObject(EventManager.nluAnswerDetectedEvent, dialogflowAnswer));
    }

   /* private AsrRequest marshalEventMessage(EventMessageObject eventMessage)
    {

    }

    public struct AsrRequest
    {
        public string queryMessageObject;
        string asr
    }*/

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
