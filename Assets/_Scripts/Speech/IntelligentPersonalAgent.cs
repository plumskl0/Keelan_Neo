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
    private IPAAction actions;
    public Text debugText;

    //Unity Actions for Event Manager
    private UnityAction<EventMessageObject> CallNLU;
    private UnityAction<EventMessageObject> Print;
    private UnityAction<EventMessageObject> IntentHandler;


    private void Awake()
    {
        asr = gameObject.AddComponent<ASR>();
        nlu = gameObject.AddComponent<NLU>();
        actions = gameObject.AddComponent<IPAAction>();

        CallNLU = new UnityAction<EventMessageObject>(SendTextToNLU);
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

        //Dialog Delegation prüfen
        Debug.Log("Überprüfe ob noch Slots fehlen: ");
        //AIOutputContext context = nluResponse.Result.GetContext();
        AIOutputContext[] context = nluResponse.Result.Contexts;
        bool slotsMissing = false;
        foreach (AIOutputContext con in context)
        {
            Debug.Log(con.Name);
            if(con.Name.Contains("dialog_context"))
            {
                slotsMissing = true;
            }
        }
        if (slotsMissing)
        {
            Debug.Log("Es fehlen noch Slotbelegungen. Ich gebe die Kontrolle an TTS");
            Debug.Log("WWE Status " + asr.WakeWordState);
            Debug.Log("STT Status " + asr.DictationState);
            EventManager.TriggerEvent(EventManager.keywordDetectedEvent, new EventMessageObject(EventManager.keywordDetectedEvent, "Slots fehlen"));
            actions.DisplayText(debugText, nluResponse.Result.Fulfillment.Speech);
        }

        //ansonsten rufe die Handler auf
        else
        {
            Debug.Log("Alle Slots gefüllt:");
            Dictionary<String, System.Object> dic = nluResponse.Result.Parameters;
            foreach (String key in dic.Keys)
            {
                Debug.Log(string.Format("Parameter: {0}", key));
            }
            actions.DisplayText(debugText, nluResponse.Result.Fulfillment.Speech);

            String intent = nluResponse.Result.Metadata.IntentName;
            String action = nluResponse.Result.Action;
            switch (action) {

                //Open Map Intent
                case IPAAction.openMap:
                    actions.OpenMap();

                    break;

                case IPAAction.closeMap:
                    actions.CloseMap();
                    actions.SetMinimapFokusOnCar();
                    break;

                case IPAAction.changeMapFixedStep:
                    String groesseneinheit = nluResponse.Result.GetStringParameter("Groesseneinheit");
                    String direction = nluResponse.Result.GetStringParameter("Direction");
                    if (direction.Length==0)
                    {
                        direction = nluResponse.Result.GetStringParameter("MoveDirection");
                    }

                    actions.ChangMapFixedStep(groesseneinheit, direction);
                    break;
                case IPAAction.focusOnCar:
                    actions.SetMinimapFokusOnCar();
                    break;

                case IPAAction.moveCar:
                    groesseneinheit = nluResponse.Result.GetStringParameter("Groesseneinheit");
                    direction = nluResponse.Result.GetStringParameter("MoveDirection");
                    actions.MoveCar(groesseneinheit, direction);
                    break;

                case IPAAction.wantToSetContext:
                    int firstFreeContext = context.Length;
                    AIOutputContext[] newContext = new AIOutputContext[firstFreeContext + 1];
                    AIOutputContext contextToInsert = new AIOutputContext();
                    contextToInsert.Name = "TestContext";
                    contextToInsert.Lifespan = 3;
                    newContext[firstFreeContext] = contextToInsert;
                    context = newContext;
                    Debug.Log("So sollte es aussehen:");
                    foreach (AIOutputContext con in context)
                    {
                        Debug.Log(con.Name);
                        if (con.Name.Contains("dialog_context"))
                        {
                            slotsMissing = true;
                        }
                    }
                    Debug.Log("Trigger SpeechCommandRecognized to send new Context to Dialogflow");
                    EventManager.TriggerEvent(EventManager.asrRequerstDetectedEvent, new EventMessageObject(EventManager.asrRequerstDetectedEvent, string.Format("Rueckantwort Kontext einfuegen")));
                    break;

                case IPAAction.setContext:
                    Debug.Log("Habe Kontext gesetzt.");
                    foreach (AIOutputContext con in context)
                    {
                        Debug.Log(con.Name);
                        if (con.Name.Contains("dialog_context"))
                        {
                            slotsMissing = true;
                        }
                    }
                    break;

                default:
                    Debug.Log(string.Format("Der Intent {0} wurde im IntentHandler nicht registiert.", intent));
                    break;
            }
        }

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

    public void SendTextToNLU(EventMessageObject asrRequestMessageObject)
    {
        string asrRequest = (string) asrRequestMessageObject.MessageBody;
        PrintMessageBody(asrRequestMessageObject);
        nlu.UnderstandRequest(asrRequest);
    }
}
