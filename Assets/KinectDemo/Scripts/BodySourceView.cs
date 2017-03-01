/* BodySourceView
 * 
 * This is a modified version of the BodySourceView script provided with the Kinect
 *  SDK. This version does a couple of things differently: It will create a "hand"
 *  prefab (Resources/Hand) while creating joints for a body. It will also edit the
 *  position, rotation, and size of the spawned body to be relative to the object
 *  that this script is present on.
 * This script must be present in an object (probably an empty) in order to display
 *  bodies for kinect interaction. The cooresponding object should be positioned in
 *  such a way that the body is visible in the camera. The properties of this script
 *  includes a reference to the BodySourceManager. 
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using System;
using Photon;

public class BodySourceView : PunBehaviour
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;
    public GameObject LefthandPrefab;
    public GameObject RighthandPrefab;
    public GameObject headPrefab;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;

    public float bodyNetworkUpdateTime = 1f / 5f;
    private float timer;

    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };
    
    void Update () 
    {
        if (BodySourceManager == null)
        {
            return;
        }
        
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }
        
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }
        
        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
              }
                
            if(body.IsTracked)
            {
                trackedIds.Add (body.TrackingId);
            }
        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                Global.DeleteBody(_Bodies[trackingId]);
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }

        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
            
            if(body.IsTracked)
            {
                if(!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body);
                }
                
                RefreshBodyObject(body, _Bodies[body.TrackingId]);

                if (Global.clientStatus == Global.netStatus.Connected)
                {
                    timer += Time.deltaTime;
                    if (timer > bodyNetworkUpdateTime)
                    {
                        // Update body position to the network
                        Global.SendMovedBody(_Bodies[body.TrackingId]);
                        timer = 0f;
                    }
                }
            }
        }
    }
    
    private GameObject CreateBodyObject(Kinect.Body newBody)//ulong id)
    {
        GameObject body = new GameObject("Body:" + newBody.TrackingId);
        body.transform.position = transform.position;
        body.transform.rotation = transform.rotation;
        body.transform.localScale = transform.localScale;

        Vector3 jointScale = body.transform.localScale * 0.3f; // New

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.05f, 0.05f);

            // jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // Old
            jointObj.transform.localScale = jointScale; // New
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;

            Vector3 position = new Vector3(0,0,0);
            position.x = body.transform.localPosition.x*0.1f;
            position.y = body.transform.localPosition.y *0.1f;
            position.z = body.transform.localPosition.z *0.1f;

            if (jt == Kinect.JointType.HandLeft)
            {
                GameObject newLeft = PhotonNetwork.Instantiate(LefthandPrefab.name, position, Quaternion.identity, 0);
                newLeft.GetComponent<HandBehaviour>().init(newBody, _BodyManager, body, HandBehaviour.hand.left);
                newLeft.transform.localScale = body.transform.localScale;
                //newLeft.transform.parent = body.transform;
                newLeft.name = "HandLeft";
            }
            else if (jt == Kinect.JointType.HandRight)
            {
                GameObject newRight = PhotonNetwork.Instantiate(RighthandPrefab.name, position, Quaternion.identity, 0);
                newRight.GetComponent<HandBehaviour>().init(newBody, _BodyManager, body, HandBehaviour.hand.right);
                newRight.transform.localScale = body.transform.localScale;
               // newRight.transform.parent = body.transform;
                newRight.name = "HandRight";
            }
            else if (jt == Kinect.JointType.Head)
            {
                GameObject newHead = PhotonNetwork.Instantiate(headPrefab.name, position, Quaternion.identity, 0);
                newHead.GetComponent<HeadBehavior>().init(newBody, body, _BodyManager);
                newHead.transform.localScale = body.transform.localScale*2f ;
                //newHead.transform.parent = body.transform;
                newHead.name = "Head";
                
            }
        }

        // Add Body to HashMap
      //  Debug.Log(body.name.Substring(body.name.Length - 5, 5));
       Global.AddObject(Int32.Parse(body.name.Substring(body.name.Length - 5, 5)), body);
        timer = -2f; // Wait two aditional seconds just in case. ;)

        // Send Body to Hololens
        Global.SendNewBody(body);

        return body;
    }
    
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            
            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }
            
            Transform jointObj = bodyObject.transform.FindChild(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);

            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue)
            {
                Transform targetJointObj = bodyObject.transform.FindChild(targetJoint.Value.JointType.ToString()); // New

                lr.SetPosition(0, jointObj.position);
                lr.SetPosition(1, targetJointObj.position); // New
                //lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value)); // Old
                lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
            }
            else
            {
                lr.enabled = false;
            }
        }
    }
    
    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}
