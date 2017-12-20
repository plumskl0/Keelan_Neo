using ApiAiSDK.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class IntelligentPersonalAgent : MonoBehaviour {
    private ASR asr;
    private NLU nlu;
    public Text debugText;

    //Unity Actions for Event Manager
    private UnityAction<EventMessageObject> CallNLU;
    private UnityAction<EventMessageObject> Print;
    private UnityAction<EventMessageObject> IntentHandler;


    private void Awake()
    {
        asr = gameObject.AddComponent<ASR>();
        nlu = gameObject.AddComponent<NLU>();
        CallNLU = new UnityAction<EventMessageObject>(nlu.UnderstandRequest);
        Print = new UnityAction<EventMessageObject>(PrintMessageBody);
        IntentHandler = new UnityAction<EventMessageObject>(HandleIntent);
    }

    private void OnEnable()
    {
        EventManager.StartListening(EventManager.keywordDetectedEvent, Print);
        EventManager.StartListening(EventManager.asrRequerstDetectedEvent, CallNLU);
        EventManager.StartListening(EventManager.nluAnswerDetectedEvent, HandleIntent);
        //EventManager.StartListening("SpeechCommandRegocnized", nlu.UnderstandRequest());
    }

    public void HandleIntent (EventMessageObject nluAnswer)
    {
        AIResponse nluResponse = (AIResponse) nluAnswer.MessageBody;
        Debug.Log("##### Habe folgenden Intent erkannt und möchte ihn jetzt verarbeiten: " + nluResponse.Result.Metadata.IntentName);

    }



    //Gibt ein Präfix gefolgt vom Inhalt der EventMessage im debugTextfeld aus
    private void PrintMessageBody(EventMessageObject _eventMessageObject)
    {
        string messageBody;
        try
        {
            messageBody = (string)_eventMessageObject.MessageBody;
        }
        catch (Exception e)
        {
            Debug.LogError("Falscher Typ des Message Bodys Objekts" +  e.Message);
            messageBody = "ERROR";
        }
        string messageType = _eventMessageObject.Type;



        if (messageType.Equals(EventManager.keywordDetectedEvent))
        {
            debugText.text = "Keyword detected: " +  messageBody;
        }

        else if (messageType.Equals(EventManager.asrRequerstDetectedEvent))
        {
            debugText.text = "ASR Request detected: " + messageBody;
        }

        else if (messageType.Equals(EventManager.nluAnswerDetectedEvent))
        {
            debugText.text = "NLU answer detected: " + messageBody ;
        }


    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SendTextToNLU()
    {

    }
}
