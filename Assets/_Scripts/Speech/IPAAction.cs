﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IPAAction : MonoBehaviour {
    //public Canvas MiniMap;

    //Map Objekte
    private GameObject mapBorder;
    private Camera minimapKamera;
    private RectTransform mapBorderRectTransform;
    private RectTransform mapOverlayRect;
    private RectTransform mapRect;
    private Minimap mapScript;

    //Steuerungsobjekte
    private SharedFields sharedData;


    //Feineinstellungsvariablen:
    private float beschleunigungszeit = 1f;


    //Konstanten für gültige Aktionen des Agents
    public const string openMap = "map.Open";
    public const string closeMap = "map.Close";
    public const string wantToSetContext = "WantToSetContext";
    public const string setContext = "SetContext";
    public const string changeMapFixedStep = "map.ChangeFixedStep";
    public const string focusOnCar = "map.FocusOnCar";
    public const string moveCar = "car.Move";


    private void ChangeMapMode(String modeName) //-> geöffnet oder geschlossen
    {
        int bildSchirmBreite = Screen.width;
        int bildschirmHoehe = Screen.height;
        Vector2 kartenGroesse = mapBorderRectTransform.sizeDelta;
        Debug.Log(kartenGroesse);

        //falls die Karte geöffnet werden soll
        if (modeName.Equals("Open"))
        {
            //Verschiebe Karte in die Mitte des Bildschirms
            mapBorderRectTransform.position = new Vector3(Convert.ToSingle(0.5 * bildSchirmBreite), Convert.ToSingle(0.5 * bildschirmHoehe), 0f);
            mapBorderRectTransform.sizeDelta = new Vector2(0.8f * mapOverlayRect.sizeDelta.x, 0.8f * mapOverlayRect.sizeDelta.y);
          //  float xnew = mapBorderRectTransform.sizeDelta.x;
            //float ynew = mapBorderRectTransform.sizeDelta.y;
            //mapRect.sizeDelta = new Vector2(xnew - 6, ynew - 6); //manuelles mitvergrößern der Karte zum Rahmen, da die Kindobjekte sich nur bei Skalierung mitvergrößern
    
            //Verkleinere Minimap um weiteren Ausschnitt zu sehen
            minimapKamera.orthographicSize = 500f;
        }

        else if (modeName.Equals("Close"))
        {
            mapBorderRectTransform.sizeDelta = new Vector2(216f, 174f);
            mapBorderRectTransform.anchoredPosition = new Vector3(121f ,109f, 0f);
            
           
            //Vergrößere Minimap um kleineren Ausschnitt zu sehen
            minimapKamera.orthographicSize = 150f;
        }

        float xnew = mapBorderRectTransform.sizeDelta.x;
        float ynew = mapBorderRectTransform.sizeDelta.y;
        mapRect.sizeDelta = new Vector2(xnew - 6, ynew - 6); //manuelles mitvergrößern der Karte zum Rahmen, da die Kindobjekte sich nur bei Skalierung mitvergrößern
    }



    public void OpenMap()
    {
        ChangeMapMode("Open");
        sharedData.SetPlayerControl(false);
    }

    public void CloseMap()
    {
        ChangeMapMode("Close");
        sharedData.SetPlayerControl(true);
    }

    public void SetMinimapFokusOnCar()
    {
        mapScript.FollowMode = true;
    }

    private float MapGroesseneinheitToFloatValue (float _wenig, float _mittel, float _viel, String _groesseneinheit)
    {
        float selectedChange;
        switch (_groesseneinheit)
        {
            case "wenig":
                selectedChange = _wenig;
                break;
            case "mittel":
                selectedChange = _mittel;
                break;
            case "viel":
                selectedChange = _viel;
                break;
            default:
                Debug.LogError("FEHLER bei Groesseneinheit: Der zurückgelieferte Wert von groesseneinheit passt in keine Klasse");
                selectedChange = 0;
                break;
        }
        return selectedChange;
    } 

    public void ChangMapFixedStep(String _groesseneinheit, String _direction) 
    {
        float wenig = 20;
        float mittel = 50;
        float viel = 100;
        float selectedChange = MapGroesseneinheitToFloatValue(wenig, mittel, viel, _groesseneinheit);
        //Zoom Änderung:
        if (_direction.Equals("In")) {
            minimapKamera.orthographicSize -= selectedChange;
        }
        else if (_direction.Equals("Out"))
        {
            minimapKamera.orthographicSize -= selectedChange;
        }
        else {
            Vector3 newCamPosition = minimapKamera.transform.position;

            //Bewegungsänderung:
            bool notRegisteredDirection = false;
            switch (_direction)
            {
                case Direction.forwards:
                    newCamPosition.x += selectedChange;
                    break;
                case Direction.back:
                    newCamPosition.x -= selectedChange;
                    break;
                case Direction.left:
                    newCamPosition.z -= selectedChange;
                    break;
                case Direction.right:
                    newCamPosition.z += selectedChange;
                    break;
                default:
                    Debug.LogError("Die von der NLU gelieferte Richtung ist nicht registriert.");
                    notRegisteredDirection = true;
                    break;
            }

            if (!notRegisteredDirection)
            {
                mapScript.FollowMode = false;
                minimapKamera.transform.position = newCamPosition;
            }
        }
    }


    private IEnumerator CarAccelerationForTimeperiod(float _accelerationTimeSeconds, float groesseneinheit, String _direction)
    {
        DateTime startTime = DateTime.Now;
        sharedData.CarAutopilot = true;    //stelle auf Steuerung per Sprache


        if(_direction.Equals(Direction.forwards) || _direction.Equals(Direction.back))
        {
            sharedData.currentMaxSpeed = SharedFields.maxSpeed;
        }

        while (DateTime.Now < (startTime.AddSeconds(_accelerationTimeSeconds)))
        {
            switch (_direction) {
                case Direction.forwards:
                    sharedData.AssistantYAchse += 0.04f;
                    break;
                case Direction.back:
                    sharedData.AssistantYAchse -= 0.04f;
                    break;
                case Direction.right:
                    sharedData.AssistantXAchse += groesseneinheit * 0.04f;
                    break;
                case Direction.left:
                    sharedData.AssistantXAchse -= groesseneinheit * 0.04f;
                    break;
                default:
                    Debug.LogError("Eine Falsche Beschleunigungsrichtung wurde angegeben. Diese Funktion ist nur zum beschleunigen gedacht.");
                    break;
            }
            yield return null;
        }

        //Geschwindigkeit nach Beschleunigung halten:
        if (_direction.Equals(Direction.forwards) || _direction.Equals(Direction.back))
        {
            sharedData.currentMaxSpeed = sharedData.currentSpeed;
        }

        //Radstellung wieder auf neutral zurücksetzen
        else
        {
            sharedData.AssistantXAchse = 0;
        }


    }


    public void MoveCar(String _groesseneinheit, String _direction)
    {
        float wenig = 0.1f;
        float mittel = 0.4f;
        float viel = 1f;
        float selectedChange = MapGroesseneinheitToFloatValue(wenig, mittel, viel, _groesseneinheit);

        //Bewegungsänderung:
        // float beschleunigungszeit = 7f;

        switch (_direction)
        {
            case Direction.forwards:
                StartCoroutine(CarAccelerationForTimeperiod(beschleunigungszeit, selectedChange, Direction.forwards));
                break;
            case Direction.back:
                StartCoroutine(CarAccelerationForTimeperiod(beschleunigungszeit,selectedChange, Direction.back));
                break;
            case Direction.left:
                StartCoroutine(CarAccelerationForTimeperiod(beschleunigungszeit, selectedChange, Direction.left));
                break;
            case Direction.right:
                StartCoroutine(CarAccelerationForTimeperiod(beschleunigungszeit,selectedChange, Direction.right));
                break;
            default:
                Debug.LogError("Die von der NLU gelieferte Richtung ist nicht registriert.");
                break;
        }
    }


    public void DisplayText (Text _ausgabefeld, String _textToDisplay)
    {
        _ausgabefeld.text = _textToDisplay;
    }

    public void Speak (Text _ausgabefeld, String _textToDisplay)
    {
        DisplayText(_ausgabefeld, _textToDisplay);  //nur solange keine TTS implementiert
    }

    public void AskQuestion (Text _ausgabefeld, String _textToDisplay)
    {
        Speak(_ausgabefeld, _textToDisplay);
        EventManager.TriggerEvent(EventManager.keywordDetectedEvent, new EventMessageObject(EventManager.keywordDetectedEvent, "MultiTurnConversation"));
    }

    public void StartNavigation (Vector3 _zielort)
    {

    }


    // Use this for initialization
    void Start () {

        //benötigte Map Objekte
        mapBorder = GameObject.FindGameObjectWithTag("MinimapBorder");
        mapBorderRectTransform = mapBorder.GetComponent<RectTransform>();
        minimapKamera = GameObject.Find("MiniMapCamera").GetComponent<Camera>();
        mapOverlayRect = GameObject.Find("Map").GetComponent<RectTransform>();  //das Overlay, welches die Map enthält
        mapRect = GameObject.Find("Minimap").GetComponent<RectTransform>();
        mapScript = minimapKamera.GetComponent<Minimap>();

        //benötigte Steuerungsobjekte (Lenkung)
        sharedData = SharedFields.Instance;
    }

    // Update is called once per frame
    void Update () {
		
	}
}

public struct Direction
{
    public const string forwards = "Forwards";
    public const string back = "Back";
    public const string left = "Left";
    public const string right = "Right";
}
