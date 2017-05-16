using System;
using UnityEngine;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.Serialization;
#if !UNITY_WSA_10_0
    using System.Runtime.Serialization.Formatters.Binary;
#endif
//using HoloToolkit.Unity;

namespace UWBNetworkingPackage
{
    /// <summary>
    /// MasterClientLauncher implements launcher functionality specific to the MasterClient
    /// </summary>
    public class MasterClientLauncher : Launcher
    {

#if UNITY_STANDALONE
#region Private Properties

        private DateTime _lastUpdate = DateTime.MinValue;   // Used for keeping the Room Mesh up to date
        private bool testing = false; // Used to enable debugging keys for testing
#endregion

        /// <summary>
        /// Attempts to connect to the specified Room Name on start, and adds MeshDisplay component
        /// for displaying the Room Mesh
        /// </summary>
        public override void Start()
        {
            base.Start();
//            gameObject.AddComponent<MeshDisplay>();
        }

        /// <summary>
        /// Called once per frame
        /// When a new mesh is recieved, display it 
        /// When L is pressed, load and send a saved Room Mesh file (used for testing without HoloLens)
        /// </summary>
        public void Update()
        {
            if (Database.LastUpdate != DateTime.MinValue && DateTime.Compare(_lastUpdate, Database.LastUpdate) < 0)
            {
                if (Database.GetMeshAsBytes() != null)
                {
                //    Create a material to apply to the mesh
                    Material meshMaterial = new Material(Shader.Find("Diffuse"));

                //    grab the meshes in the database
                    IEnumerable<Mesh> temp = new List<Mesh>(Database.GetMeshAsList());

                    foreach (var mesh in temp)
                    {
                //        for each mesh in the database, create a game object to represent
                //        and display the mesh in the scene
                        GameObject obj1 = new GameObject("mesh");

                //        add a mesh filter to the object and assign it the mesh
                        MeshFilter filter = obj1.AddComponent<MeshFilter>();
                        filter.mesh = mesh;

                //        add a mesh rendererer and add a material to it
                        MeshRenderer rend1 = obj1.AddComponent<MeshRenderer>();
                        rend1.material = meshMaterial;
                    }
                }
                _lastUpdate = Database.LastUpdate;
            }

            //Loading a mesh from a file for testing purposes.
            if (Input.GetKeyDown("l"))
            {
                // Database.UpdateMesh(MeshSaver.Load("RoomMesh"));
                var memoryStream = new MemoryStream(File.ReadAllBytes("Assets/ASL/SampleRoomMesh"));
                this.DeleteLocalMesh();
                Database.UpdateMesh(memoryStream.ToArray());
                photonView.RPC("ReceiveMesh", PhotonTargets.Others, GetLocalIpAddress() + ":" + Port);
            }

            if (testing)
            {
                if (Input.GetKeyDown("c"))
                {
                    // Database.UpdateMesh(MeshSaver.Load("RoomMesh"));
                    var memoryStream = new MemoryStream(File.ReadAllBytes("imaginecup"));
                    Database.AddToMesh(memoryStream.ToArray());
                    photonView.RPC("ReceiveMesh", PhotonTargets.Others, GetLocalIpAddress() + ":" + Port);
                }

                //Deleting of meshes for testing purposes
                if (Input.GetKeyDown("d"))
                {
                    this.DeleteMesh();
                }

                ////AssettBundle sample send for testing purposes
                if (Input.GetKeyDown("b"))
                {
                    string path = Application.dataPath + "/ASL/StreamingAssets/AssetBundlesPC";
                    foreach (string file in System.IO.Directory.GetFiles(path))
                    {
                        if (file.Contains("networkbundle") && !file.Contains("manifest") && !file.Contains("meta"))
                        {
                            Debug.Log(file);
                            //Note: this will break if the file path in which asset bundles is stored
                            //is change.  If time, will come back later to fix the string parsing within
                            //the file.
                            byte[] bytes = File.ReadAllBytes(file);
                            Debug.Log(bytes.Length);
                            AssetBundle bundle = AssetBundle.LoadFromMemory(bytes);

                            GameObject networkObject = bundle.LoadAsset("Cube") as GameObject;
                            Instantiate(networkObject, transform.position, transform.rotation);
                            PhotonView[] nViews = networkObject.GetComponentsInChildren<PhotonView>();
                            nViews[0].viewID = PhotonNetwork.AllocateViewID();

                        }
                    }
                }

                //AssetBundle object instantiation for testing purposes
                if (Input.GetKeyDown("i"))
                {
                    int id1 = PhotonNetwork.AllocateViewID();

                    photonView.RPC("SpawnNetworkObject", PhotonTargets.AllBuffered, transform.position, transform.rotation, id1, "Cube");
                }
            }
        }

