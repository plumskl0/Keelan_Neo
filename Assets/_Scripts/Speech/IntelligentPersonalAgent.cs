using ApiAiSDK.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntelligentPersonalAgent : MonoBehaviour {
    //Szenenobjekte
    public GameObject minimapLocationIcon;

    //Scriptvwerweise
    private IAutomaticSpeechInterface asr;
    private INaturalLanguageUnderstandingInterface nlu;
    private WindowsVoice tts;
    private IPAAction actions;
    public Text debugText;
    private SharedFields sharedData = SharedFields.Instance;

    //Unity Actions for Event Manager
    private UnityAction<EventMessageObject> CallNLU;
    private UnityAction<EventMessageObject> Print;
    private UnityAction<EventMessageObject> IntentHandler;


    private void Awake()
    {
        if (!sharedData.TrainingMode)  //aktiviere Sprachassistent nicht beim ML Training -> Keywords können Simulation stören
        {
            asr = gameObject.AddComponent<ASR>();
            nlu = gameObject.AddComponent<NLU>();
            actions = gameObject.AddComponent<IPAAction>();
            tts = gameObject.AddComponent<WindowsVoice>();


            CallNLU = new UnityAction<EventMessageObject>(SendTextToNLU);
            Print = new UnityAction<EventMessageObject>(PrintMessageBody);
            IntentHandler = new UnityAction<EventMessageObject>(HandleIntent);
        }
    }
    private void Start()
    {
        if (!sharedData.TrainingMode)
        {
            Debug.Log("Untersuche Namen des Spielers");
            StartCoroutine(DelayedPlayerNameCollection(2.0f));
        }
    }
    IEnumerator DelayedPlayerNameCollection(float waitTime)
    {
        Debug.Log("Warte...");
        yield return new WaitForSeconds(waitTime);
        if (File.Exists(sharedData.userInfoPath))
        {
            Debug.LogError("Datei war schon da");
            StreamReader streamReader = new StreamReader(sharedData.userInfoPath);
            string[] userInfoArray = streamReader.ReadToEnd().Split(';');
            bool foundNameEntrie = false;

            foreach (string s in userInfoArray)
            {
                string[] keyValue = s.Split(':');
                if (keyValue[0].Equals("name"))
                {
                    foundNameEntrie = true;
                    sharedData.playerName = keyValue[1];
                    EventManager.TriggerEvent(EventManager.asrRequerstDetectedEvent, new EventMessageObject(EventManager.asrRequerstDetectedEvent, "Lade Namen aus Speicher " + sharedData.playerName)); //Informiere NLU darüber, dass der Name des Spielers bekannt ist
                }
            }
            streamReader.Close();
            if(!foundNameEntrie)   //Falls Dabei existiert ohne hinterlegten Namen -> behandle wie neuen Spieler
            {
                Debug.LogError("PlayerName nicht vorhanden, obwohl Datei exisitert. Wird erstellt wenn ich den Namen habe.");
                EventManager.TriggerEvent(EventManager.asrRequerstDetectedEvent, new EventMessageObject(EventManager.asrRequerstDetectedEvent, "Neuen Spieler anmelden"));
            }
        }
        else
        {
            Debug.LogError("UserInfo nicht vorhanden. Wird erstellt wenn ich den Namen habe.");
            EventManager.TriggerEvent(EventManager.asrRequerstDetectedEvent, new EventMessageObject(EventManager.asrRequerstDetectedEvent, "Neuen Spieler anmelden"));
        }
    }

    private void OnEnable()
    {
        EventManager.StartListening(EventManager.keywordDetectedEvent, Print);
        EventManager.StartListening(EventManager.asrRequerstDetectedEvent, CallNLU);
        EventManager.StartListening(EventManager.nluAnswerDetectedEvent, HandleIntent);
        //EventManager.StartListening("SpeechCommandRegocnized", nlu.UnderstandRequest());

    }

    public void HandleIntent (EventMessageObject nluAnswer)
    {
        AIResponse nluResponse = (AIResponse) nluAnswer.MessageBody;
        Debug.Log("##### Habe folgenden Intent erkannt und möchte ihn jetzt verarbeiten: " + nluResponse.Result.Metadata.IntentName);

        //Dialog Delegation prüfen
        Debug.Log("Überprüfe ob noch Slots fehlen: ");
        //AIOutputContext context = nluResponse.Result.GetContext();
        AIOutputContext[] context = nluResponse.Result.Contexts;
        bool slotsMissing = false;
        foreach (AIOutputContext con in context)
        {
            Debug.Log(con.Name);
            if(con.Name.Contains("dialog_context"))
            {
                slotsMissing = true;
            }
        }
        if (slotsMissing)
        {
            Debug.Log("Es fehlen noch Slotbelegungen. Ich gebe die Kontrolle an TTS");
            //Debug.Log("WWE Status " + asr.WakeWordState);
            //Debug.Log("STT Status " + asr.DictationState);
            EventManager.TriggerEvent(EventManager.keywordDetectedEvent, new EventMessageObject(EventManager.keywordDetectedEvent, "Slots fehlen"));
            //actions.DisplayText(debugText, nluResponse.Result.Fulfillment.Speech);
            WindowsVoice.speak(string.Format("{0}", nluResponse.Result.Fulfillment.Speech), delay: 0f);
        }

        //ansonsten rufe die Handler auf
        else
        {
            Debug.Log("Alle Slots gefüllt:");
            Dictionary<String, System.Object> dic = nluResponse.Result.Parameters;
            foreach (String key in dic.Keys)
            {
                Debug.Log(string.Format("Parameter: {0}", key));
            }
            actions.DisplayText(debugText, nluResponse.Result.Fulfillment.Speech);

            String intent = nluResponse.Result.Metadata.IntentName;
            Result nluResultObj = nluResponse.Result;

            //Ausgabe der Dialogflow Response -> Metadata.EndConversation Boolean bestimmt ob als Frage oder Aussage
            //***Kann in Unity JSON Objekt bisher nicht abgerufen werden -> Parameter: endConversation simuliert ihn
            if (nluResultObj.Fulfillment.Speech != "")
            {
                if (nluResultObj.GetStringParameter("endConversation").Equals("true"))  //***evtl. muss hier eine Abfrage rein die beim default fallback auch das mikrofon schließt
                {
                    actions.Speak(nluResultObj.Fulfillment.Speech);
                }
                else  //ansonsten öffne das Mikrofon wieder
                {
                    actions.AskQuestion(nluResultObj.Fulfillment.Speech);
                }
            }

            String action = nluResponse.Result.Action;
            switch (action) {

                /*case IPAAction.askQuestion:
                    actions.AskQuestion(nluResultObj.GetStringParameter("speak"));
                    break;


                case IPAAction.speak:
                    actions.Speak(nluResultObj.GetStringParameter("speak"));
                    break;*/

                //Intents zur Fahrzeugsteuerung
                case IPAAction.moveCar:
                    String groesseneinheit = nluResultObj.GetStringParameter("Groesseneinheit");
                    String direction = nluResultObj.GetStringParameter("MoveDirection");
                    actions.MoveCar(groesseneinheit, direction);
                    break;

                case IPAAction.stopCar:
                    actions.StopCarMovement();
                    break;

                case IPAAction.getCarControlBack:
                    actions.GetCarControlBack();
                    break;

                case IPAAction.takePlateControl:
                    actions.TakePlateControl();
                    break;

                case IPAAction.getPlateControlBack:
                    actions.GetPlateControlBack();
                    break;

                //Map Manipulation Intents:
                case IPAAction.openMap:
                    actions.OpenMap();
                    break;

                case IPAAction.closeMap:
                    actions.CloseMap();
                    actions.SetMinimapFokusOnCar();
                    break;

                case IPAAction.saveNavigationPoint:
                    actions.SaveNavigationPoint(minimapLocationIcon);
                    break;

                case IPAAction.changeMapFixedStep:
                    groesseneinheit = nluResponse.Result.GetStringParameter("Groesseneinheit");
                    direction = nluResponse.Result.GetStringParameter("Direction");
                    if (direction.Length == 0)
                    {
                        direction = nluResponse.Result.GetStringParameter("MoveDirection");
                    }

                    actions.ChangMapFixedStep(groesseneinheit, direction);
                    break;
                case IPAAction.focusOnCar:
                    actions.SetMinimapFokusOnCar();
                    break;

                //Navigation Intents:
                case IPAAction.startNavigation:
                    int zielNummer = Int32.Parse(nluResultObj.GetStringParameter("navigationNumber"));
                    Vector3 target = sharedData.savedPlacesOnMap[zielNummer - 1].transform.position;
                    actions.StartNavigation(target);
                    break;

                //Intents zur Unterstützung der Teststreckenerstellung
                case IPAAction.restartTraining:
                    SceneManager.LoadScene("Level1Training");   // Todo:Index auf Namen der Szene ändern 
                    break;

                case IPAAction.setCheckpoint:
                    actions.SetTrainingCheckpoint(sharedData.currentFrameCount);
                    actions.Speak("Sicherungspunkt erstellt bei Frame:" + sharedData.currentFrameCount);
                    break;

                case IPAAction.discardCheckpoint:
                    actions.SetTrainingCheckpoint(sharedData.currentFrameCount);
                    break;

                case IPAAction.endTrainingRouteCreation:
                    sharedData.trainingRouteRecordingStopped = true;    //Beendet hinzufügen neuer Framestrokes in AlternateCarController


                    //Entferne PlayerControl und Ball
                    sharedData.SetPlayerControl(false);
                    GameObject.FindGameObjectWithTag("Ball").SetActive(false);
                    if (String.IsNullOrEmpty(sharedData.playerName))
                    {
                        Debug.LogError("Fehler bei Namenserfassung. Öffne Texteingabe.");
                        GameObject.Find("NameQuestionCanvas").GetComponent<Canvas>().enabled = true;
                    }
                    break;

                case IPAAction.performanceAndDifficultyMeasured:
                    bool performanceOK = nluResultObj.GetStringParameter("Performance").Equals("gut") ? true : false;
                    if(sharedData.debugMode && performanceOK)
                    {
                        //Debug.LogError(WindowsVoice.statusMessage);
                        sharedData.trainingRouteDifficulty = nluResultObj.GetStringParameter("Difficulty") + "/";   //sobald gesetzt schreibt AlternateCarController die Route entsprechend ca Codezeile 500

                        //Beende verzögert
                        StartCoroutine(SharedFields.DelayedQuit(10f));
                    }
                    else
                    {
                        Debug.LogError(nluResultObj.GetStringParameter("Performance"));
                        actions.Speak("Verwerfe Strecke aufgrund schlechter Performance. Beende Programm");
                        IEnumerator co = SharedFields.DelayedQuit(10f);
                        StartCoroutine(co);

                        //Application.Quit();
                    }
                    break;
                case IPAAction.wantToSetContext:
                    int firstFreeContext = context.Length;
                    AIOutputContext[] newContext = new AIOutputContext[firstFreeContext + 1];
                    AIOutputContext contextToInsert = new AIOutputContext();
                    contextToInsert.Name = "TestContext";
                    contextToInsert.Lifespan = 3;
                    newContext[firstFreeContext] = contextToInsert;
                    context = newContext;
                    Debug.Log("So sollte es aussehen:");
                    foreach (AIOutputContext con in context)
                    {
                        Debug.Log(con.Name);
                        if (con.Name.Contains("dialog_context"))
                        {
                            slotsMissing = true;
                        }
                    }
                    Debug.Log("Trigger SpeechCommandRecognized to send new Context to Dialogflow");
                    EventManager.TriggerEvent(EventManager.asrRequerstDetectedEvent, new EventMessageObject(EventManager.asrRequerstDetectedEvent, string.Format("Rueckantwort Kontext einfuegen")));
                    break;

                case IPAAction.setContext:
                    Debug.Log("Habe Kontext gesetzt.");
                    foreach (AIOutputContext con in context)
                    {
                        Debug.Log(con.Name);
                        if (con.Name.Contains("dialog_context"))
                        {
                            slotsMissing = true;
                        }
                    }
                    break;

                //PlayerInfo
                case IPAAction.setPlayerName:
                    //var outText = NewJSon::Newtonsoft.Json.JsonConvert.SerializeObject(nluResultObj.GetJsonParameter("UserName"), jsonSettings);
                    //Debug.LogError(outText);

                    string name ="";
                    //Sonderbehandlung falls Dialogflow Entitie givenName benutzt -> bekomme neues Dict statt String
                    Dictionary<string, object> parameterDict = nluResultObj.Parameters;
                    Type t = parameterDict["UserName"].GetType();
                    bool isDict = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>);
                    Dictionary<string, object> givenNameEntitie;
                    if (isDict)
                    {
                        givenNameEntitie = (Dictionary<string, object>) parameterDict["UserName"];
                        name = (string) givenNameEntitie["given-name"];
                    }
                    else
                    {
                        name = nluResultObj.GetStringParameter("UserName");
                    }

                    //Debug.LogErrorFormat("is dict? {0}", isDict);

                    //Debug.LogError( parameterDict["UserName"].GetType());
                    
                    Debug.LogError("Haben folgenden Namen erkannt: " + name);
                    if (String.IsNullOrEmpty(name))
                        Debug.LogError("******Name konnte nicht gesetzt werden. Baue erneute Nachfrage ein?");
                    actions.SetPlayerName(name);
                    Debug.LogError("Der neue Name ist: " + sharedData.playerName);
                    break;

                default:
                    Debug.Log(string.Format("Der Intent {0} wurde im IntentHandler nicht registiert.", intent));
                    //WindowsVoice.speak("Diesen Intent kenne ich nicht", 0f);  //wird von Fallback Intent in Block unten gemacht
                    break;
            }




}

    }


    //Gibt ein Präfix gefolgt vom Inhalt der EventMessage im debugTextfeld aus
    private void PrintMessageBody(EventMessageObject _eventMessageObject)
    {
        string messageBody;
        try
        {
            messageBody = (string)_eventMessageObject.MessageBody;
        }
        catch (Exception e)
        {
            Debug.LogError("Falscher Typ des Message Bodys Objekts" +  e.Message);
            messageBody = "ERROR";
        }
        string messageType = _eventMessageObject.Type;



        if (messageType.Equals(EventManager.keywordDetectedEvent))
        {
            debugText.text = "Keyword detected: " +  messageBody;
        }

        else if (messageType.Equals(EventManager.asrRequerstDetectedEvent))
        {
            debugText.text = "ASR Request detected: " + messageBody;
        }

        else if (messageType.Equals(EventManager.nluAnswerDetectedEvent))

        {
            debugText.text = "NLU answer detected: " + messageBody ;
                 
        }


    }


    public void SendTextToNLU(EventMessageObject asrRequestMessageObject)
    {
        string asrRequest = (string) asrRequestMessageObject.MessageBody;
        PrintMessageBody(asrRequestMessageObject);
        nlu.UnderstandRequest(asrRequest);
    }
}
