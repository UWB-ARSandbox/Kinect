using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Windows.Kinect;
using UnityEngine.Windows.Speech;

public class VoiceScript : MonoBehaviour {

    private BodySourceManager bodysource;
    Windows.Kinect.AudioSource KinectAudio;

    public Minimap MinimapScript;

    KeywordRecognizer keywordRecognizer;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    // Use this for initialization
    void Start () {
        bodysource = GameObject.Find("BodySourceManager").GetComponent<BodySourceManager>();

        //if (bodysource.getSensor().AudioSource != null)
        //{
        //    KinectAudio = bodysource.getSensor().AudioSource;
        //}

        keywords.Add("create", () =>
        {
            Debug.Log("Create Triggered");
        });
        keywords.Add("position", () =>
        {
            MinimapScript.Open();
            Debug.Log("Position Triggered");
        });
        keywords.Add("close", () =>
        {
            MinimapScript.Close();
            Debug.Log("Position Triggered");
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

        string[] mList = Microphone.devices.ToArray();

        for (int i = 0;i < mList.Length; i++)
        {
            Debug.Log(mList[i]);
        }
    }

    // Update is called once per frame
    void Update () {
        //if (KinectAudio != null)
        //{
        //    //Microphone.Start();
        //}
        //else
        //{
        //    KinectAudio = bodysource.getSensor().AudioSource;
        //    if (KinectAudio == null)
        //    {
        //        Debug.LogError("Kinect Audio Source returned null");
        //    }
        //}
    }

}
