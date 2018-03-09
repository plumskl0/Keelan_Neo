using ApiAiSDK.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NLU : MonoBehaviour, INaturalLanguageUnderstandingInterface {

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
}
