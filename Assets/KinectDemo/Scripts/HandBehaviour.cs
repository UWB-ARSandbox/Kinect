﻿/* HandBehaviour
 * 
 * This script is present in the Hand prefab, and manages grabbing and releasing of objects.
 *  The hand will be created automatically, and it is not necessary to put one in the scene.
 *  The prefab has a number of colors for when the hand is open, closed, pointing (lasso), or
 *  at rest (or unrecognized). It also includes two shaders-- one for the highlight of an object,
 *  applied to objects being hovered on. Another is the standard renderer-- what the set the
 *  object back to.
 * If the highlighted object contains a "Spawnable" script, this script creates a new Prefab
 *  with a name identical to that highlighted object's name. So, if the highlighted object's name
 *  is Cube, it will create a new Resources/Prefabs/Cube prefab.
 */

using UnityEngine;
using UnityEngine.UI;
using Windows.Kinect;

public class HandBehaviour : MonoBehaviour
{
    private bool mousePointerMode;

    // Kinect Body Objects
    private BodySourceManager bodyManager;
    private Body trackedBody;
    private GameObject trackedBodyObject;

    // Held and touched object references
    private GameObject highlightedObject = null;
    private GameObject selectedObject = null;

    // Position multiplier
    public float multiplier = 10f;

    // Reused variables
    private Vector3 position;
    private Vector3 selectedPosition;
    private CameraSpacePoint pos; // Kinect's implied position
    private string otherHand;
    private Windows.Kinect.Vector4 rot; // Kinect's implied rotation
    private Windows.Kinect.Vector4 tempRot;

    // Hand Colors
    public Color restColor;
    public Color openColor;
    public Color closedColor;
    public Color pointColor;
    private Renderer handRenderer;

    // Held object shaders
    public Shader standardShader;
    public Shader outlineShader;

    public GameObject mouseLeftCursor;
    public GameObject mouseRightCursor;
    private Camera mainCamera;
    private Minimap myMiniMap;
    // Left/Right Hand enum
    public enum hand
    {
        right,
        left
    }
    private hand thisHand;

    // Use this for specific initialization
    public void init(Body bodyToTrack, BodySourceManager bodySourceManager, GameObject trackedBodyObj, hand handType)
    {
        trackedBody = bodyToTrack;
        bodyManager = bodySourceManager;
        thisHand = handType;
        trackedBodyObject = trackedBodyObj;
    }

    // GameObject initlization
    void Start()
    {
        myMiniMap = GameObject.FindObjectOfType<Minimap>();
        mousePointerMode = false;
        position = new Vector3();
        selectedPosition = new Vector3();
        if (handRenderer == null)
            handRenderer = gameObject.GetComponent<Renderer>();
    }

    //return the hand position
    public CameraSpacePoint getHandPos()
    {
        return pos;
    }

