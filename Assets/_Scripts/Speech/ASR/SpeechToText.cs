using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class SpeechToText : MonoBehaviour, ISpeechToTextInterface {

    private DictationRecognizer dictationRecognizer;

//    private DictationRecognizer dictationRecognizer;
    private String _userCommand;
    private ASR asr;
    private Text debugText;

    public string UserCommand
    {
        get
        {
            return _userCommand;
        }

        set
        {
            _userCommand = value;
        }
    }

    // Use this for initialization
    void Awake () {
        debugText = GameObject.Find("DebugText").GetComponent<Text>();
        asr = GetComponent<ASR>();
        dictationRecognizer = new DictationRecognizer();
        dictationRecognizer.InitialSilenceTimeoutSeconds = 20f;
        dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
        dictationRecognizer.DictationError += DictationRecognizer_DictationError;
        dictationRecognizer.DictationComplete += (completionCause) =>
        {
            if(completionCause.Equals(DictationCompletionCause.Complete))
            {
                Debug.LogErrorFormat("STT erfolgreich fertig wg: {0}... brauche ich hier noch ein Event das zurückwechselt? Soll bereits bei DictationResult passieren.",completionCause.ToString());
                EventManager.TriggerEvent(EventManager.ttsUnhandledError, new EventMessageObject(EventManager.ttsUnhandledError, completionCause.ToString()));  //todo: falls benötigt noch neues Event erstellen
            }

            else if(completionCause.Equals(DictationCompletionCause.TimeoutExceeded) || completionCause.Equals(DictationCompletionCause.PauseLimitExceeded))
            {
                Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);
                EventManager.TriggerEvent(EventManager.ttsTimeout, new EventMessageObject(EventManager.ttsTimeout, completionCause.ToString()));
            }
            else if(completionCause.Equals(DictationCompletionCause.AudioQualityFailure) || completionCause.Equals(DictationCompletionCause.MicrophoneUnavailable) || completionCause.Equals(DictationCompletionCause.NetworkFailure) || completionCause.Equals(DictationCompletionCause.UnknownError))
            {
                Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);
                EventManager.TriggerEvent(EventManager.ttsError, new EventMessageObject(EventManager.ttsError, completionCause.ToString()));
            }
            else
            {
                Debug.LogErrorFormat("Dictation fertig mit unbehandeltem Zustand: {0}", completionCause);
                EventManager.TriggerEvent(EventManager.ttsUnhandledError, new EventMessageObject(EventManager.ttsUnhandledError, completionCause.ToString()));
            }

            
        };
         dictationRecognizer.DictationHypothesis += (text) =>
         {
             Debug.LogFormat("Dictation hypothesis: {0}", text);
             if(text!="")
                debugText.text = text;
             else
             {
                 Debug.LogError("STT Eingabe erkannt - wurde aber nicht verstanden.");
             }
         };
    }

    private void OnEnable()
    {
        
    }

    private void DictationRecognizer_DictationError(string error, int hresult)
    {
        Debug.LogError("***ERROR: " + error);
    }

    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        //asr.Print(text);
        asr.LastCommand = text;
        Debug.Log("Trigger asrRequestDetected Event");
        EventManager.TriggerEvent(EventManager.asrRequerstDetectedEvent, new EventMessageObject(EventManager.asrRequerstDetectedEvent, text));
        //asr.SwitchToWakeWordDetection();
        //asr.SwitchToWakeWordDetection(text, asr.debugText);
        //_userCommand = text;
        //resultText.text = _userCommand;
        //Debug.Log(_userCommand);
    }


    public void StartDetection()
    {
        Debug.Log("Starte Dictation Mode");
        //Debug.Log(dictationRecognizer.ToString());
        dictationRecognizer.Start();
        //Debug.Log("Dictation started");
    }

    public void StopDetection()
    {
        Debug.Log("Stoppe Dictation Mode");
        dictationRecognizer.Stop();
        /*if (dictationRecognizer.Status.Equals(SpeechSystemStatus.Running))
        {
            Debug.Log("Stoppe Dictation Mode");
            dictationRecognizer.Stop();
            //dictationRecognizer.Dispose();
        }
        */
    }

    // Update is called once per frame
    void Update () {
		
	}

    public SpeechSystemStatus GetState()
    {
        return dictationRecognizer.Status;
    }
}
