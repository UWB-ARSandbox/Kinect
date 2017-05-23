using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Windows.Kinect;
using UnityEngine.Windows.Speech;

public class VoiceScript_Position : MonoBehaviour
{

    public Minimap MinimapScript;

    KeywordRecognizer keywordRecognizer;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    // Use this for initialization
    void Start()
    {

        keywords.Add("close", () =>
        {
            MinimapScript.Close();
        });

        keywords.Add("return", () =>
        {
            MinimapScript.Close();
        });

        keywords.Add("swap one", () =>
        {
            MinimapScript.Swap(1);
        });

        keywords.Add("swap two", () =>
        {
            MinimapScript.Swap(2);
        });

        keywords.Add("lock camera", () =>
        {
            if(!MinimapScript.getCameraLock())
                MinimapScript.LockCameraToggle();
        });

        keywords.Add("unlock camera", () =>
        {
            if (MinimapScript.getCameraLock())
                MinimapScript.LockCameraToggle();
        });

        keywords.Add("front camera", () =>
        {
            if (MinimapScript.getCameraLock())
                MinimapScript.CameraPosition(1);
        });

        keywords.Add("third person camera", () =>
        {
            if (MinimapScript.getCameraLock())
                MinimapScript.CameraPosition(2);
        });

        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;

        keywordRecognizer.Start();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;
        // if the keyword recognized is in our dictionary, call that Action.
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

}
