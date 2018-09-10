using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using UnityEngine.Events;

public class ASR : MonoBehaviour, IAutomaticSpeechInterface
{
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

    private bool isSwitchingASRMode = false;
    public delegate void AsrSwitchDelegate();
    AsrSwitchDelegate SwitchToWWE;
    AsrSwitchDelegate SwitchToSTT;


    Queue<AsrSwitchDelegate> ASRModeSwitchQueue = new Queue<AsrSwitchDelegate>();




    //Delegate enableWWE = SwitchToWakeWordDetection;

    //Unity Actions for EventManager
    private UnityAction<EventMessageObject> EnableWakeWord;
    private UnityAction<EventMessageObject> EnableWakeWordAfterError;
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
        //STT = SpeechAssistant.AddComponent<STTGoogleCloudSpeech>();


        WWE = SpeechAssistant.AddComponent<WakeWordEngine>();
        //SpeechAssistant.AddComponent<GoogleVoiceSpeech>();

        Debug.Log("Stt hinzugefügt");
        MicrophoneBorder = GameObject.Find("MicrophoneBorder");

        EnableWakeWord = new UnityAction<EventMessageObject>(AddWWERequestToQueue);
        EnableWakeWordAfterError = new UnityAction<EventMessageObject>(SSTErrorHandling);
        EnableSpeechToText = new UnityAction<EventMessageObject>(AddSTTRequestToQueue);


