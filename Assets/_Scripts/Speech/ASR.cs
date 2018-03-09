using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using UnityEngine.Events;

public class ASR : MonoBehaviour, IAutomaticSpeechInterface {
    //Debug Ausgabefelder 
    /*public Text debugText;
    public Text wakeWordText;
    public Text sttText;
    public Text WakeWordStateText;
    public Text DictationStateText;*/
    private GameObject MicrophoneBorder;
    private Image background;

    private string lastCommand;


    private GameObject SpeechAssistant;
    private ISpeechToTextInterface STT;
    private IWakeWordEngineInterface WWE;
    private SpeechSystemStatus WakeWordState;
    private SpeechSystemStatus DictationState;
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
        STT = SpeechAssistant.AddComponent<SpeechToText>();
        //STT = GetComponent<SpeechToText>();
        WWE = SpeechAssistant.AddComponent<WakeWordEngine>();
        //WWE = GetComponent<WakeWordEngine>();
        Debug.Log("Stt hinzugefügt");
        MicrophoneBorder = GameObject.Find("MicrophoneBorder");

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
        //WWE.keywordRecognizer.Start();
        WWE.InitDetection();

        background = MicrophoneBorder.GetComponent<Image>();
        StartCoroutine("FlashMicrophoneOverlay");
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
        //falls ein Aufruf zum Wechsel zu STT kommt bevor diese gestoppt wurde läuft die WWE noch nicht
        if (WantToChangeToWakeWordDetection)
        {
            StartCoroutine("WaitForDictationStop");
        }
        else
        {
            WWE.StopDetection();
            STT.StartDetection();
        }
    }

    IEnumerator WaitForDictationStop ()
    {
        while (WantToChangeToWakeWordDetection)
        {
            Debug.Log("Warte auf Stop der Dictation");
            yield return null;
        }
        Debug.Log("Dictation gestoppt - ich wechsle jetzt zu TTS");
        WWE.StopDetection();
        STT.StartDetection();
    }


    private void StopDictation()
    {
        STT.StopDetection();
    }

    /*
    public void SwitchToWakeWordDetection(String textHeard, Text textFieldToDisplay)
    {
        Debug.Log("VORHER Status Wakeword: " + PhraseRecognitionSystem.Status);
        Debug.Log("VORHER Status Dication: " + STT.dictationRecognizer.Status);
        textFieldToDisplay.text = textHeard;
        //wakeWordText.text = "";
        STT.StopDetection();
        WantToChangeToWakeWordDetection = true;

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
        STT.StartDetection();
        Debug.Log("Status Wakeword: " + WakeWordState);
        Debug.Log("Status Dication: " + DictationState);
    }
    */

    private DateTime waitXSeconds(double secondsToWait)
    {
        DateTime startTime = DateTime.Now;
        DateTime endTime = startTime.AddSeconds(secondsToWait);
        return endTime;
    }

    IEnumerator FlashMicrophoneOverlay()
    {
        
        int schalter = 0;
        //DateTime wechselzeit = waitXSeconds(1);
        for (; ; )
        {
            if (DictationState.Equals(SpeechSystemStatus.Running))
            {
                switch (schalter)
                {
                    case 0:
                        //Debug.Log("Wechsel zu rot");
                        background.color = Color.red;
                        schalter = 1;
                        break;
                    case 1:
                        //Debug.Log("Wechsel zu weis");
                        background.color = Color.white;
                        schalter = 0;
                        break;
                }
            }
            else if (WakeWordState.Equals(SpeechSystemStatus.Running))
            {

                //Debug.Log(string.Format("Endzeit: {0} {1} ist früher als aktuelle Zeit {2} {3}", wechselzeit.Second, wechselzeit.Millisecond,DateTime.Now.Second, DateTime.Now.Millisecond));
                background.color = Color.blue;
                schalter = 0;
            }

            else
            {
                Debug.Log(string.Format("*****ERROR: WWE Status: {0} ____ TTS Status:{1}", WakeWordState, DictationState));
            }
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    private void FixedUpdate()
    {
       /* if (DictationState.Equals(SpeechSystemStatus.Running))
        {
            StartCoroutine("FlashMicrophoneOverlay");
        }
        */
    }


    // Update is called once per frame
    void Update () {
        WakeWordState = WWE.GetState();
        DictationState = STT.GetState();
        //lasse das Mikrofon Overlay blinken, falls TTS aktiv ist
        /*if (DictationState.Equals(SpeechSystemStatus.Running))   {
            StartCoroutine("FlashMicrophoneOverlay");
        }*/
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
