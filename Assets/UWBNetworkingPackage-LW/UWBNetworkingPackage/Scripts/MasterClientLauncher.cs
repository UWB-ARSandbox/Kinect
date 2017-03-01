

using System;
using UnityEngine;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using HoloToolkit.Unity;

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

        #endregion

        /// <summary>
        /// Attempts to connect to the specified Room Name on start, and adds MeshDisplay component
        /// for displaying the Room Mesh
        /// </summary>
        public override void Start()
        {
            base.Start();
            gameObject.AddComponent<MeshDisplay>();
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
                gameObject.GetComponent<MeshDisplay>().DisplayMesh();
                _lastUpdate = Database.LastUpdate;
            }

            if (Input.GetKeyDown("l"))
            {
                Database.UpdateMesh(MeshSaver.Load("RoomMesh"));
                photonView.RPC("ReceiveMesh", PhotonTargets.Others, GetLocalIpAddress() + ":" + Port);
            }
        }

        /// <summary>
        /// When connect to the Master Server, create a room using the specified room name
        /// </summary>
        public override void OnConnectedToMaster()
        {
            PhotonNetwork.CreateRoom(RoomName);
        }

        /// <summary>
        /// After creating a room, set up a multi-threading tcp listener to listen on the specified port
        /// Once someone connects to the port, send the currently saved (in Database) Room Mesh
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
        /// Receive room mesh from specifed PhotonNetwork.Player.ID
        /// This is a RPC method this will be called by HoloLens
        /// </summary>
        /// <param name="id">The player id that will receive mesh</param>
        [PunRPC]
        public override void ReceiveMesh(int id)
        {
            // Setup TCPListener to wait and receive mesh
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

        #endregion

        #region Private Method

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

        #endregion

#endif
    }
}

