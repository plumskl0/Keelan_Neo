using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

[RequireComponent(typeof(AudioSource))]

public class STTGoogleCloudSpeech : MonoBehaviour, ISpeechToTextInterface {
    public Text text;
    private ASR asr;

    struct ClipData
    {
        public int samples;
    }

    const int HEADER_SIZE = 44;

    private int minFreq;
    private int maxFreq;

    private bool micConnected = false;

    //A handle to the attached AudioSource
    private AudioSource goAudioSource;

    public string apiKey = "AIzaSyBmMVWXPniL8yl2CSVLuh7NE3Uj4VhCO_s";

    public SpeechSystemStatus GetState()
    {
        if (micConnected)
        {
            if (!Microphone.IsRecording(null))
            {
                return SpeechSystemStatus.Stopped;
            }
            else
            {
                return SpeechSystemStatus.Running;
            }
        }
        else
        {
            Debug.LogError("Statusüberprüfung der STT ergab, dass dieser kein Mikrofon verfügbar ist");
            return SpeechSystemStatus.Failed;
        }


    }

    public IEnumerator RecordCoroutine()
    {

        //Start recording and store the audio captured from the microphone at the AudioClip in the AudioSource
        Debug.LogError("Starte recording");
        goAudioSource.clip = Microphone.Start(null, false, 7, maxFreq); //Currently set for a 7 second clip
        //Spiele die Audio Source um in Update anhand der Spectrum Data zu ermitteln ob jemand spricht oder nicht
        /*while (!(Microphone.GetPosition(null) > 0)) { }
        goAudioSource.volume = 0.001f;
        goAudioSource.Play( );*/

        while (Microphone.IsRecording(null))
        {
            //Debug.LogError("Recording in progress");
            yield return null;
        }

        float filenameRand = UnityEngine.Random.Range(0.0f, 10.0f);

        string filename = "testing" + filenameRand;

        //Microphone.End(null); //Stop the audio recording

        Debug.Log("Recording Stopped");

        if (!filename.ToLower().EndsWith(".wav"))
        {
            filename += ".wav";
        }

        var filePath = Path.Combine("testing/", filename);
        filePath = Path.Combine(Application.persistentDataPath, filePath);
        Debug.Log("Created filepath string: " + filePath);

        // Make sure directory exists if user is saving to sub dir.
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        SavWav.Save(filePath, goAudioSource.clip); //Save a temporary Wav File
        Debug.Log("Saving @ " + filePath);
        string apiURL = "https://speech.googleapis.com/v1/speech:recognize?&key=" + apiKey;
        string Response;

        Debug.Log("Uploading " + filePath);

        Task<string> googleCloudSpeechResultTask = Task.Run<string>(() => HttpUploadFile(apiURL, filePath, "file", "audio/wav; rate=44100"));
        while (!googleCloudSpeechResultTask.IsCompleted)
        {
            yield return null;
        }

        Response = googleCloudSpeechResultTask.Result;
        //Response = HttpUploadFile(apiURL, filePath, "file", "audio/wav; rate=44100");
        Debug.Log("Response String: " + Response);

        var jsonresponse = SimpleJSON.JSON.Parse(Response);

        if (jsonresponse != null)
        {
            string resultString = jsonresponse["results"][0].ToString();
            var jsonResults = SimpleJSON.JSON.Parse(resultString);

            string transcripts = jsonResults["alternatives"][0]["transcript"].ToString();

            Debug.Log("transcript string: " + transcripts);
            //TextBox.text = transcripts;
            //text.text = transcripts;
            asr.LastCommand = transcripts;
            Debug.Log("Trigger asrRequestDetected Event");
            EventManager.TriggerEvent(EventManager.asrRequerstDetectedEvent, new EventMessageObject(EventManager.asrRequerstDetectedEvent, transcripts));

        }
        //goAudioSource.Play(); //Playback the recorded audio

        File.Delete(filePath); //Delete the Temporary Wav file


    }


    public void StartDetection()
    {
        Debug.Log("Starte Dictation Mode");
        //If there is a microphone
        if (micConnected)
        {
            //If the audio from any microphone isn't being recorded
            if (!Microphone.IsRecording(null))
            {
                StartCoroutine(RecordCoroutine());
                //GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 25, 200, 50), "Recording in progress...");
            }
            else
            {
                Debug.LogError("Versuch auf Mic zuzugreifen, während dieses bereits lief");
            }
        }
        
        else // No microphone
        {
            //Print a red "Microphone not connected!" message at the center of the screen
            GUI.contentColor = Color.red;
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 25, 200, 50), "Microphone not connected!");
        }
    }

    public void StopDetection()
    {
        Debug.Log("Stop erfolgt bei Google Cloud STT automatisch");
    }

    // Use this for initialization
    void Awake()
    {
        asr = GetComponent<ASR>();

        //Check if there is at least one microphone connected
        if (Microphone.devices.Length <= 0)
        {
            //Throw a warning message at the console if there isn't
            Debug.LogWarning("Microphone not connected!");
        }
        else //At least one microphone is present
        {
            //Set 'micConnected' to true
            micConnected = true;
            //Debug.Log("Habe ein Mic");

            //Get the default microphone recording capabilities
            Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);

            //According to the documentation, if minFreq and maxFreq are zero, the microphone supports any frequency...
            if (minFreq == 0 && maxFreq == 0)
            {
                //...meaning 44100 Hz can be used as the recording sampling rate
                maxFreq = 44100;
            }

            //Get the attached AudioSource component
            goAudioSource = this.GetComponent<AudioSource>();
        }
    }

    float[] clipSampleData = new float[1024];
    bool isSpeaking = false;
    float minimumLevel = 0.00001f;

    // Update is called once per frame
    void Update () {
        /*goAudioSource.GetSpectrumData(clipSampleData, 0, FFTWindow.Rectangular);
        float currentAverageVolume = clipSampleData.Average();
        Debug.Log(currentAverageVolume);

        if (currentAverageVolume > minimumLevel)
        {
            isSpeaking = true;
        }
        else if (isSpeaking)
        {
            isSpeaking = false;
            //volume below level, but user was speaking before. So user stopped speaking
        }*/
    }

    public string HttpUploadFile(string url, string file, string paramName, string contentType)
    {

        System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
        Debug.Log(string.Format("Uploading {0} to {1}", file, url));

        Byte[] bytes = File.ReadAllBytes(file);
        String file64 = Convert.ToBase64String(bytes,
                                         Base64FormattingOptions.None);

        Debug.Log(file64);

        try
        {

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {




                string json = "{ \"config\": { \"languageCode\" : \"de-DE\" }, \"audio\" : { \"content\" : \"" + file64 + "\"}}";

                Debug.Log(json);
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Debug.Log(httpResponse);

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Debug.Log("Response:" + result);
                return result;

            }

        }
        catch (WebException ex)
        {
            var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
            Debug.Log(resp);

        }


        return "empty";

    }

}
