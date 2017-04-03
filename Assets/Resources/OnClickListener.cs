using UnityEngine;
using System.Collections;
using Photon;

public class OnClickListener : PunBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnMouseDown()
    {
        Debug.Log("OnMouseDown called");
        if (!gameObject.GetPhotonView().isMine)
        {
            gameObject.GetPhotonView().RequestOwnership();
        }
    }
}
