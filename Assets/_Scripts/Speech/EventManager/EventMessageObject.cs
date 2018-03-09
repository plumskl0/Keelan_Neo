using ApiAiSDK.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventMessageObject {
    private string type;    //valid Types for Events are in EventManager.cs
    private string stringRequest;
    private AIResponse nluAnswer;
    private object messageBody;


    public string Type
    {
        get
        {
            return type;
        }

        set
        {
            //TODO: Check if valid type
            type = value;
        }
    }

    public object MessageBody
    {
        get
        {
            if (type == EventManager.keywordDetectedEvent)
            {
                return stringRequest;
            }
            else if (type == EventManager.asrRequerstDetectedEvent)
            {
                return stringRequest;
            }
            else if (type == EventManager.nluAnswerDetectedEvent)
            {
                return nluAnswer;
            }
            else
            {
                return "Type der Message entspricht keinem der erlaubten Werte.";
            }
        }

        set
        {
            //TODO: Check if Structure is valid - considering the type chosen
            if (type == EventManager.keywordDetectedEvent)
            {
                if (value.GetType().Equals(typeof(string)))
                {
                    stringRequest = (string)value;
                }
                else
                {
                    Debug.LogError("***Fehler: Type der Message war keywordDetectedRequest, aber MessageBody kein string.");
                }
            }

            else if (type == EventManager.asrRequerstDetectedEvent)
            {
                if (value.GetType().Equals(typeof(string))) {
                    stringRequest = (string)value;
                }
                else
                {
                    Debug.LogError("***Fehler: Type der Message war asrRequest, aber MessageBody kein string.");
                }
            }
            else if (type == EventManager.nluAnswerDetectedEvent)
            {
                if ((value.GetType().Equals(typeof(AIResponse)))) { 
                    nluAnswer = (AIResponse)value;
                }

                else
                {
                    Debug.LogError("***Fehler: Type der Message war NLUResponse, aber MessageBody kein AiResponseObject.");
                }
            }

            else
            {
                //MessageBody = value;
                Debug.LogError("***Fehler: konnte MessageBody nicht setzen, da Type keinem der erlaubten Werte entspricht!");
            }
            
        }
    }

    public EventMessageObject (string _type, string _messageBody)
    {
        Type = _type;
        MessageBody = _messageBody;
    }

    public EventMessageObject(string _type, AIResponse _messageBody)
    {
        Type = _type;
        nluAnswer = _messageBody;
    }


}
