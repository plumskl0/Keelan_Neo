using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAutomaticSpeechInterface {
    void AddWakeWords(String[] wordsToAdd);
    //void SwitchToWakeWordDetection(EventMessageObject args);
    //void SwitchToSpeechToText(EventMessageObject args);
    //-> muss nur die beiden entsprechenden Events verarbeiten

}
