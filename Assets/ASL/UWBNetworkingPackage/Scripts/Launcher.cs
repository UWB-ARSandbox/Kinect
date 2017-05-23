using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine.SceneManagement;
using System.Net;

#if !UNITY_EDITOR && UNITY_WSA_10_0
using HoloToolkit.Unity;
#endif

namespace UWBNetworkingPackage
{
    /// <summary>
    /// Launcher is an abstract class that contains the base functionality needed for any device to 
    /// connect to a Game Room via Photon Unity Networking. 
    /// </summary>
    public abstract class Launcher : Photon.PunBehaviour
    {
#region Private Properties

        private static string _version = "1";   // Should be set to the current version of your application 

#endregion

#region Public Properties

        // Needed for Room Mesh sending
        [Tooltip("A port number for devices to communicate through. The port number should be the same for each set of projects that need to connect to each other and share the same Room Mesh.")]
        public int Port; 
       
        // Needed for Photon 
        [Tooltip("The name of the room that this project will attempt to connect to. This room must be created by a \"Master Client\".")]    
        public string RoomName;

        public AssetBundle networkAssets;

        #endregion

        /// <summary>
        /// Sets the Photon Network settings on awake
        /// </summary>
        public virtual void Awake()
        {
            PhotonNetwork.logLevel = PhotonLogLevel.Full;
            PhotonNetwork.autoJoinLobby = false;           
            PhotonNetwork.automaticallySyncScene = true;    

            Port = gameObject.GetComponent<NetworkManager>().Port;
            RoomName = gameObject.GetComponent<NetworkManager>().RoomName;

            Debug.Log("Laucher awaken");
        }

        /// <summary>
        /// Attempts to connect to the specified Room Name on start
        /// </summary>
        public virtual void Start()
        {
            Connect();
        }

        /// <summary>
        /// Joins the specified Room Name if already connected to the Photon Network; otherwise, connect to the master server using the current version
        /// number
        /// </summary>
        private void Connect()
        {
            if (PhotonNetwork.connected)
            {
                PhotonNetwork.JoinRoom(RoomName);
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings(_version);
            }
        }

        /// <summary>
        /// Send mesh to a host specified by networkConfig.
        /// Currently, only the HoloLens implements this method
        /// </summary>
        /// <param name="networkConfig">IP and port number of the host. Format: IP:Port</param>
        [PunRPC]
        public virtual void SendMesh(String networkConfig)
        {
            Debug.Log("Callee is not HoloLens and this is a HoloLens only method");
        }

        /// <summary>
        /// Send mesh to a host specified by PhotonNetwork.Player.ID.
        /// Currently, only the MasterClient implements this method
        /// </summary>
        /// <param name="id">The player id that will sent the mesh</param>
        [PunRPC]
        public virtual void SendMesh(int id)
        {
            Debug.Log("Callee is not MasterClient and this is a MasterClient only method");
        }

        //Virtual client method for sending mesh
        public virtual void SendMesh()
        {

        }

        //Virtual client method for sending addMesh
        public virtual void SendAddMesh()
        {

        }

        /// <summary>
        /// Receive room mesh from specified network configuration.
        /// Currently, only the ReveivingClient class implements this method
        /// </summary>
        /// <param name="networkConfig">The IP and port number that client can reveice room mesh from. The format is IP:Port</param>
        [PunRPC]
        public virtual void ReceiveMesh(String networkConfig)
        {
            Debug.Log("Callee is not a regular client and this is a regular client only method");
        }

        /// <summary>
        /// Receive room mesh from specifed PhotonNetwork.Player.ID. 
        /// Currently, only the MasterClient implements this method
        /// </summary>
        /// <param name="id">The player id that will receive mesh</param>
        [PunRPC]
        public virtual void ReceiveMesh(int id)
        {
            Debug.Log("Callee is not MasterClient and this is a MasterClient only method");
        }

        /// <summary>
        /// This will send a call to delete all meshes held by the clients
        /// This is a RPC method that will be called by ReceivingClient
        /// </summary>
        [PunRPC]
        public virtual void DeleteMesh()
        {

        }

