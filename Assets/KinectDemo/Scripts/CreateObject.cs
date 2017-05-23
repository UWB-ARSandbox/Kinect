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
                CreateCube();
            }

            if (Input.GetKeyDown("s"))
            {
                CreateSphere();
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                CreateTrophy();
            }
        }

        public void CreateSphere()
        {
            PhotonNetwork.Instantiate("Sphere", main.transform.position + main.transform.forward * 3, Quaternion.identity, 0);
        }
        public void CreateCube()
        {
            PhotonNetwork.Instantiate("Cube", main.transform.position + main.transform.forward * 3, Quaternion.identity, 0);
        }
        public void CreateTrophy()
        {
            PhotonNetwork.Instantiate("Trophy", main.transform.position + main.transform.forward * 3, Quaternion.identity, 0);
        }
    }

}
