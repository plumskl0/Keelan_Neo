using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public interface IWakeWordEngineInterface{
    void AddWakeWords(String[] wordsToAdd);
    void InitDetection();
    void StartDetection();
    void StopDetection();
    SpeechSystemStatus GetState();
}
