/* HandBehaviour
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
using Windows.Kinect;

public class HandBehaviour : MonoBehaviour
{
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
	void Start ()
    {
        position = new Vector3();
        selectedPosition = new Vector3();
        if(handRenderer == null)
            handRenderer = gameObject.GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(trackedBody.IsTracked)
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

            // Update hand position
            /*
            position.x = pos.X * multiplier;
            position.y = pos.Y * multiplier;// *0.25f;  
            position.z = pos.Z * multiplier;// *2.25f;
            */
            position = trackedBodyObject.transform.TransformPoint(new Vector3(pos.X*10f, pos.Y*10f, pos.Z*10f));
            //position.x = position.x * multiplier;
            //position.y = position.y * multiplier;// *0.25f;  
            //position.z = position.z * multiplier;// *2.25f;

            gameObject.transform.position = position;
            //gameObject.transform.localPosition = position;

            // Move currently selected object
            if (selectedObject != null)
            {
                // Set currently selected object's position
                selectedObject.transform.position = transform.position;

                // Set currently selected object's rotation
                Vector3 relative = transform.parent.FindChild(otherHand).position;

                selectedObject.transform.LookAt(relative);
            }

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
                else if(trackedBody.HandLeftState == HandState.Lasso)
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
            // If our tracked body doesn't exist, neither should this hand
            Destroy(this.gameObject);
        }
	}

    // If we have a selected objecct, and we let go, make sure to tell the
    //  network what it's new position is.
    private void releaseObject()
    {
        if(selectedObject != null)
        {
           
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
        if(selectedObject == null && other.gameObject != highlightedObject)
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
        if(highlightedObject != null)
        {
            // Remove the object's shader outline
            highlightedObject.GetComponent<Renderer>().material.shader = standardShader;
            highlightedObject = null;
        }
    }
}