        /// <summary>
        /// When connect to the Master Server, create a room using the specified room name
        /// </summary>
        public override void OnConnectedToMaster()
        {
            PhotonNetwork.CreateRoom(RoomName);
            string path = Application.dataPath + "/ASL/StreamingAssets/AssetBundlesPC";
            foreach (string file in System.IO.Directory.GetFiles(path))
            {
                if (file.Contains("networkbundle") && !file.Contains("manifest") && !file.Contains("meta"))
                {
                    byte[] bytes = File.ReadAllBytes(file);
                    this.networkAssets = AssetBundle.LoadFromMemory(bytes);
                }
            }
        }

        /// <summary>
        /// After creating a room, set up a multi-threading tcp listener to listen on the specified port
        /// Once someone connects to the port, send the currently saved (in Database) Room Mesh
        /// It creates a second tcp listener to send asset bundles across Port+1
        /// </summary>
        public override void OnCreatedRoom()
        {
            TcpListener server = new TcpListener(IPAddress.Any, Port);
            server.Start();
            new Thread(() =>
            {
                Debug.Log("MasterClient start listening for new connection");
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Debug.Log("New connection established");
                    new Thread(() =>
                    {
                        using (NetworkStream stream = client.GetStream())
                        {
                            var data = Database.GetMeshAsBytes();
                            stream.Write(data, 0, data.Length);
                            Debug.Log("Mesh sent: mesh size = " + data.Length);
                        }
                        client.Close();
                    }).Start();

                }
            }).Start();
        }



