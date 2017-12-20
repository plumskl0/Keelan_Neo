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

    public void UnderstandRequest (object query)
    {
        EventMessageObject queryMessage = (EventMessageObject) query;
        string asrRequest = (string) queryMessage.MessageBody;
        AIResponse dialogflowAnswer = dialogflowConnection.SendVoiceText(asrRequest);
        EventManager.TriggerEvent(EventManager.nluAnswerDetectedEvent, new EventMessageObject(EventManager.nluAnswerDetectedEvent, dialogflowAnswer));
        Debug.Log("Trigger NLUAnswerDetected Event");
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
