using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public interface ISpeechToTextInterface {
    void StartDetection();
    void StopDetection();
    SpeechSystemStatus GetState();

}
