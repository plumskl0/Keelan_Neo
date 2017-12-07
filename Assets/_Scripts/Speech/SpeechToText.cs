using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class SpeechToText : MonoBehaviour {

    public DictationRecognizer dictationRecognizer;

//    private DictationRecognizer dictationRecognizer;
    private String _userCommand;
    private ASR asr;

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
        asr = GetComponent<ASR>();
        dictationRecognizer = new DictationRecognizer();
        dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
        dictationRecognizer.DictationError += DictationRecognizer_DictationError;
	}

    private void DictationRecognizer_DictationError(string error, int hresult)
    {
        Debug.Log("***ERROR: " + error);
    }

    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        asr.Print(text);
        asr.SwitchToWakeWordDetection(text);
        //_userCommand = text;
        //resultText.text = _userCommand;
        //Debug.Log(_userCommand);
    }


    public void StartDetection()
    {
        Debug.Log("Starte Dictation Mode");
        //Debug.Log(dictationRecognizer.ToString());
        dictationRecognizer.Start();
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
}
