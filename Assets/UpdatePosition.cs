using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatePosition : MonoBehaviour {
    

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        var camera = GameObject.Find("VirtualCamera(Clone)");
        
        if (camera)
        {
            var pos = camera.transform.position;
            var rot = camera.transform.rotation;
            this.transform.position = pos;
            //Quaternion Rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
            this.transform.rotation = rot;

            Camera topCamera = GameObject.Find("Top-Down").GetComponent<Camera>();
            Quaternion topRotation = Quaternion.Euler(90, rot.y, 0);
            topCamera.transform.rotation = topRotation;
        }
    }
}
