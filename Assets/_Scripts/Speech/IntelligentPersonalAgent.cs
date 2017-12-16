using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class IntelligentPersonalAgent : MonoBehaviour {
    private ASR asr;
    private NLU nlu;
    public Text debugText;
    private UnityAction callNLU;


    private void Awake()
    {
        asr = gameObject.AddComponent<ASR>();
        nlu = gameObject.AddComponent<NLU>();
        callNLU = new UnityAction<object>(nlu.UnderstandRequest());
    }

    private void OnEnable()
    {
        EventManager.StartListening("SpeechCommandRegocnized", callNLU);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SendTextToNLU()
    {

    }
}
