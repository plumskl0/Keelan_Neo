using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour {
    private Dictionary<string, UnityEventWithParameter> eventDictonary;
    private static EventManager eventManager;

    //Available Events:
    public const string keywordDetectedEvent = "keyword Detected";
    public const string asrRequerstDetectedEvent = "Speech Command Regocnized";
    public const string nluAnswerDetectedEvent = "NLU answer recignized";


    public static EventManager instance
    {
        get
        {
            if(!eventManager)
            {
                eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;
                if(!eventManager)
                {
                    Debug.LogError("Ein GameObjekt muss das EventManager Skript besitzen. ");
                }
                else
                {
                    eventManager.Init();
                }
            }
            return eventManager;
        }
    }

    void Init()
    {
        if(eventDictonary == null)
        {
            eventDictonary = new Dictionary<string, UnityEventWithParameter>();
        }
    }

    public static void StartListening (string eventName, UnityAction<EventMessageObject> listener)
    {
        UnityEventWithParameter thisEvent = null;
        if(instance.eventDictonary.TryGetValue (eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEventWithParameter();
            thisEvent.AddListener(listener);
            instance.eventDictonary.Add(eventName,thisEvent);
        }
    }

    public static void StopListening (string eventName, UnityAction<EventMessageObject> listener)
    {
        if (eventManager == null) return;
        UnityEventWithParameter thisEvent = null;
        if (instance.eventDictonary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

   /* public static void TriggerEvent (string eventName, Object args )
    {
        UnityEventWithParameter thisEvent = null;
        if (instance.eventDictonary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke();
        }
    }*/ 

    public static void TriggerEvent (string eventName, EventMessageObject args)
    {
        UnityEventWithParameter thisEvent = null;
        if (instance.eventDictonary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(args);
        }
    }
 

}

public class UnityEventWithParameter : UnityEvent<EventMessageObject>
{

}

/*public class UnityEventWithParameter2 : UnityEvent<EventMessageObject>
{

}*/