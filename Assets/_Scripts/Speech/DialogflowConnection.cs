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

    public Text answerTextField;
    public Text inputTextField;
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

        //apiAiUnity.OnError += HandleOnError;
        //apiAiUnity.OnResult += HandleOnResult;
    }

    //void HandleOnResult(object sender, AIResponseEventArgs e)
    //{
    //    var aiResponse = e.Response;
    //    if (aiResponse != null)
    //    {
    //        Debug.Log("Folgende Anfrage kam an: " + aiResponse.Result.ResolvedQuery);
    //        var outText = JsonConvert.SerializeObject(aiResponse, jsonSettings);

    //        Debug.Log(outText);

    //        answerTextField.text = outText;

    //    }
    //    else
    //    {
    //        Debug.LogError("Response is null");
    //    }
    //}

    //void HandleOnError(object sender, AIErrorEventArgs e)
    //{
    //    RunInMainThread(() => {
    //        Debug.LogException(e.Exception);
    //        Debug.Log(e.ToString());
    //        answerTextField.text = e.Exception.Message;
    //    });
    //}

    // Update is called once per frame
    void Update()
    {
        if (apiAiUnity != null)
        {
            apiAiUnity.Update();
        }

        // dispatch stuff on main thread
        //while (ExecuteOnMainThread.Count > 0)
        //{
        //    ExecuteOnMainThread.Dequeue().Invoke();
        //}
    }

    //private void RunInMainThread(Action action)
    //{
    //    ExecuteOnMainThread.Enqueue(action);
    //}

    //public void PluginInit()
    //{

    //}

    public AIResponse SendVoiceText(string input)
    {
        //var text = inputTextField.text;
        //var text = answerTextField.text;
        var text = input;
        Debug.Log(text);

        AIResponse response = apiAiUnity.TextRequest(text);

        if (response != null)
        {
            Debug.Log("Resolved query: " + response.Result.ResolvedQuery);
            Debug.Log(response.Result.Metadata.IntentName);

            var outText = JsonConvert.SerializeObject(response, jsonSettings);
            Debug.Log("Result: " + outText);

            answerTextField.text = outText;
            return response;
        }
        else
        {
            Debug.LogError("Response is null");
            return response;
        }

    }


    public void SendText()
    {
        var text = inputTextField.text;

        Debug.Log(text);

        AIResponse response = apiAiUnity.TextRequest(text);

        if (response != null)
        {
            Debug.Log("Resolved query: " + response.Result.ResolvedQuery);

            var outText = JsonConvert.SerializeObject(response, jsonSettings);

            Debug.Log("Result: " + outText);

            answerTextField.text = outText;
        }
        else
        {
            Debug.LogError("Response is null");
        }

    }
}
