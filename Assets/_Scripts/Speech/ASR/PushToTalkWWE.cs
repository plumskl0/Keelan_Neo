using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class PushToTalkWWE : MonoBehaviour, IWakeWordEngineInterface  {
    bool isListening = false;

    public SpeechSystemStatus GetState()
    {
        if(isListening)
        {
            return SpeechSystemStatus.Running;
        }
        else
        {
            return SpeechSystemStatus.Stopped;
        }
    }

    public void InitDetection()
    {
        isListening = true;
    }

    public void StartDetection()
    {
        isListening = true;

    }

    public void StopDetection()
    {
        isListening = false;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(isListening)
        {
            if(Input.GetKeyDown(KeyCode.Mouse2) || (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.LeftShift)))
            {
                EventManager.TriggerEvent(EventManager.keywordDetectedEvent, new EventMessageObject(EventManager.keywordDetectedEvent, "Triggered by PushToTalk"));
            }
        }
	}
}
