using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace UWBNetworkingPackage
{
    /// <summary>
    /// ReceivingClientLauncher is an abstract class (extended by all "Client" devices - Vive, Oculus, Kinect) that connects 
    /// to Photon and sets up a TCP connection with the Master Client to recieve Room Meshes when they are sent
    /// </summary>
    public abstract class ReceivingClientLauncher : Launcher
    {
// Ensure not HoloLens
#if UNITY_EDITOR && !UNITY_WSA_10_0

        /// <summary>
        /// After connect to master server, join the room that's specify by Laucher.RoomName
        /// </summary>
        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinRoom(RoomName);
        }

        /// <summary>
        /// After join the room, ask master client to sent the mesh to this client
        /// </summary>
        public override void OnJoinedRoom()
        {
            Debug.Log("Client joined room.");
            photonView.RPC("SendMesh", PhotonTargets.MasterClient, PhotonNetwork.player.ID);
        }

        /// <summary>
        /// When cannot join the room (refer to UWBNetworkingPackage documentation for possible reasons of failure), 
        /// disconnect from Photon
        /// </summary>
        /// <param name="codeAndMsg">Information about the failed connection</param>
        public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
            Debug.Log("A room created by the Master Client could not be found. Disconnecting from PUN");
            PhotonNetwork.Disconnect();
        }

        #region RPC Method

        /// <summary>
        /// Receive Room Mesh from specified network configuration. This is a RPC method that will be called by the Master Client
        /// </summary>
        /// <param name="networkConfig">The IP and port number that client can reveice room mesh from. The format is IP:Port</param>
        [PunRPC]
        public override void ReceiveMesh(string networkConfig)
        {
            var networkConfigArray = networkConfig.Split(':');

            TcpClient client = new TcpClient();
            client.Connect(IPAddress.Parse(networkConfigArray[0]), Int32.Parse(networkConfigArray[1]));

            using (var stream = client.GetStream())
            {
                byte[] data = new byte[1024];

                Debug.Log("Start receiving mesh.");
                using (MemoryStream ms = new MemoryStream())
                {
                    int numBytesRead;
                    while ((numBytesRead = stream.Read(data, 0, data.Length)) > 0)
                    {
                        ms.Write(data, 0, numBytesRead);
                    }
                    Debug.Log("Finish receiving mesh: size = " + ms.Length);
                    client.Close();
                    Database.UpdateMesh(ms.ToArray());
                }
            }

            client.Close();

            if (GetComponent<MeshDisplay>() == null)
            {
                gameObject.AddComponent<MeshDisplay>().DisplayMesh();
            }
            else
            {
                gameObject.GetComponent<MeshDisplay>().DisplayMesh();
            }
        }
        #endregion
#endif
    }
}