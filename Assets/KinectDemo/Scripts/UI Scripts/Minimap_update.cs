using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap_update : MonoBehaviour {

    //init minimap camera and kinect camera references
    Camera MinimapCamera;
    Camera Kinect;

    // Use this for initialization
    void Start () {

        //go through all cameras and get minimap camera and kinect camera
        foreach (Camera c in Camera.allCameras)
        {
            if (c.gameObject.name == "Minimap Camera")
                MinimapCamera = c;
            if (c.gameObject.name == "KinectReference")
                Kinect = c;
        }
    }
	
	// Update is called once per frame
	void Update () {
        //update the position of the minimap camera
        MinimapCamera.transform.position = Kinect.transform.position + new Vector3(0, 10, 0);
    }
}
