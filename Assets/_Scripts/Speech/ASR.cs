using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using UnityEngine.Events;

public class ASR : MonoBehaviour {
    //Debug Ausgabefelder 
    /*public Text debugText;
    public Text wakeWordText;
    public Text sttText;
    public Text WakeWordStateText;
    public Text DictationStateText;*/

    private string lastCommand;


    private GameObject SpeechAssistant;
    private SpeechToText STT;
    private WakeWordEngine WWE;
    SpeechSystemStatus WakeWordState;
    SpeechSystemStatus DictationState;
    private Boolean WantToChangeToWakeWordDetection = false;

    //Unity Actions for EventManager
    private UnityAction<EventMessageObject> EnableWakeWord;
    private UnityAction<EventMessageObject> EnableSpeechToText;

    public string LastCommand
    {
        get
        {
            return lastCommand;
        }

        set
        {
            lastCommand = value;
        }
    }

    //private List<String> WakeWords = new List<String>();

    private void Awake()
    {
        SpeechAssistant = gameObject;
        SpeechAssistant.AddComponent<SpeechToText>();
        STT = GetComponent<SpeechToText>();
        SpeechAssistant.AddComponent<WakeWordEngine>();
        WWE = GetComponent<WakeWordEngine>();
        Debug.Log("Stt hinzugefügt");

        EnableWakeWord = new UnityAction<EventMessageObject>(SwitchToWakeWordDetection);
        EnableSpeechToText = new UnityAction<EventMessageObject>(SwitchToSpeechToText);


        //Registriere Events:
        //EventManager.StartListening()
    }

    // Use this for initialization
    void Start() {
        /* SpeechAssistant = gameObject;
         SpeechAssistant.AddComponent<SpeechToText>();
         STT = GetComponent<SpeechToText>();
         SpeechAssistant.AddComponent<WakeWordEngine>();
         WWE = GetComponent<WakeWordEngine>();
         Debug.Log("Stt hinzugefügt");
         */

        //AddWakeWords(new String[] { "computer", "auto" });
        Debug.Log(PhraseRecognitionSystem.Status);
        WWE.keywordRecognizer.Start();
        //SwitchToWakeWordDetection();
        //STT = GameObject.Find(STT);
    }

    private void OnEnable()
    {
        EventManager.StartListening(EventManager.keywordDetectedEvent, EnableSpeechToText);
        EventManager.StartListening(EventManager.asrRequerstDetectedEvent, EnableWakeWord);
    }

    public void AddWakeWords(String[] wordsToAdd)
    {
        WWE.AddWakeWords(wordsToAdd);
    }

    public void SwitchToWakeWordDetection(EventMessageObject args)
    {
        STT.StopDetection();
        WantToChangeToWakeWordDetection = true;
    }

    public void SwitchToSpeechToText(EventMessageObject args)
    {
        WWE.StopDetection();
        STT.StartDetection();
    }


    public void StopDictation()
    {
        STT.StopDetection();
    }


    public void SwitchToWakeWordDetection(String textHeard, Text textFieldToDisplay)
    {
        Debug.Log("VORHER Status Wakeword: " + PhraseRecognitionSystem.Status);
        Debug.Log("VORHER Status Dication: " + STT.dictationRecognizer.Status);
        textFieldToDisplay.text = textHeard;
        //wakeWordText.text = "";
        STT.StopDetection();
        WantToChangeToWakeWordDetection = true;
        /*DateTime timeout = waitXSeconds(5);
        while (STT.dictationRecognizer.Status.Equals(SpeechSystemStatus.Running))
        {
            Debug.Log("warte bis DictationMode geschlossen ist");
            if (timeout > DateTime.Now)
            {
                Debug.Log("Konnte Dictation nicht stoppen");
                break;
            }
        };*/

        //WWE.StartDetection();
        /*timeout = waitXSeconds(5);
        while (PhraseRecognitionSystem.Status.Equals(SpeechSystemStatus.Stopped))
        {
            Debug.Log("warte bis WakeWordEnginge startet");
            if (timeout > DateTime.Now)
            {
                Debug.Log("Konnte WakeWordEngine nicht starten");
                break;
            }
        };*/

        Debug.Log("Status Wakeword: " + PhraseRecognitionSystem.Status);
        Debug.Log("Status Dication: " + STT.dictationRecognizer.Status);
    }

    public void SwitchToSpeechToText(String textHeard, Text textFieldToDisplay)
    {
        textFieldToDisplay.text = textHeard;
        //sttText.text = "";
        Debug.Log("VORHER Status Wakeword: " + WakeWordState);
        Debug.Log("VORHER Status Dication: " + DictationState);
        WWE.StopDetection();
        /*
        DateTime timeout = waitXSeconds(5);
        while (PhraseRecognitionSystem.Status.Equals(SpeechSystemStatus.Running))
        {
            Debug.Log("warte bis WakeWordEnginge geschlossen ist");
            if (timeout > DateTime.Now)
            {
                Debug.Log("Konnte WakeWordEngine nicht stoppen");
                break;
            }
        };*/
        STT.StartDetection();
        /*timeout = waitXSeconds(5);
        while (DictationState.Equals(SpeechSystemStatus.Stopped))
        {
            Debug.Log("warte bis DictationMode startet");
            if (timeout.CompareTo(DateTime.Now) < 0)    //-> Deadline ueberschritten
            {
                Debug.Log("Konnte Dictation nicht starten");
                break;
            }
        };*/
        Debug.Log("Status Wakeword: " + WakeWordState);
        Debug.Log("Status Dication: " + DictationState);
    }


    private DateTime waitXSeconds(double secondsToWait)
    {
        DateTime startTime = DateTime.Now;
        DateTime endTime = startTime.AddSeconds(secondsToWait);
        return endTime;
    }

    // Update is called once per frame
    void Update () {
        WakeWordState = PhraseRecognitionSystem.Status;
        DictationState = STT.dictationRecognizer.Status;

        //WakeWordStateText.text = WakeWordState.ToString();
        //DictationStateText.text = DictationState.ToString();


        if(WantToChangeToWakeWordDetection)
        {
            Debug.Log("*****Sie möchten zur WakeWordEngine wechseln...");
            if (DictationState.Equals(SpeechSystemStatus.Running))
            {
                Debug.Log("....aber der DictationMode lief noch.****");
                Debug.Log("DictationState: " + DictationState);
            }
            else if (DictationState.Equals(SpeechSystemStatus.Stopped))
            {
                Debug.Log("....und los gehts.*******");
                WWE.StartDetection();
                WantToChangeToWakeWordDetection = false;
            }
            else
            {
                Debug.Log("_______________ERROR!!!__________");
            }
                    
            
        }
        /*if(PhraseRecognitionSystem.Status.Equals(SpeechSystemStatus.Stopped) && STT.dictationRecognizer.Status.Equals(SpeechSystemStatus.Stopped))
        {
            Debug.Log("------ Keine Spracherkennung aktiv. Starte WakeWordEngine");
            SwitchToWakeWordDetection("fehler erkannt");
        }*/

    }
}