        /// <summary>
        /// Deletes local copy of the mesh
        /// </summary>
        [PunRPC]
        public void DeleteLocalMesh()
        {
            if (Database.GetMeshAsBytes() != null)
            {
                Database.DeleteMesh();
            }
            foreach (GameObject gameObject in FindObjectsOfType(typeof(GameObject)))
            {
                if (gameObject.name == "mesh")
                {
                    Destroy(gameObject);
                }
            }

        }

        /// <summary>
        /// Needs to be called in order to instatite an object from asset bundle.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="id1"></param>
        /// <param name="np"></param>
        [PunRPC]
        public void SpawnNetworkObject(Vector3 pos, Quaternion rot, int id1, String assetName)
        {
            GameObject networkObject = networkAssets.LoadAsset(assetName) as GameObject;
            Instantiate(networkObject, pos, rot);
            PhotonView[] nViews = networkObject.GetComponentsInChildren<PhotonView>();
            nViews[0].viewID = id1;
        }

        /// <summary>
        /// Initiates the sending of a Mesh from ReceivingClient
        /// </summary>
        /// <param name="networkConfig"></param>
        [PunRPC]
        public virtual void SendAddMesh(string networkConfig)
        {

        }

        /// <summary>
        /// Receive room mesh to add to total mesh (MasterClient)
        /// and add it to the total roommesh
        /// </summary>
        /// <param name="id">The player id that will receive mesh</param>
        [PunRPC]
        public virtual void ReceiveAddMesh(int id)
        {

        }

        /// <summary>
        /// Receive room mesh to add to total mesh (ReceiveClientLauncher)
        /// and add it to the total roommesh
        /// </summary>
        /// <param name="networkConfig">The player id that will receive mesh</param>
        [PunRPC]
        public virtual void ReceiveAddMesh(string networkConfig)
        {

        }
    }

    //NEEDED FOR HOLOLENS BUT NOTHING ELSE-------------------------------------------------------------------------------------
    //WILL NOT COMPILE TO ANYTHING ELSE BUT HOLOLENS SO LEAVE THIS SECTION OF CODE COMMENTED OUT IF NOT HOLOLENS
    //
    /// <summary>
    /// MeshDisplay extends SpatialMappingSource (provided by Holotoolkit) to implement the
    /// functionality needed to display a mesh created via a HoloLens
    /// </summary>
#if !UNITY_EDITOR && UNITY_WSA_10_0
    public class MeshDisplay : SpatialMappingSource
    {
        /// <summary>
        /// Display the mesh currently saved in Database
        /// </summary>
        public void DisplayMesh()
        {
            var meshes = (List<Mesh>)Database.GetMeshAsList();
            Debug.Log(meshes.Count);
            foreach (var mesh in meshes)
            {
                GameObject surface = AddSurfaceObject(mesh, string.Format("Beamed-{0}", SurfaceObjects.Count), transform);
                surface.transform.parent = SpatialMappingManager.Instance.transform;
                surface.GetComponent<MeshRenderer>().enabled = true;
                surface.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }

        /// <summary>
        /// Scan the room and return the scaned room as a RoomMesh (serialized as a byte array)
        /// This method can ONLY be used by HoloLens
        /// </summary>
        /// <returns>Serialized Room Mesh</returns>
        public byte[] LoadMesh()
        {
            SpatialMappingManager mappingManager = GetComponent<SpatialMappingManager>();
            List<MeshFilter> meshFilters = mappingManager.GetMeshFilters();
            List<Mesh> meshes = new List<Mesh>();

            foreach (var meshFilter in meshFilters)
            {
                Mesh mesh = meshFilter.sharedMesh;
                Mesh clone = new Mesh();
                List<Vector3> verts = new List<Vector3>();
                verts.AddRange(mesh.vertices);

                for (int i = 0; i < verts.Count; i++)
                {
                    verts[i] = meshFilter.transform.TransformPoint(verts[i]);
                }

                clone.SetVertices(verts);
                clone.SetTriangles(mesh.triangles, 0);
                meshes.Add(clone);
            }

            return SimpleMeshSerializer.Serialize(meshes);
        }
    }
//   END OF NEEDED FOR HOLOLENS BUT NOTHING ELSE-------------------------------------------------------------------------------
    /// <summary>
#endif
}


