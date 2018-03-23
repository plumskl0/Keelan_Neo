extern alias NewJSon;
extern alias fastJ;

using ApiAiSDK;
using ApiAiSDK.Model;
using ApiAiSDK.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
//using Newtonsoft.Json;
using System.Net;
using UnityEngine;

public class DialogflowConnection : MonoBehaviour
{
    private ApiAiUnity apiAiUnity;
    private AudioSource aud;
    //public AudioClip listeningSound;

    /*private readonly fastJSON.JSONParameters jsons = new JSONParameters
    {
        NullValueHandling = NullValueHandling.Ignore
    };*/


   private readonly NewJSon::Newtonsoft.Json.JsonSerializerSettings jsonSettings = new NewJSon::Newtonsoft.Json.JsonSerializerSettings
    {
        //StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
        NullValueHandling = NewJSon::Newtonsoft.Json.NullValueHandling.Ignore
        //StringEscapeHandling = Newtonsoft.Json.StringEscapeHandling
    };

    private readonly Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    // Use this for initialization
    IEnumerator Start()
    {
        // check access to the Microphone
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            throw new NotSupportedException("Microphone using not authorized");
        }

        ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) =>
        {
            return true;
        };

        const string ACCESS_TOKEN = "3634f11198d345b5aee6a88ec6a93065";

        var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.German);

        apiAiUnity = new ApiAiUnity();
        apiAiUnity.Initialize(config);
        apiAiUnity.OnError += HandleOnError;
    }

    private void HandleOnError(object sender, AIErrorEventArgs e)
    {
        Debug.Log("******Fehler bei APIAI Module: " + e.Exception.Message);
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
            //var outText = fastJSON.JSON.ToJSON(response);
            var outText = NewJSon::Newtonsoft.Json.JsonConvert.SerializeObject(response, jsonSettings);
            Debug.Log("Result: " + outText);

            return response;
        }
        else
        {
            Debug.LogError("Response is null");
            return response;
        }

    }
}