        //Registriere Events:
        //EventManager.StartListening()
    }

    public void AddSTTRequestToQueue(EventMessageObject args)
    {
        Debug.LogFormat("Wechsel zu STT angefordert. Message: {0}", args.MessageBody.ToString());
        ASRModeSwitchQueue.Enqueue(SwitchToSTT);
    }


    public void AddWWERequestToQueue(EventMessageObject args)
    {
        Debug.LogFormat("Wechsel zu WWE angefordert. Message: {0}", args.MessageBody.ToString());
        ASRModeSwitchQueue.Enqueue(SwitchToWWE);
    }

    private void SSTErrorHandling(EventMessageObject args)  //Prüfe bei Fehlern der SST den ASR Zustand und starte ggf. WakeWordEngine
    {
        //Prüfe #Kommandos in Schlange -> keine + kein aktiver Wechsel-> gehe zu WWE; sonst: Ausgabe aller Befehle + Handle je nachdem was drin steht
        if (ASRModeSwitchQueue.Count == 0 && WakeWordState!= SpeechSystemStatus.Running &&!isSwitchingASRMode)   //Zurückwechseln zur WWE notwendig, da sonst keine ASR Technik mehr aktiv ist
        {
            Debug.LogErrorFormat("Die Schlange war leer als der Fehler auftrag {0}. Füge deshalb Wechsel zu WWE hinzu ", args.MessageBody);
            ASRModeSwitchQueue.Enqueue(SwitchToWWE);
        }
        else if (ASRModeSwitchQueue.Count > 0)
        {
            Debug.LogError("Benötige weitere Fehlerbehandlung da STT Fehler auftrat als die Schalange wie folgt gefüllt war:");
            int i = 0;
            foreach (AsrSwitchDelegate command in ASRModeSwitchQueue)
            {
                Debug.LogErrorFormat("Befehl {0} befindet sich an Position {1}", command.Method.Name, i);
                i++;
            }
        }
        else
        {
            Debug.LogErrorFormat("Schlange war leer bei ASR Fehleranzeige. Aber keine Wechsel hinzugefügt da entweder STT oder Wechsel aktiv.");
        }

    }


    // Use this for initialization
    void Start()
    {
        SwitchToWWE = new AsrSwitchDelegate(SwitchToWakeWordDetection);
        SwitchToSTT = new AsrSwitchDelegate(SwitchToSpeechToText);

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
        EventManager.StartListening(EventManager.keywordDetectedEvent, EnableSpeechToText);  //ausschalten um WWE alleine zu testen
        EventManager.StartListening(EventManager.asrRequerstDetectedEvent, EnableWakeWord);
        EventManager.StartListening(EventManager.ttsError, EnableWakeWordAfterError); //todo: für alle drei Hinweis an Nutzer wo fehler liegt -> bisher wird nur zur WakeWordEngine gewechselt
        EventManager.StartListening(EventManager.ttsTimeout, EnableWakeWordAfterError);
        EventManager.StartListening(EventManager.ttsUnhandledError, EnableWakeWordAfterError);
    }

    public void AddWakeWords(String[] wordsToAdd)
    {
        WWE.AddWakeWords(wordsToAdd);
    }

    /*public void SwitchToWakeWordDetection(EventMessageObject args)
    {
        isSwitchingASRMode = true;
        STT.StopDetection();
        WantToChangeToWakeWordDetection = true;
        isSwitchingASRMode = false;
    }

    public void SwitchToSpeechToText(EventMessageObject args)
    {
        isSwitchingASRMode = true;
        //falls ein Aufruf zum Wechsel zu STT kommt bevor diese gestoppt wurde läuft die WWE noch nicht
        if (WantToChangeToWakeWordDetection)   //Event Trigger -> Programm ist gerade dabei von TTS zu WWE zu wechseln, während das Event sagt es möchte erneut zur TTS -> warte bis wechsel zu WWE abgeschlossen, beende WWE und starte TTS
        {
            StartCoroutine("WaitForDictationStop");
        }
        else
        {
            WWE.StopDetection();
            STT.StartDetection();
        }
        isSwitchingASRMode = false;
    }*/

    //Restart WakeWord for casses keyword does not work anymore
    public void RefreshWakeWordDetection() 
    {
        
        if (!isSwitchingASRMode)
        {
            isSwitchingASRMode = true;
            //SwitchToWakeWord in case Dictation is activ
            if (DictationState == SpeechSystemStatus.Running)
            {
                Debug.LogError("MANUELLER Wechsel von STT zu WWE.");
                SwitchToWakeWordDetection();
            }
            else if(WakeWordState == SpeechSystemStatus.Running)
            {
                WWE.StopDetection();
                WantToChangeToWakeWordDetection = true;
            }
            else
            {
                Debug.LogError("Fehler bei manuellem Refresh der WWE. Nicht behandelter Fall.");
            }
            isSwitchingASRMode = false;
        }
        else
        {
            Debug.LogError("Ignoriere manuellen Refresh der WWE, da gerade ein Wechsel stattfindet");
        }

    }

    //Test: Funktionen ohne Argumente -> werden tatsächlich nicht genutzt
    public void SwitchToWakeWordDetection()
    {
        isSwitchingASRMode = true;
        if (WakeWordState == SpeechSystemStatus.Running)
        {
            Debug.LogError("Zwei aufeinander folgende Befehle, um zur WWE zu wechseln. -> Ignoriere letzte");
        }
        else
        {
            STT.StopDetection();
            WantToChangeToWakeWordDetection = true;
        }
    }

    public void SwitchToSpeechToText()
    {
        isSwitchingASRMode = true;

        if (DictationState == SpeechSystemStatus.Running)
        {
            Debug.LogError("Zwei aufeinander folgende Befehle, um zur STT zu wechseln. -> Ignoriere letzte");
        }
        else
        {
            //falls ein Aufruf zum Wechsel zu STT kommt bevor diese gestoppt wurde läuft die WWE noch nicht
            if (WantToChangeToWakeWordDetection)   //Event Trigger -> Programm ist gerade dabei von TTS zu WWE zu wechseln, während das Event sagt es möchte erneut zur TTS -> warte bis wechsel zu WWE abgeschlossen, beende WWE und starte TTS
            {
                StartCoroutine("WaitForDictationStop");
            }
            else
            {
                WWE.StopDetection();
                STT.StartDetection();
            }
        }
        isSwitchingASRMode = false;
    }

    IEnumerator WaitForDictationStop()
    {
        while (WantToChangeToWakeWordDetection)
        {
            //Debug.Log("Warte auf Stop der Dictation");
            Debug.LogError("Versuchter Start der STT während gerade zur WWE gewechselt wird. Verzögere bis WWE gestartet");
            yield return null;
        }
        Debug.LogError("WWE gestartet - ich wechsle jetzt zu TTS");
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
            //Debug.Log("Coroutine aktiv");
            //Debug.LogError(string.Format("*****ERROR: WWE Status: {0} ____ TTS Status:{1}", WWE.GetState(), STT.GetState()));
            //Debug.LogError("WWE is listening: " + WWE.)
            //if (DictationState.Equals(SpeechSystemStatus.Running))
            if (STT.GetState().Equals(SpeechSystemStatus.Running) && !WWE.GetState().Equals(SpeechSystemStatus.Running))
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
            //else if (WakeWordState.Equals(SpeechSystemStatus.Running))
            else if (WWE.GetState().Equals(SpeechSystemStatus.Running) && !STT.GetState().Equals(SpeechSystemStatus.Running))
            {

                //Debug.Log(string.Format("Endzeit: {0} {1} ist früher als aktuelle Zeit {2} {3}", wechselzeit.Second, wechselzeit.Millisecond,DateTime.Now.Second, DateTime.Now.Millisecond));
                background.color = Color.blue;
                schalter = 0;
            }

            else
            {
                Debug.LogError(string.Format("*****ERROR: WWE Status: {0} ____ TTS Status:{1}", WakeWordState, DictationState));
                background.color = Color.black;
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
    void Update()
    {
        WakeWordState = WWE.GetState();
        DictationState = STT.GetState();
        //lasse das Mikrofon Overlay blinken, falls TTS aktiv ist
        /*if (DictationState.Equals(SpeechSystemStatus.Running))   {
            StartCoroutine("FlashMicrophoneOverlay");
        }*/
        //WakeWordStateText.text = WakeWordState.ToString();
        //DictationStateText.text = DictationState.ToString();


        if (WantToChangeToWakeWordDetection)
        {
            Debug.LogError("*****Sie möchten zur WakeWordEngine wechseln...");
            if (DictationState.Equals(SpeechSystemStatus.Running))
            {
                Debug.LogError("....aber der DictationMode lief noch.****");
                Debug.LogError("DictationState: " + DictationState);
                Debug.LogErrorFormat("ASRQueue Count: {0}", ASRModeSwitchQueue.Count);
            }
            else if (DictationState.Equals(SpeechSystemStatus.Stopped))
            {
                Debug.LogError("....und los gehts.*******");
                WWE.StartDetection();
                WantToChangeToWakeWordDetection = false;
                isSwitchingASRMode = false;
            }
            else
            {
                Debug.LogErrorFormat("......_______________ERROR!!!__________...DictationState steht auf: {0}", DictationState);
            }
        }
        /*if(PhraseRecognitionSystem.Status.Equals(SpeechSystemStatus.Stopped) && STT.dictationRecognizer.Status.Equals(SpeechSystemStatus.Stopped))
        {
            Debug.Log("------ Keine Spracherkennung aktiv. Starte WakeWordEngine");
            SwitchToWakeWordDetection("fehler erkannt");
        }*/

        int i = 0;
        foreach (AsrSwitchDelegate command in ASRModeSwitchQueue)
        {
            Debug.LogErrorFormat("Befehl {0} befindet sich an Position {1}", command.Method.Name, i);
            i++;
        }

        //Falls zuletzt ausgeführter Wechsel fertig -> nehme den nächsten aus der Warteschlange und führe ihn aus
        if (ASRModeSwitchQueue.Count != 0)
        {
            if (!isSwitchingASRMode)
            {
                if (ASRModeSwitchQueue.Count == 1)   //Bei nur einem in der Liste und keinem der gerade arbeiten kann direkt gewechselt werden
                {
                    //Ignoeriere Wechsel, wenn dieser Modus bereits aktiv ist
                    if((ASRModeSwitchQueue.Peek().Method.Name.Equals(SwitchToSTT.Method.Name) && DictationState.Equals(SpeechSystemStatus.Stopped)) || (ASRModeSwitchQueue.Peek().Method.Name.Equals(SwitchToWWE.Method.Name) && WakeWordState.Equals(SpeechSystemStatus.Stopped)))
                    {
                        Debug.LogError("Haben nur einen Befehl in der Warteschlage und führen diesen aus.");
                        ASRModeSwitchQueue.Dequeue()();
                    }
                    else
                    {
                        Debug.LogError("Modus in den gewechselt werden soll ist bereits aktiv. -> Pop ohne Ausführung");
                        ASRModeSwitchQueue.Dequeue();
                    }
                    
                }

                else
                {
                    if (ASRModeSwitchQueue.Peek().Method.Name.Equals(SwitchToSTT.Method.Name))
                    {
                        //Debug.LogError("Nehme das vordere der Warteschlage weg. **STT Befehl**");
                        //ASRModeSwitchQueue.Dequeue()();
                        AsrSwitchDelegate topFunction = ASRModeSwitchQueue.Dequeue();

                        if (DictationState.Equals(SpeechSystemStatus.Stopped))     //wenn zur STT gewechselt werden soll darf diese momentan nicht aktiv sein
                        {
                            Debug.LogError("Nehme das vordere der Warteschlage weg. **STT Befehl**");
                            topFunction();
                            //ASRModeSwitchQueue.Dequeue()();
                        }
                        else
                        {
                            Debug.LogErrorFormat("DictationState steht auf: {0}. Ich kann daher nicht zu STT wechseln. --POP--", DictationState);
                        }
                    }

                    else if (ASRModeSwitchQueue.Peek().Method.Name.Equals(SwitchToWWE.Method.Name))  //für WWE muss STT beendet sein
                    {
                        //Debug.LogError("Nehme das vordere der Warteschlage weg. **WWE Befehl**");
                        //ASRModeSwitchQueue.Dequeue()();

                        AsrSwitchDelegate topFunction = ASRModeSwitchQueue.Dequeue();
                        if (WakeWordState.Equals(SpeechSystemStatus.Stopped))
                        {
                            Debug.LogError("Nehme das vordere der Warteschlage weg. **WWE Befehl**");
                            topFunction();
                            //ASRModeSwitchQueue.Dequeue()();
                        }
                        else
                        {
                            Debug.LogErrorFormat("WakeWordState steht auf: {0}. Ich kann daher nicht zu WWE wechseln.--POP--", WakeWordState);
                        }
                    }
                    else
                    {
                        Debug.LogErrorFormat("Konnte Methode {0} nicht ausführen, da der Gegenpart noch lief.", ASRModeSwitchQueue.Peek().Method.Name);
                    }
                }
            }
            else if (isSwitchingASRMode)
            {
                Debug.LogErrorFormat("Warte mit Mode Wechsel... ein anderer wechselt noch. WWEState: {0}, STTState: {1}", WakeWordState, DictationState);
            }
        }
        else
        {
            //Debug.Log("Kein Wechsel beantragt.");
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.H))
        {
            RefreshWakeWordDetection();
        }
    }
}
