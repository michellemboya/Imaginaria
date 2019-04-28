using System.Collections;
using UnityEngine;
using IBM.Watson.TextToSpeech.V1;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Utilities;
using UnityEngine.UI;
using IBM.Watson.SpeechToText.V1;
using IBM.Cloud.SDK.DataTypes;


public class WatsonTextToSpeech : MonoBehaviour
{
    #region PLEASE SET THESE VARIABLES IN THE INSPECTOR
    [Header("Text to Speech")]
    [SerializeField]
    [Tooltip("The service URL (optional). This defaults to \"https://stream.watsonplatform.net/text-to-speech/api\"")]
    private string TextToSpeechURL;
    [Header("IAM Authentication")]
    [Tooltip("The IAM apikey.")]
    [SerializeField]
    private string TextToSpeechIamApikey;
    [Tooltip("The IAM url used to authenticate the apikey (optional). This defaults to \"https://iam.bluemix.net/identity/token\".")]
    [SerializeField]
    private string TextToSpeechIamUrl;

    [Space(10)]
    [Tooltip("The service URL (optional). This defaults to \"https://stream.watsonplatform.net/speech-to-text/api\"")]
    [SerializeField]
    private string _serviceUrl;
    [Tooltip("Text field to display the results of streaming.")]
    public Text ResultsField;
    [Header("IAM Authentication")]
    [Tooltip("The IAM apikey.")]
    [SerializeField]
    private string _iamApikey;

    [Header("Parameters")]
    // https://www.ibm.com/watson/developercloud/speech-to-text/api/v1/curl.html?curl#get-model
    [Tooltip("The Model to use. This defaults to en-US_BroadbandModel")]
    [SerializeField]
    private string _recognizeModel;
    #endregion

    private int _recordingRoutine = 0;
    private string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingBufferSize = 1;
    private int _recordingHZ = 22050;

    private SpeechToTextService _service;

    private TextToSpeechService textToSpeech;


    private bool storytold = false;
    private string firstUserAnswer;
    private string secondUserAnswer;
    private string thirdUserAnswer;

    string wordsSaid;

    private string fStory;

    private string intro = "Hello storytelller. I am Sahara. An African proverb says: Until the Lioness can tell her story, the hunter will always be the hero. Shall we tell our own stories?";

    private string TiyesStory = "There was a girl called Teeyay who loved to travel but she was afraid of something. Let's put together her story. In one word, tell me her greatest fear.";

    void Start()
    {
        LogSystem.InstallDefaultReactors();
        
        Runnable.Run(CredentialCheck());
        LogSystem.InstallDefaultReactors();
        Runnable.Run(CreateService());

    }


    public IEnumerator CredentialCheck()
    {
        Credentials credentials = null;
        //Authenticate using iamApikey
        TokenOptions tokenOptions = new TokenOptions()
        {
            IamApiKey = TextToSpeechIamApikey
        };

        credentials = new Credentials(tokenOptions, TextToSpeechURL);

        while (!credentials.HasIamTokenData())
            yield return null;

        textToSpeech = new TextToSpeechService(credentials);

        StartStory();

    }

    public IEnumerator CallTextToSpeech(string outputText)
    {
        byte[] synthesizeResponse = null;
        AudioClip clip = null;
        textToSpeech.Synthesize(
            callback: (DetailedResponse<byte[]> response, IBMError error) =>
            {
                synthesizeResponse = response.Result;
                clip = WaveFile.ParseWAV("myClip", synthesizeResponse);
                PlayClip(clip);

            },
            text: outputText,
            voice: "en-US_AllisonVoice",
            accept: "audio/wav"
        );

        while (synthesizeResponse == null)
            yield return null;

        yield return new WaitForSeconds(clip.length);
    }

    private void PlayClip(AudioClip clip)
    {
        if (Application.isPlaying && clip != null)
        {
            GameObject audioObject = new GameObject("AudioObject");
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.spatialBlend = 0.0f;
            source.loop = false;
            source.clip = clip;
            source.Play();

            GameObject.Destroy(audioObject, clip.length);
        }

    }

