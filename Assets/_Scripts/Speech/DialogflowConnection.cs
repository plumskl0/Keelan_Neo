using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using ApiAiSDK;
using ApiAiSDK.Model;
using ApiAiSDK.Unity;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Generic;

public class DialogflowConnection : MonoBehaviour
{
    private ApiAiUnity apiAiUnity;
    private AudioSource aud;
    //public AudioClip listeningSound;

    private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
       //StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
        //NullValueHandling = NullValueHandling.Ignore
        //StringEscapeHandling = Newtonsoft.Json.StringEscapeHandling
    };

    private readonly Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    // Use this for initialization
    void Start()
    {
        //const string ACCESS_TOKEN = "3485a96fb27744db83e78b8c4bc9e7b7";
        const string ACCESS_TOKEN = "3634f11198d345b5aee6a88ec6a93065";

        var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.German);

        apiAiUnity = new ApiAiUnity();
        apiAiUnity.Initialize(config);
    }

    // Update is called once per frame
    void Update()
    {
        if (apiAiUnity != null)
        {
            apiAiUnity.Update();
        }
    }



    public AIResponse SendVoiceText(string input)
    {
        //var text = inputTextField.text;
        //var text = answerTextField.text;
        var text = input;
        //Debug.Log(text);

        AIResponse response = apiAiUnity.TextRequest(text);

        if (response != null)
        {
            //Debug.Log("Resolved query: " + response.Result.ResolvedQuery);
            //Debug.Log(response.Result.Metadata.IntentName);

            //var outText = JsonConvert.SerializeObject(response, jsonSettings);
            //Debug.Log("Result: " + outText);

            return response;
        }
        else
        {
            Debug.LogError("Response is null");
            return response;
        }

    }
}
