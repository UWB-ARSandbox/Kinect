using UnityEngine;
using System.Collections;

public class ViveHeadTracking : MonoBehaviour {

    private GameObject cameraRig;

    // Use this for initialization
    void Start()
    {
        cameraRig = GameObject.FindGameObjectWithTag("CameraRig");
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraRig != null)
        {
            this.transform.position = cameraRig.transform.GetChild(2).position;
            this.transform.rotation = cameraRig.transform.GetChild(2).rotation;
        }
    }
}
