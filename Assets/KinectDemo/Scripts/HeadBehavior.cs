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
using Photon;
using Windows.Kinect;


public class HeadBehavior : PunBehaviour
{
    [Tooltip("The Kinect head object to be instantiated (must exist in the Photon Resources folder")]
    public GameObject kinectHead;
    // Kinect Body Objects
    private BodySourceManager bodyManager;
    private Body trackedBody;
    private GameObject trackedBodyObject;

    // Position multiplier
    public float multiplier = 10f;

    // Reused variables
    private Vector3 position;
    private Vector3 selectedPosition;
    private CameraSpacePoint pos; // Kinect's implied position
    private string otherHand;
    private Windows.Kinect.Vector4 rot; // Kinect's implied rotation
    private Windows.Kinect.Vector4 tempRot;
    private bool instantiated = false;
  


    // Use this for specific initialization
    public void init(Body bodyToTrack, GameObject trackedBodyObj, BodySourceManager bodySourceManager)
    {
        trackedBody = bodyToTrack;
        bodyManager = bodySourceManager;
        trackedBodyObject = trackedBodyObj;
    }
 
    // GameObject initlization
    void Start()
    {
        position = new Vector3();
        selectedPosition = new Vector3();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (trackedBody.IsTracked)
        {
           
            pos = trackedBody.Joints[JointType.Head].Position;

            // Update hand position
            /*
            position.x = pos.X * multiplier;
            position.y = pos.Y * multiplier*0.75f;
            position.z = pos.Z * multiplier*2.25f;
            */
            position = trackedBodyObject.transform.TransformPoint(new Vector3(pos.X * 10f, pos.Y * 10f, pos.Z * 10f));
            gameObject.transform.localPosition = position;

        }
        else
        {
            // If our tracked body doesn't exist, neither should this hand
            Destroy(this.gameObject);
        }
    }


}