    //return the hand state
    public HandState getHandState()
    {
        if (thisHand == hand.left)
        {
            return trackedBody.HandLeftState;
        }
        else
        {
            return trackedBody.HandRightState;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (trackedBody.IsTracked)
        {
            // Get joint position
            if (thisHand == hand.left)
            {
                pos = trackedBody.Joints[JointType.HandLeft].Position;
                otherHand = "HandRight";
                //rot = trackedBody.JointOrientations[JointType.WristLeft].Orientation;
            }
            else
            {
                pos = trackedBody.Joints[JointType.HandRight].Position;
                otherHand = "HandLeft";
                //rot = trackedBody.JointOrientations[JointType.WristRight].Orientation;
            }

            if (GameObject.Find("Position_UI"))
            {
                mousePointerMode = true;
            }
            else
            {
                mousePointerMode = false;
            }

            if (!mousePointerMode)
            {
                position = trackedBodyObject.transform.TransformPoint(new Vector3(pos.X * 10f, pos.Y * 10f, pos.Z * 10f));
            }

            if ( mainCamera == null)
            {
                mainCamera = GameObject.Find("KinectReference").GetComponent<Camera>();
            }           

            gameObject.transform.position = position;
            //gameObject.transform.localPosition = position;

            // Move currently selected object
            if (selectedObject != null)
            {
                // Set currently selected object's position
                selectedObject.transform.position = transform.position;


            }

            if (!mousePointerMode)
            {
                // Check for hand gestures
                if (thisHand == hand.left)
                {
                    if (trackedBody.HandLeftState == HandState.Open)
                    {
                        releaseObject();
                        handRenderer.material.color = openColor;
                    }
                    else if (trackedBody.HandLeftState == HandState.Closed)
                    {
                        grabObject();
                        handRenderer.material.color = closedColor;
                    }
                    else if (trackedBody.HandLeftState == HandState.Lasso)
                    {
                        handRenderer.material.color = pointColor;
                    }
                    else
                    {
                        handRenderer.material.color = restColor;
                    }
                }
                else // Right hand
                {
                    if (trackedBody.HandRightState == HandState.Open)
                    {
                        releaseObject();
                        handRenderer.material.color = openColor;
                    }
                    else if (trackedBody.HandRightState == HandState.Closed)
                    {
                        grabObject();
                        handRenderer.material.color = closedColor;
                    }
                    else if (trackedBody.HandRightState == HandState.Lasso)
                    {
                        handRenderer.material.color = pointColor;
                    }
                    else
                    {
                        handRenderer.material.color = restColor;
                    }
                }
            }
            else
            {
                // Check for hand gestures specific to only the mouse pinter mode
                if (thisHand == hand.left)
                {
                    if (mouseLeftCursor == null) mouseLeftCursor = GameObject.Find("MouseCursorL");
                    if (trackedBody.HandLeftState == HandState.Open)
                    {
                        if (myMiniMap) myMiniMap.toggleLeftHandGrab(false);
                        //releaseObject();
                        handRenderer.material.color = openColor;
                        if (mouseLeftCursor != null)
                        {
                            var color = openColor;
                            color.a = 1f;
                            mouseLeftCursor.GetComponent<Image>().color = color;
                        }
                    }
                    else if (trackedBody.HandLeftState == HandState.Closed)
                    {
                        if (myMiniMap) myMiniMap.toggleLeftHandGrab(true);
                        //grabObject();
                        handRenderer.material.color = closedColor;
                        if (mouseLeftCursor != null)
                        {
                            var color = closedColor;
                            color.a = 1f;
                            mouseLeftCursor.GetComponent<Image>().color = color;
                        }
                    }
                    else
                    {
                        handRenderer.material.color = restColor;
                        if (mouseLeftCursor != null)
                        {
                            var color = restColor;
                            color.a = 1f;
                            mouseLeftCursor.GetComponent<Image>().color = color;
                        }
                    }
                    if (mouseLeftCursor == null) return;
                    if (mainCamera == null) return;


                    Vector3 newPos = new Vector3((Screen.width/2 )+ (pos.X * 1000),( Screen.height/2 )+ (pos.Y * 1000), pos.Z);

                    if (newPos.x > 0 && newPos.x < mainCamera.pixelWidth && newPos.y > 0 && newPos.y < mainCamera.pixelWidth)
                    {
                        mouseLeftCursor.transform.position = newPos;
                    }
                    else
                    {
                        Debug.Log("mouse if off screen position");
                    }
                }
                else
                {
                    if (mouseRightCursor == null) mouseRightCursor = GameObject.Find("MouseCursorR");
                    if (trackedBody.HandRightState == HandState.Open)
                    {
                        if (myMiniMap) myMiniMap.toggleRightHandGrab(false);
                        //releaseObject();
                        handRenderer.material.color = openColor;
                        if (mouseRightCursor != null)
                        {
                            var color = openColor;
                            color.a = 1f;
                            mouseRightCursor.GetComponent<Image>().color = color;
                        }
                    }
                    else if (trackedBody.HandRightState == HandState.Closed)
                    {
                        if (myMiniMap) myMiniMap.toggleRightHandGrab(true);
                        //grabObject();
                        handRenderer.material.color = closedColor;
                        if (mouseRightCursor != null)
                        {
                            var color = closedColor;
                            color.a = 1f;
                            mouseRightCursor.GetComponent<Image>().color = color;
                        }
                    }
                    else
                    {
                        handRenderer.material.color = restColor;
                        if (mouseRightCursor != null)
                        {
                            var color = restColor;
                            color.a = 1f;
                            mouseRightCursor.GetComponent<Image>().color = color;
                        }
                    }
                    if (mouseRightCursor == null) return;
                    if (mainCamera == null) return;

                    Vector3 newPos = new Vector3((Screen.width / 2) + (pos.X * 1000), (Screen.height / 2 )+ (pos.Y * 1000), pos.Z);

                    if (newPos.x > 0 && newPos.x < mainCamera.pixelWidth && newPos.y > 0 && newPos.y < mainCamera.pixelWidth)
                    {
                        mouseRightCursor.transform.position = newPos;
                    }
                    else
                    {
                        Debug.Log("mouse if off screen position");
                    }
                }
            }
        }
        else
        {
            // If our tracked body doesn't exist, neither should this hand
            Destroy(this.gameObject);
        }
    }

    // turn the mouse pointer mode off or no
    public void turnOnMousePointerMode(bool isOn)
    {
        mousePointerMode = isOn;
    }

    // If we have a selected objecct, and we let go, make sure to tell the
    //  network what it's new position is.
    private void releaseObject()
    {
        //Debug.Log("released");
        if (selectedObject != null)
        {
            selectedObject.GetPhotonView().TransferOwnership(1);
        }
        selectedObject = null;


    }

    // If we have a highlighted object, make it our selected object. If that object is
    //  a Spawnable object, create a prefab using it's name, and tell the network.
    private void grabObject()
    {
        if (selectedObject == null && highlightedObject != null)
        {
            /*if ((highlightedObject.GetComponent("Spawnable") as Spawnable) != null)
            {
                // Create a new copy of the selected object
                GameObject newObj = GameObject.Instantiate(Resources.Load("Prefabs/" + highlightedObject.name)) as GameObject;
                // Add it to the list to be sent over the network
            
                // Set our selected object equal to the newly created object
                selectedObject = newObj;
            }*/

            if (!highlightedObject.GetPhotonView().isMine)
            {
                highlightedObject.GetPhotonView().RequestOwnership();
            }
            selectedObject = highlightedObject;


            removeHighlight();
        }
    }

    // If we're not moving an object, and we touch one, highlight it.
    void OnTriggerEnter(Collider other)
    {
        // If we're not grabbing, and we've entered a different object..
        if (selectedObject == null && other.gameObject != highlightedObject)
        {
            removeHighlight();
            highlightObject(other.gameObject);
        }
    }

    // If we leave our highlighted object, unhighlight it.
    void OnTriggerExit(Collider other)
    {
        // If we've exited our currently highlighted object..
        if (other.gameObject == highlightedObject)
        {
            removeHighlight();
        }
    }

    // Change the renderer of our highlighted object to have an outline.
    private void highlightObject(GameObject obj)
    {
        // Outline this object using the shader
        highlightedObject = obj;
        highlightedObject.GetComponent<Renderer>().material.shader = outlineShader;
        float outlineWidth = .01f * obj.GetComponent<Renderer>().bounds.size.magnitude;
        highlightedObject.GetComponent<Renderer>().material.SetFloat("_Outline", outlineWidth);
    }

    // Change the renderer of our highlighted object back to standard, and
    //  set our highlighted object to 'none'.
    private void removeHighlight()
    {
        if (highlightedObject != null)
        {
            // Remove the object's shader outline
            highlightedObject.GetComponent<Renderer>().material.shader = standardShader;
            highlightedObject = null;
        }
    }
}
