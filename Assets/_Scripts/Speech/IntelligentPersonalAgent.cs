using ApiAiSDK.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    //Informationen des IPA
    private string lastIPAspeech;  //was der IPA zuletzt gesagt hat
    private string lastIPAAction; //unterscheidet sich von lastIPASpeech, da nicht alle Intents eine Action und oder Speech beeinhalten
    private Dictionary<string, string> symmetricTasksDict = new Dictionary<string, string>();
    private Dictionary<string, string> eventTriggerStringForActionDict = new Dictionary<string, string>();
    private Dictionary<string, string> repeatableTasksDict = new Dictionary<string, string>();


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
            StartCoroutine(DelayedPlayerNameCollection(0.5f));
        }
        FillSymmetricDictionary();
        FillEventTriggerDictionary();
        FillRepeatableTasksDict();
    }

    private void FillSymmetricDictionary ()
    {   //***********Problem: bei der jetzigen Implementierung werden die Undo Tasks nur lokal ausgeführt -> das heißt Kontextvariablen in Dialogflow werden nicht korrigierts
        //Müsste stattdessen pro Task einen String hinterlegen der bei ASREventDetected, die entsprechende Gegenaktion ausführt

        //direkt Ausführbar
        symmetricTasksDict.Add(IPAAction.openMap, IPAAction.closeMap);
        symmetricTasksDict.Add(IPAAction.startNavigation, IPAAction.endNavigation); //geht ohne Parameter Extraktion nur in eine Richtung
        symmetricTasksDict.Add(IPAAction.getCarControlBack, IPAAction.takeCarControl);
        symmetricTasksDict.Add(IPAAction.getPlateControlBack, IPAAction.takePlateControl);
        symmetricTasksDict.Add(IPAAction.setCheckpoint, IPAAction.discardCheckpoint);  //führt nicht dazu, dass ein vorheriger Checkpoint wiederhergestellt wird
        symmetricTasksDict.Add(IPAAction.setPlayerName, IPAAction.rename);


        //nach Parameter Extraktion ausführbar
        symmetricTasksDict.Add(IPAAction.saveNavigationPoint, IPAAction.deleteNavigationPoint);

        //nach Umkehrung der Parameter Ausführbar
        //symmetricTasks.Add(IPAAction.changeMapFixedStep, IPAAction.changeMapFixedStep); //Für Zoom und Bewegung der Karte
        //symmetricTasks.Add(IPAAction.moveCar, IPAAction.moveCar);
    }

    public void FillEventTriggerDictionary ()
    {
        //Alle hier auskommentierten Kommandos funktionieren nur in eine Richtung ohne Parameter zu speichern
        eventTriggerStringForActionDict.Add(IPAAction.openMap, "Öffne Karte");
        eventTriggerStringForActionDict.Add(IPAAction.startNavigation, "Navigiere"); //geht ohne Parameter nur als Neustart ohne Ziel
        eventTriggerStringForActionDict.Add(IPAAction.getCarControlBack, "Fahrzeugkontrolle abgeben");
        eventTriggerStringForActionDict.Add(IPAAction.getPlateControlBack, "Teller Steuerung abgeben");
        eventTriggerStringForActionDict.Add(IPAAction.setCheckpoint, "Setze Checkpoint" ); //ist nicht ganz symmetriepunkt -> setzt ihn an aktueller Position und stellt nicht alten wieder her
        //eventTriggerStringForAction.Add(IPAAction.setPlayerName);

        eventTriggerStringForActionDict.Add(IPAAction.closeMap, "Map schließen");
        eventTriggerStringForActionDict.Add(IPAAction.endNavigation, "Navigation beenden"); //geht ohne Parameter Extraktion nur in eine Richtung
        eventTriggerStringForActionDict.Add(IPAAction.takeCarControl, "Lenke das Auto" );
        eventTriggerStringForActionDict.Add(IPAAction.takePlateControl, "Balanciere den Teller");
        eventTriggerStringForActionDict.Add(IPAAction.discardCheckpoint, "Verwerfe Checkpoint");
        eventTriggerStringForActionDict.Add(IPAAction.rename, "Spielerwechsel");
        eventTriggerStringForActionDict.Add(IPAAction.deleteNavigationPoint, "Entferne Ort");

    }

    public void FillRepeatableTasksDict()
    {
        repeatableTasksDict.Add(IPAAction.setCheckpoint, "Setze Checkpoint");
        repeatableTasksDict.Add(IPAAction.rename, "Spielerwechsel");
        repeatableTasksDict.Add(IPAAction.deleteNavigationPoint, "Entferne Ort");
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

    int slotMissingRounds = 0; //zähle mehrfache Nachfrage nach fehlenden Slots
    bool wasInDialogDelegationLastRound = false;

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
            wasInDialogDelegationLastRound = true; //damit nächster Intent sieht, ob DialogDelegation zuvor aktiv war
            slotMissingRounds++;
            Debug.Log("Es fehlen noch Slotbelegungen. Ich gebe die Kontrolle an TTS");
            //Debug.Log("WWE Status " + asr.WakeWordState);
            //Debug.Log("STT Status " + asr.DictationState);
            EventManager.TriggerEvent(EventManager.keywordDetectedEvent, new EventMessageObject(EventManager.keywordDetectedEvent, "Slots fehlen"));
            //actions.DisplayText(debugText, nluResponse.Result.Fulfillment.Speech);
            string cancelInfo = "";

            if(slotMissingRounds>1)
            {
                cancelInfo = "Du kannst ungewollte Anfragen jederzeit beenden, indem du abbrechen sagst.";
            }
            WindowsVoice.speak(string.Format("{0} {1}", cancelInfo, nluResponse.Result.Fulfillment.Speech), delay: 0f);
        }

        //ansonsten rufe die Handler auf
        else
        {
            slotMissingRounds = 0;
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
            bool canceledDialogDelegation = nluResponse.Result.ResolvedQuery.Equals("abbrechen") && wasInDialogDelegationLastRound ? true:false;    //Dialogflow eigenes Abbrechen simuliert endConversation nicht

            if (nluResultObj.Fulfillment.Speech != "")
            {
                if (nluResultObj.GetStringParameter("endConversation").Equals("true") || canceledDialogDelegation)  //***evtl. muss hier eine Abfrage rein die beim default fallback auch das mikrofon schließt
                {
                    actions.Speak(nluResultObj.Fulfillment.Speech);
                }
                else  //ansonsten öffne das Mikrofon wieder
                {
                    actions.AskQuestion(nluResultObj.Fulfillment.Speech);
                }
            }

            String action = nluResponse.Result.Action;

            bool noActionIntent = false;  //wird true falls der Intent keine Action enthält oder diese nicht implementiert ist

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

                case IPAAction.takeCarControl:
                    actions.TakeCarControl();
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

                case IPAAction.deleteNavigationPoint:
                    int zielNummer2 = Int32.Parse(nluResultObj.GetStringParameter("navigationNumber"));
                    actions.DeleteNavigationPoint(zielNummer2);
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
                    if (sharedData.savedPlacesOnMap.Count > zielNummer - 1)
                    {
                        Vector3 target = sharedData.savedPlacesOnMap[zielNummer - 1].transform.position;
                        actions.StartNavigation(target);
                    }
                    else
                    {
                        WindowsVoice.speak(string.Format("Ich konnte keinen Ort mit Nummer {0} finden.", zielNummer), 3);
                    }
                    break;

                case IPAAction.endNavigation:
                    actions.EndNavigation();
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
                    actions.SetTrainingCheckpoint(0);
                    break;

                case IPAAction.endTrainingRouteCreation:
                    actions.EndTraining();
                    /*sharedData.trainingRouteRecordingStopped = true;    //Beendet hinzufügen neuer Framestrokes in AlternateCarController


                    //Entferne PlayerControl und Ball
                    sharedData.SetPlayerControl(false);
                    GameObject.FindGameObjectWithTag("Ball").SetActive(false);
                    if (String.IsNullOrEmpty(sharedData.playerName))
                    {
                        Debug.LogError("Fehler bei Namenserfassung. Öffne Texteingabe.");
                        GameObject.Find("NameQuestionCanvas").GetComponent<Canvas>().enabled = true;
                    }*/
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

                case IPAAction.rename:
                    EventManager.TriggerEvent(EventManager.asrRequerstDetectedEvent, new EventMessageObject(EventManager.asrRequerstDetectedEvent, "Spielerwechsel"));
                    break;

                //Allgemeine Befehle:
                case IPAAction.repeatAnswer:
                    actions.Speak(lastIPAspeech);
                    break;

                case IPAAction.repeatLastAction:
                    if(repeatableTasksDict.ContainsKey(lastIPAAction))
                    {
                        EventManager.TriggerEvent(EventManager.asrRequerstDetectedEvent, new EventMessageObject(EventManager.asrRequerstDetectedEvent, repeatableTasksDict[lastIPAAction]));
                    }
                    break;

                case IPAAction.undo:
                    string undoAction = "";
                    Debug.LogError("Last Action was: " + lastIPAAction);
                    if (symmetricTasksDict.ContainsKey(lastIPAAction))
                    {
                        undoAction = symmetricTasksDict[lastIPAAction];
                    }
                    else if (symmetricTasksDict.ContainsValue(lastIPAAction))
                    {
                        foreach (KeyValuePair<string, string> keyValue in symmetricTasksDict)
                        {
                            if (keyValue.Value.Equals(lastIPAAction))
                                undoAction = keyValue.Key;
                        }
                    }
                    else
                    {
                        Debug.LogErrorFormat("Der letzte Task hatte keine Implementierung eines symmetrischen Gegenparts");
                        WindowsVoice.speak("Der letzte Task hatte keine Implementierung eines symmetrischen Gegenparts", 2);
                    }


                    if (eventTriggerStringForActionDict.ContainsKey(undoAction))
                    {
                        string voiceCommandForUndoAction = eventTriggerStringForActionDict[undoAction];
                        EventManager.TriggerEvent(EventManager.asrRequerstDetectedEvent, new EventMessageObject(EventManager.asrRequerstDetectedEvent, voiceCommandForUndoAction));
                    }
                    else
                    {
                        actions.Speak("Ich kann die letzte Aktion nicht rueckgängig machen. Die eingetragenen Funktionspaare funktionieren nur in die andere Richtung.");
                    }
                    break;

                default:
                    Debug.Log(string.Format("Der Intent {0} wurde im IntentHandler nicht registiert.", intent));
                    noActionIntent = true;
                    //WindowsVoice.speak("Diesen Intent kenne ich nicht", 0f);  //wird von Fallback Intent in Block unten gemacht
                    break;
            }

            //Die letzte Aktion soll von Intents ohne Action nicht überschrieben werden
            if(!noActionIntent)
            {
                lastIPAAction = action;
            }

        }
        lastIPAspeech = nluResponse.Result.Fulfillment.Speech;
        Debug.LogError(lastIPAspeech + " ist zuletzt gesagtes");

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
