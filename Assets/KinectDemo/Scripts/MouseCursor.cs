using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using Windows.Kinect;

public class MouseCursor : MonoBehaviour
{
    public Body trackedBody;                    // body instance for this cursor
    public GameObject mouseLeftCursor;          // left cursor object
    public GameObject mouseRightCursor;         // right cursor object 
    public Camera mainCamera;                   // main Camera in scene
    public Minimap myMiniMap;                   // the mini map script
    public bool isActive;                       // should the mouse cursors update
    public List<Button> InteractableButtons;    // all the buttons for cursor interaction

    public GameObject Timer;
    private int SelectedButton;
    private int CountDown;
    private int SelectedHand;

    void Start()
    {
        isActive = false;
        if (!mouseLeftCursor) mouseLeftCursor = GameObject.Find("MouseCursorL");
        if (!mouseRightCursor) mouseRightCursor = GameObject.Find("MouseCursorR");
        if (!mainCamera) mainCamera = GameObject.Find("KinectReference").GetComponent<Camera>();
        if (!myMiniMap) myMiniMap = FindObjectOfType<Minimap>();
    }

    void Update()
    {
        if (!isActive) return;
        if (trackedBody == null) return;
        if (!mainCamera) return;
        if (!trackedBody.IsTracked)
        {
            trackedBody = null;
            return;
        }

        // make sure left cursor exists
        if (mouseLeftCursor)
        {
            // check left hand state and set color accordingly
            if (trackedBody.HandLeftState == HandState.Open)
            {
                if (myMiniMap) myMiniMap.toggleLeftHandGrab(false);

                var color = Color.green;
                color.a = 1f;
                mouseLeftCursor.GetComponent<Image>().color = color;
            }
            else if (trackedBody.HandLeftState == HandState.Closed)
            {
                if (myMiniMap) myMiniMap.toggleLeftHandGrab(true);

                var color = Color.red;
                color.a = 1f;
                mouseLeftCursor.GetComponent<Image>().color = color;
            }
            else
            {
                var color = Color.white;
                color.a = 1f;
                mouseLeftCursor.GetComponent<Image>().color = color;
            }

            // get the left hand position and convert it to screen space
            var leftPos = trackedBody.Joints[JointType.HandLeft].Position;
            Vector3 newPosLeft = new Vector3((Screen.width / 2) + (leftPos.X * 1000), (Screen.height / 2) + (leftPos.Y * 1000), leftPos.Z);

            // if cursor is outside of bounds dont update its position
            if (newPosLeft.x > 0 && newPosLeft.x < mainCamera.pixelWidth && newPosLeft.y > 0 && newPosLeft.y < mainCamera.pixelWidth)
            {
                mouseLeftCursor.transform.position = newPosLeft;
            }
        }

        // make sure right cursor exists
        if (mouseRightCursor)
        {
            // check right hand state and set color accordingly
            if (trackedBody.HandRightState == HandState.Open)
            {
                if (myMiniMap) myMiniMap.toggleRightHandGrab(false);

                var color = Color.green;
                color.a = 1f;
                mouseRightCursor.GetComponent<Image>().color = color;

            }
            else if (trackedBody.HandRightState == HandState.Closed)
            {
                if (myMiniMap) myMiniMap.toggleRightHandGrab(true);

                var color = Color.red;
                color.a = 1f;
                mouseRightCursor.GetComponent<Image>().color = color;

            }
            else
            {
                var color = Color.white;
                color.a = 1f;
                mouseRightCursor.GetComponent<Image>().color = color;
            }

            // get the right hand position and convert it to screen space
            var rightPos = trackedBody.Joints[JointType.HandRight].Position;
            Vector3 newPosRight = new Vector3((Screen.width / 2) + (rightPos.X * 1000), (Screen.height / 2) + (rightPos.Y * 1000), rightPos.Z);

            // if cursor is outside of bounds dont update its position
            if (newPosRight.x > 0 && newPosRight.x < mainCamera.pixelWidth && newPosRight.y > 0 && newPosRight.y < mainCamera.pixelWidth)
            {
                mouseRightCursor.transform.position = newPosRight;
            }
        }

        var hand = mouseRightCursor;
        var Xval = hand.gameObject.transform.position.x;
        var Yval = hand.gameObject.transform.position.y;
        for (int i = 0; i < InteractableButtons.Count; i++)
        {
            if (!InteractableButtons[i].IsActive()) continue;
            //var T = InteractableButtons[i].gameObject.transform;
            var currW = InteractableButtons[i].gameObject.GetComponent<RectTransform>().rect.width;
            var currH = InteractableButtons[i].gameObject.GetComponent<RectTransform>().rect.height;
            var currPos = InteractableButtons[i].gameObject.transform.position;
            Debug.Log(currPos);
            Debug.Log(Xval + " " + Yval);

            if (Xval > (currPos.x - (currW/2)) && Xval < (currPos.x + (currW/2)) && Yval > (currPos.y - (currH / 2)) && Yval < (currPos.y + (currW/2)))
            {
                Timer.gameObject.SetActive(true);
                Timer.GetComponent<Animator>().Play("LoadingAnimation");
                Debug.Log("In Box");
                SelectedHand = 0;
                SelectedButton = i;
                Timer.gameObject.transform.position = currPos;
                InteractableButtons[i].gameObject.GetComponent<Image>().color = Color.green;
                CountDown--;
                break;
            }
            else if (i == SelectedButton && SelectedHand == 0)
            {
                Timer.gameObject.SetActive(false);
                Timer.GetComponent<Animator>().Stop();
                InteractableButtons[i].gameObject.GetComponent<Image>().color = Color.white;
                SelectedButton = -1;
                CountDown = 90;
            }
        }

        if (CountDown < 0)
        {
            Timer.gameObject.SetActive(false);
            CountDown = 90;
            InteractableButtons[SelectedButton].onClick.Invoke();
        }
    }

    public void setBody(Body bodyToTrack)
    {
        trackedBody = bodyToTrack;
    }

    public bool isBodyActive()
    {
        if (trackedBody == null)
            return false;
        else
            return true;
    }

    public void setActive(bool active)
    {
        isActive = active;
    }
}


