using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class WakeWordEngine : MonoBehaviour, IWakeWordEngineInterface {

    //private string[] keywords = new String[] { "computer", "auto" };
    private List<String> WakeWords = new List<String>();
    public KeywordRecognizer keywordRecognizer;
    private ASR asr;


	// Use this for initialization
	void Awake () {
        asr = GetComponent<ASR>();
        AddWakeWords(new String[] { "computer", "Auto" });
        keywordRecognizer = new KeywordRecognizer(WakeWords.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        PhraseRecognitionSystem.OnError += (errorCode) =>
        {
            Debug.LogError(string.Format("***************Es ist ein Fehler in der WakeWord Engine aufgetreten: {0}", errorCode.ToString()));
        };

    }

    private void PhraseRecognitionSystem_OnError(SpeechError errorCode)
    {
        throw new NotImplementedException();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Keyword erkannt: " + args.text);
        EventManager.TriggerEvent(EventManager.keywordDetectedEvent, new EventMessageObject(EventManager.keywordDetectedEvent, args.text));
        Debug.Log("Trigger WakewordDetected Event");
        //asr.SwitchToSpeechToText(args.text, asr.debugText);   wird jetzt durch Events getriggert
    }

    public void AddWakeWords(String[] wordsToAdd)
    {
        foreach (string s in wordsToAdd)
        {
            WakeWords.Add(s);
        }
    }

    public void InitDetection ()
    {
        keywordRecognizer.Start();
    }

    public void StartDetection()
    {
        PhraseRecognitionSystem.Restart();
        Debug.Log("Starte Keyword Detection");
        /*
        if (!keywordRecognizer.IsRunning)   //nur erster Methodenaufruf darf über Start() kommen
        {
            keywordRecognizer.Start();
        }
        else
        {
            PhraseRecognitionSystem.Restart();
        }*/
    }

    public void StopDetection()
    {
        Debug.Log("Stoppe Keyword Detection");
        PhraseRecognitionSystem.Shutdown();
        /*if (keywordRecognizer.IsRunning)
        {
            Debug.Log("Stoppe Keyword Detection");
            PhraseRecognitionSystem.Shutdown();
            //keywordRecognizer.Stop();
        }*/
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public SpeechSystemStatus GetState()
    {
        return PhraseRecognitionSystem.Status;
    }
}