    public void StartStory ()
    {
      
        Runnable.Run(CallTextToSpeech(intro));
        //}
        //else {
        StartCoroutine(ListenForYes());

        //}
    }



    public IEnumerator TellStory()
    {
        yield return new WaitForSeconds(5);
        Runnable.Run(CallTextToSpeech(TiyesStory));
        yield return new WaitForSeconds(10);
        StartCoroutine(ListenToAnswer());

    }

    public IEnumerator ListenToAnswer() {
       
        _service.StartListening(OnRecognize, OnRecognizeSpeaker);
        StartRecording();
        Debug.Log("we are listening");
        yield return new WaitForSeconds(5);
       
        Debug.Log(wordsSaid);
        firstUserAnswer = wordsSaid;
        StopRecording();
        _service.StopListening();
        StartCoroutine(AskFirstQ());

    }

    public IEnumerator AskFirstQ()
    {
        yield return new WaitForSeconds(5);
        Runnable.Run(CallTextToSpeech("Thank you. Now tell me where she wants to go."));
        yield return new WaitForSeconds(5);
        StartCoroutine(ListenToSecondAnswer());

    }

    public IEnumerator ListenToSecondAnswer()
    {
        _service.StartListening(OnRecognize, OnRecognizeSpeaker);
        StartRecording();
        Debug.Log("we are listening");
        yield return new WaitForSeconds(5);
        Debug.Log(wordsSaid);
        secondUserAnswer = wordsSaid;
        StopRecording();
        _service.StopListening();
        StartCoroutine(AskSecondQ());

    }

    public IEnumerator AskSecondQ()
    {
        yield return new WaitForSeconds(5);
        Runnable.Run(CallTextToSpeech("Great. Can you tell me what she hopes to find?"));
        yield return new WaitForSeconds(5);
        StartCoroutine(ListenToThirdAnswer());

    }

    public IEnumerator ListenToThirdAnswer()
    {
        _service.StartListening(OnRecognize, OnRecognizeSpeaker);
        StartRecording();
        Debug.Log("we are listening");
        yield return new WaitForSeconds(5);
        Debug.Log(wordsSaid);
        thirdUserAnswer = wordsSaid;
        StopRecording();
        _service.StopListening();
        TypingStory();

    }

    public void TypingStory() {

        fStory = "Once upon a time, there was a girl called Tiye. Tiye loved to travel the world and she was very brave. She loved to see knew places. But Teeyay had a fear. She was afraid of " + firstUserAnswer + ". " + "She wanted to travel to " + secondUserAnswer + " to help her be brave again. There, she found " + thirdUserAnswer;
        storytold = true;
        Runnable.Run(CallTextToSpeech(fStory));
    }

    public IEnumerator ListenForYes () {

        yield return new WaitForSeconds(15);
        Debug.Log("listening to you Ari!");

       //Active = true;
        StartRecording();
        yield return new WaitForSeconds(5);
        Debug.Log("You want this Ari " + wordsSaid);
        string yes = "yes";

        bool b = wordsSaid.Contains(yes);

        if (b)
        {

          Runnable.Run(CallTextToSpeech("Let us begin."));
           StopRecording();
            _service.StopListening();
            StartCoroutine(TellStory());

        }

        else 
        {
            Runnable.Run(CallTextToSpeech("Do not be afriad to know the truth. Have a nice day."));
            StopRecording();
            _service.StopListening();
        }

    }

    private IEnumerator CreateService()
    {
        if (string.IsNullOrEmpty(_iamApikey))
        {
            throw new IBMException("Plesae provide IAM ApiKey for the service.");
        }

        //  Create credential and instantiate service
        Credentials credentials = null;

        //  Authenticate using iamApikey
        TokenOptions tokenOptions = new TokenOptions()
        {
            IamApiKey = _iamApikey
        };

        credentials = new Credentials(tokenOptions, _serviceUrl);

        //  Wait for tokendata
        while (!credentials.HasIamTokenData())
            yield return null;

        _service = new SpeechToTextService(credentials);
        _service.StreamMultipart = true;

        Active = true;
        //StartRecording();
    }

