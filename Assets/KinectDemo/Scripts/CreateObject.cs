using UnityEngine;
using System.Collections;
using Photon;

/// <summary>
/// CreateObject object listens to keyboard actions. Once "c" is pressed, it will instantiate a cube; once "s" is pressed, it
/// will instantiate a sphere.
/// Note: You cannot press any button before you join in a room. You must have a cube prefab and sphere prefab in Photon
/// Unity Networking/Resources folder.
/// </summary>

namespace UWBNetworkingPackage.KinectDemo {

    public class CreateObject : PunBehaviour
    {

        public Camera main;

        void Update()
        {
            if (Input.GetKeyDown("c"))
            {
                PhotonNetwork.Instantiate("Cube", main.transform.position + main.transform.forward * 2, Quaternion.identity, 0);
            }

            if (Input.GetKeyDown("s"))
            {
                PhotonNetwork.Instantiate("Sphere", main.transform.position + main.transform.forward * 2, Quaternion.identity, 0);
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                PhotonNetwork.Instantiate("Trophy", main.transform.position + main.transform.forward * 2, Quaternion.identity, 0);
            }
        }
    }

}
