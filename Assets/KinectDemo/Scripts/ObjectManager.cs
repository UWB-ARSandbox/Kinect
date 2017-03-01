using UnityEngine;
using System.Collections;
using Photon;
using Random = System.Random;

namespace UWBNetworkingPackage.HoloLensDemo
{
    /// <summary>
    /// Script that allows a HoloLens to instantiate/move sphere and cube objects
    /// (located in the Photon Resources folder)
    /// Also instantiates and tracks a sphere that acts as the HoloLens avatar
    /// </summary>
    public class ObjectManager : PunBehaviour
    {
        [Tooltip("The cube object to be instantiated (must exist in the Photon Resources folder")]
        public GameObject Cube;

        [Tooltip("The sphere object to be instantiated (must exist in the Photon Resources folder")]
        public GameObject Sphere;

        [Tooltip("The Kinect head object to be instantiated (must exist in the Photon Resources folder")]
        public GameObject kinectHead;

        [Tooltip("The Kinect hand object to be instantiated (must exist in the Photon Resources folder")]
        public GameObject kinectHandLeft;

        [Tooltip("The Kinect hand object to be instantiated (must exist in the Photon Resources folder")]
        public GameObject kinectHandRight;

        [Tooltip("Distance, in meters, to offset the cursor from the collision point.")]
        public float DistanceFromCollision = 1f;

        private readonly Random _rand = new Random();           // For generating random numbers

        private readonly ArrayList _cubes = new ArrayList();    // Stores all cube instances
        private readonly ArrayList _spheres = new ArrayList();  // Stores all sphere instances
       

        /// <summary>
        /// On joined room, instantiates the HoloLens head avatar object (specified by user)
        /// </summary>
        public override void OnJoinedRoom()
        {
            PhotonNetwork.Instantiate(kinectHead.name, Vector3.zero, Quaternion.identity, 0);
            PhotonNetwork.Instantiate(kinectHandLeft.name, Vector3.zero, Quaternion.identity, 0);
            PhotonNetwork.Instantiate(kinectHandRight.name, Vector3.zero, Quaternion.identity, 0);
        }

        /// <summary>
        /// Instantiates the cube object in the origin of the game space
        /// </summary>
        public void CreateCube()
        {
            

            
            _cubes.Add(PhotonNetwork.Instantiate(Cube.name, Vector3.zero, Quaternion.identity, 0));
        }

        /// <summary>
        /// Instantiates teh sphere object in the origin of the game space
        /// </summary>
        public void CreateSphere()
        {
           
            _spheres.Add(PhotonNetwork.Instantiate(Sphere.name, Vector3.zero, Quaternion.identity, 0));
        }

        /// <summary>
        /// Randomly chooses once of the stored sphere instances and moves it in a random direction 
        /// along the x and y axis
        /// </summary>
        public void MoveSphere()
        {
            int index = _rand.Next(0, _spheres.Count);
            GameObject sphereToMove = (GameObject) _spheres[index];
            int moveDirection = _rand.Next(0, 4);
            MoveObject(sphereToMove, moveDirection);
          
        }
        /// <summary>
        /// Randomly chooses once of the stored cube instances and moves it in a random direction 
        /// along the x and y axis
        /// </summary>
        public void MoveCube()
        {
            int index = _rand.Next(0, _cubes.Count);
            GameObject cubeToMove = (GameObject)_cubes[index];
            int moveDirection = _rand.Next(0, 4);
            MoveObject(cubeToMove, moveDirection);
        }


        /// <summary>
        /// Private method that determines the direction to move the given object along the 
        /// x and y axis
        /// If the given direction is 0 - move up, 1 - move down, 2 - move right, 3 - move left
        /// </summary>
        /// <param name="obj">The object to move</param>
        /// <param name="direction">Specifies the direction to move (default is left)</param>
        private void MoveObject(GameObject obj, int direction)
        {
            if (direction == 0)
            {
                obj.transform.position += Vector3.up;
            }
            else if (direction == 1)
            {
                obj.transform.position += Vector3.down;
            }
            else if (direction == 2)
            {
                obj.transform.position += Vector3.right;
            }
            else
            {
                obj.transform.position += Vector3.left;
            }
        }
    }
}