using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Windows.Kinect;
using UnityEngine.Windows.Speech;

public class VoiceScript_Main : MonoBehaviour {

    public Minimap MinimapScript;
    public UWBNetworkingPackage.KinectDemo.CreateObject CreateObject;

    KeywordRecognizer keywordRecognizer;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    // Use this for initialization
    void Start () {

        keywords.Add("create cube", () =>
        {
            CreateObject.CreateCube();
        });
        keywords.Add("create sphere", () =>
        {
            CreateObject.CreateSphere();
        });
        keywords.Add("create trophy", () =>
        {
            CreateObject.CreateTrophy();
        });

        keywords.Add("change position", () =>
        {
            MinimapScript.Open();
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
    void Update () {
        
    }

}
