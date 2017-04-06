using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManipulator : MonoBehaviour {

    public Camera myPrimaryCamera;
    public GameObject myButton;
    public GameObject myCube;

    // Use this for initialization
    void Start ()
    {
        myButton = GameObject.Find("Minimap Button");
        if (myButton != null)
        {
            Vector3 screenPos = myPrimaryCamera.ScreenToViewportPoint(myButton.transform.position);
            Debug.Log("This is the screeen space position for MiniMap Button: " + screenPos);
            Vector3 screenPosq = myPrimaryCamera.ScreenToWorldPoint(myButton.transform.position);
            Debug.Log("This is the screeen space position for MiniMap Button: " + screenPosq);
            Vector3 screenPosa = myPrimaryCamera.ViewportToScreenPoint(myButton.transform.position);
            Debug.Log("This is the screeen space position for MiniMap Button: " + screenPosa);
            //Vector3 screenPosy = myPrimaryCamera.WorldToScreenPoint(myButton.transform.position);
            //Debug.Log("This is the screeen space position for MiniMap Button: " + screenPosy);
        }


        myCube = GameObject.Find("Cube (2)");
        if (myCube != null)
        {
            Vector3 screenPos = myPrimaryCamera.ScreenToViewportPoint(myCube.transform.position);
            Debug.Log("This is the screeen space position for Cube: " + screenPos);
            Vector3 screenPose = myPrimaryCamera.ScreenToViewportPoint(myCube.transform.position);
            Debug.Log("This is the screeen space position for Cube: " + screenPose);
            Vector3 screenPoss = myPrimaryCamera.ViewportToScreenPoint(myCube.transform.position);
            Debug.Log("This is the screeen space position for Cube: " + screenPoss);
            //Vector3 screenPosu = myPrimaryCamera.ScreenToWorldPoint(myCube.transform.position);
            //Debug.Log("This is the screeen space position for Cube: " + screenPosu);
        }
    }

    // Update is called once per frame
    void Update ()
    {
        Vector3 boxPos = myPrimaryCamera.WorldToScreenPoint(myCube.transform.position);


        //myButton.transform.position = myPrimaryCamera.WorldToScreenPoint(myCube.transform.position);

        //Debug.Log("my pos" + myPrimaryCamera.WorldToScreenPoint(myCube.transform.position));

        //Debug.Log("This is the screeen space position for MiniMap Button: " + buttonPos);
        //Debug.Log("This is the screeen space position for Cube: " + boxPos);
        if (boxPos.x < (myButton.transform.position.x + 100) && boxPos.x > (myButton.transform.position.x - 100) && boxPos.y < (myButton.transform.position.y + 100) && boxPos.y > (myButton.transform.position.y - 100))
        {
            myButton.gameObject.GetComponent<Image>().color = Color.red;
        }
        else
        {
            myButton.gameObject.GetComponent<Image>().color = Color.blue;
        }

    }
}