        /// <summary>
        /// This returns local IP address
        /// </summary>
        /// <returns>Local IP address of the machine running as the Master Client</returns>
        private IPAddress GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    return ip;
                }
            }
            return null;
        }

        /// <summary>
        /// Performs the actual sending of bundles.  The path is determined
        /// by the calling function which is dependent upon platform
        /// </summary>
        /// <param name="id"></param>
        /// <param name="path"></param>
        /// <param name="port"></param>
        private void SendBundles(int id, string path, int port)
        {
            TcpListener bundleListener = new TcpListener(IPAddress.Any, port);

            bundleListener.Start();
                    new Thread(() =>
                        {
                            var client = bundleListener.AcceptTcpClient();
                            
                            using (var stream = client.GetStream())
                            {
                                //needs to be changed back
                                byte[] data = File.ReadAllBytes(path);
                                stream.Write(data, 0, data.Length);
                                client.Close();
                                Debug.Log("finish sending bundle" + stream.Length);

                            }
                            
                            bundleListener.Stop();
                        }).Start();
                    photonView.RPC("ReceiveBundles", PhotonPlayer.Find(id), GetLocalIpAddress() + ":" + port);
        }

        #region RPC Method

        /// <summary>
        /// Send mesh to a host specified by PhotonNetwork.Player.ID 
        /// This is a RPC method that will be called by ReceivingClient
        /// </summary>
        /// <param name="id">The player id that will sent the mesh</param>
        [PunRPC]
        public override void SendMesh(int id)
        {
            if (Database.GetMeshAsBytes() != null)
            {
                photonView.RPC("ReceiveMesh", PhotonPlayer.Find(id), GetLocalIpAddress() + ":" + Port);
            }
        }


        /// <summary>
        /// Send bundles for PC
        /// </summary>
        /// <param name="id"></param>
        [PunRPC]
        public void SendPCBundles(int id)
        {
            string path = Application.dataPath + "/ASL/StreamingAssets/AssetBundlesPC";
            foreach (string file in System.IO.Directory.GetFiles(path))
            {
                if (file.Contains("networkbundle") && !file.Contains("manifest") && !file.Contains("meta"))
                {
                    SendBundles(id, file, (Port + 5));
                }
            }
        }        

        /// <summary>
        /// Send bundles for Android
        /// </summary>
        /// <param name="id"></param>
        [PunRPC]
        public void SendAndroidBundles(int id)
        {
            string path = Application.dataPath + "/ASL/StreamingAssets/AssetBundlesAndroid";
            foreach (string file in System.IO.Directory.GetFiles(path))
            {
                if (file.Contains("networkbundle") && !file.Contains("manifest") && !file.Contains("meta"))
                {
                    SendBundles(id, file, (Port + 2));
                }
            }
        }

        /// <summary>
        /// Send bundles for hololens
        /// </summary>
        /// <param name="id"></param>
        [PunRPC]
        public void SendHololensBundles(int id)
        {
            string path = Application.dataPath + "/ASL/StreamingAssets/AssetBundlesHololens";
            foreach (string file in System.IO.Directory.GetFiles(path))
            {
                if (file.Contains("networkbundle") && !file.Contains("manifest") && !file.Contains("meta"))
                {
                    SendBundles(id, file, (Port + 3));
                }
            }
        }

        /// <summary>
        /// Receive room mesh from specifed PhotonNetwork.Player.ID
        /// This is a RPC method this will be called by HoloLens
        /// </summary>
        /// <param name="id">The player id that will receive mesh</param>
        [PunRPC]
        public override void ReceiveMesh(int id)
        {
            // Setup TCPListener to wait and receive mesh
            this.DeleteLocalMesh();
            TcpListener receiveTcpListener = new TcpListener(IPAddress.Any, Port + 1);
            receiveTcpListener.Start();
            new Thread(() =>
            {
                var client = receiveTcpListener.AcceptTcpClient();
                using (var stream = client.GetStream())
                {
                    byte[] data = new byte[1024];

                    Debug.Log("Start receiving mesh");
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int numBytesRead;
                        while ((numBytesRead = stream.Read(data, 0, data.Length)) > 0)
                        {
                            ms.Write(data, 0, numBytesRead);
                        }
                        Debug.Log("finish receiving mesh: size = " + ms.Length);
                        client.Close();
                        Database.UpdateMesh(ms.ToArray());
                    }
                }
                client.Close();
                receiveTcpListener.Stop();
                photonView.RPC("ReceiveMesh", PhotonTargets.Others, GetLocalIpAddress() + ":" + Port);
            }).Start();

            photonView.RPC("SendMesh", PhotonPlayer.Find(id), GetLocalIpAddress() + ":" + (Port + 1));
        }

        /// <summary>
        /// This will send a call to delete all meshes held by the clients
        /// This is a RPC method that will be called by ReceivingClient
        /// </summary>
        [PunRPC]
        public override void DeleteMesh()
        {
            if (Database.GetMeshAsBytes() != null)
            {
                photonView.RPC("DeleteLocalMesh", PhotonTargets.Others);
            }
            this.DeleteLocalMesh();
        }

        /// <summary>
        /// Initiates the sending of a Mesh to add
        /// </summary>
        public override void SendAddMesh()
        {
            if (Database.GetMeshAsBytes() != null)
            {
                photonView.RPC("ReceiveAddMesh", PhotonTargets.Others, GetLocalIpAddress() + ":" + Port);
            }
        }

        /// <summary>
        /// Receive room mesh from specifed PhotonNetwork.Player.ID. 
        /// and add it to the total roommesh
        /// </summary>
        /// <param name="networkConfig">The player id that will receive mesh</param>
        /// <param name="networkOrigin">The origin of the sent mesh</param>
        [PunRPC]
        public override void ReceiveAddMesh(int id)
        {
            // Setup TCPListener to wait and receive mesh
            TcpListener receiveTcpListener = new TcpListener(IPAddress.Any, Port + 4);
            receiveTcpListener.Start();
            new Thread(() =>
            {
                var client = receiveTcpListener.AcceptTcpClient();
                using (var stream = client.GetStream())
                {
                    byte[] data = new byte[1024];

                    Debug.Log("Start receiving mesh");
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int numBytesRead;
                        while ((numBytesRead = stream.Read(data, 0, data.Length)) > 0)
                        {
                            ms.Write(data, 0, numBytesRead);
                        }
                        Debug.Log("finish receiving mesh: size = " + ms.Length);
                        client.Close();
                        Database.AddToMesh(ms.ToArray());
                    }
                }
                client.Close();
                receiveTcpListener.Stop();
                photonView.RPC("ReceiveMesh", PhotonTargets.Others, GetLocalIpAddress() + ":" + Port);
            }).Start();

            photonView.RPC("SendAddMesh", PhotonPlayer.Find(id), GetLocalIpAddress() + ":" + (Port + 4));
        }

#endregion

#endif
    }
}