    public bool Active
    {
        get { return _service.IsListening; }
        set
        {
            if (value && !_service.IsListening)
            {
                _service.RecognizeModel = (string.IsNullOrEmpty(_recognizeModel) ? "en-US_BroadbandModel" : _recognizeModel);
                _service.DetectSilence = true;
                _service.EnableWordConfidence = true;
                _service.EnableTimestamps = true;
                _service.SilenceThreshold = 0.01f;
                _service.MaxAlternatives = 1;
                _service.EnableInterimResults = true;
                _service.OnError = OnError;
                _service.InactivityTimeout = -1;
                _service.ProfanityFilter = false;
                _service.SmartFormatting = true;
                _service.SpeakerLabels = false;
                _service.WordAlternativesThreshold = null;
                _service.StartListening(OnRecognize, OnRecognizeSpeaker);
            }
            else if (!value && _service.IsListening)
            {
                _service.StopListening();
            }
        }
    }

    private void StartRecording()
    {
        if (_recordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
            _recordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    private void StopRecording()
    {
        if (_recordingRoutine != 0)
        {
            Microphone.End(_microphoneID);
            Runnable.Stop(_recordingRoutine);
            _recordingRoutine = 0;
        }
    }

    private void OnError(string error)
    {
        Active = false;

        Log.Debug("ExampleStreaming.OnError()", "Error! {0}", error);
    }

    private IEnumerator RecordingHandler()
    {
        Log.Debug("ExampleStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
        _recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
        yield return null;      // let _recordingRoutine get set..

        if (_recording == null)
        {
            StopRecording();
            yield break;
        }

        bool bFirstBlock = true;
        int midPoint = _recording.samples / 2;
        float[] samples = null;

        while (_recordingRoutine != 0 && _recording != null)
        {
            int writePos = Microphone.GetPosition(_microphoneID);
            if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
            {
                Log.Error("ExampleStreaming.RecordingHandler()", "Microphone disconnected.");

                StopRecording();
                yield break;
            }

            if ((bFirstBlock && writePos >= midPoint)
              || (!bFirstBlock && writePos < midPoint))
            {
                // front block is recorded, make a RecordClip and pass it onto our callback.
                samples = new float[midPoint];
                _recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
                record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
                record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
                record.Clip.SetData(samples, 0);

                _service.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                // calculate the number of samples remaining until we ready for a block of audio, 
                // and wait that amount of time it will take to record.
                int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)_recordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }

        }

        yield break;
    }

    public void OnRecognize(SpeechRecognitionEvent result)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    wordsSaid = alt.transcript;
                    Log.Debug("ExampleStreaming.OnRecognize()", wordsSaid);
                    ResultsField.text = wordsSaid;

                }

                if (res.keywords_result != null && res.keywords_result.keyword != null)
                {
                    foreach (var keyword in res.keywords_result.keyword)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "keyword: {0}, confidence: {1}, start time: {2}, end time: {3}", keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
                    }
                }

                if (res.word_alternatives != null)
                {
                    foreach (var wordAlternative in res.word_alternatives)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "Word alternatives found. Start time: {0} | EndTime: {1}", wordAlternative.start_time, wordAlternative.end_time);
                        foreach (var alternative in wordAlternative.alternatives)
                            Log.Debug("ExampleStreaming.OnRecognize()", "\t word: {0} | confidence: {1}", alternative.word, alternative.confidence);
                    }
                }
            }
        }
    }

    public void OnRecognizeSpeaker(SpeakerRecognitionEvent result)
    {

        if (result != null)
        {
            foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
            {
                Log.Debug("ExampleStreaming.OnRecognizeSpeaker()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
            }
        }
    }



 
}