using System;
using UnityEngine;
using System.Collections;
using HoloToolkit.Unity;
using Photon;

#if !UNITY_EDITOR && UNITY_WSA_10_0
using Windows.Networking.Sockets;
using Windows.Foundation;
using Windows.Networking;
using Windows.Storage.Streams;
#endif

namespace UWBNetworkingPackage
{
    /// <summary>
    /// HoloLauncher implements launcher functionality specific to the HoloLens
    /// </summary>
    public class HoloLensLauncher : Launcher
    {
        public static SpatialMappingManager SpatialMappingManager;  // Needed for Room Scanning / Room Mesh

        // Only included if HoloLens
#if !UNITY_EDITOR && UNITY_WSA_10_0
        private StreamSocket holoClient;    // Async network client (asynchronous I/O needed for HoloLens)
        private IAsyncAction connection;    // Used for creating an asynchronous connection to the Master Client
#endif
        
        /// <summary>
        /// Sets Photon Network settings and retrieves reference to the Spatial Mapping Manager on awake
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            SpatialMappingManager = gameObject.AddComponent<SpatialMappingManager>();
        }

        /// <summary>
        /// Called when connected to the Master Server (different than the MasterClient - refer to Photon documentation)
        /// Attempt to join the specified Room Name
        /// </summary>
        public override void OnConnectedToMaster()
        {
            Debug.Log("OnConnectedToMaster called... Room Name: " + RoomName);

            PhotonNetwork.JoinRoom(RoomName);
        }

        /// <summary>
        /// Called after successfully joining the specified Room Name 
        /// Starts the observer (room scanning) functionality of the HoloLens
        /// </summary>
        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom called...");
            if (!SpatialMappingManager.IsObserverRunning())
            {
                SpatialMappingManager.StartObserver();
            }
        }

        /// <summary>
        /// Called if joining the specified Room Name failed 
        /// Log the issue to console and disconnect from Photon 
        /// </summary>
        /// <param name="codeAndMsg">Information about the failed connection</param>
        public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
            Debug.LogWarning("A room created by the Master Client could not be found. Disconnecting from PUN");
            PhotonNetwork.Disconnect();
        }

        /// <summary>
        /// Loads the mesh currently saved on the HoloLens, adds the mesh to the Database, and attempts to send the mesh to the 
        /// Master Client by establishing a new network connection (set up via RPC)
        /// </summary>
        public void SendMesh()
        {
            Database.UpdateMesh(gameObject.AddComponent<MeshDisplay>().LoadMesh());
            photonView.RPC("ReceiveMesh", PhotonTargets.MasterClient, PhotonNetwork.player.ID);
        }

        #region RPC Method

        // Only included if HoloLens
#if !UNITY_EDITOR && UNITY_WSA_10_0

        /// <summary>
        /// Asynchronously sends the Room Mesh to the specified network configuration
        /// </summary>
        /// <param name="networkConfig">The network information used for sending the mesh (IP address and port)</param>
        [PunRPC]
        public override void SendMesh(string networkConfig)
        {
            Debug.Log("Master client has created a listener and would like us to send mesh to: " + networkConfig);

            holoClient = new StreamSocket();
            string[] networkConfigArray = networkConfig.Split(':');
            connection = holoClient.ConnectAsync(new HostName(networkConfigArray[0]), networkConfigArray[1]);
            var aach = new AsyncActionCompletedHandler(NetworkConnectedHandler);
            connection.Completed = aach;
        }  
#endif

        #endregion

        #region Private Method

        // Only included if HoloLens
#if !UNITY_EDITOR && UNITY_WSA_10_0

        /// <summary>
        /// Called when the async action is completed (establishing a connection within SendMesh(string networkConfig))
        /// If connection is successful, write the Room Mesh data from the Database to the network connection
        /// If connection is unsuccessful, dispose of the client (StreamSocket)
        /// </summary>
        /// <param name="asyncInfo">Information about the async action</param>
        /// <param name="status">The current status of the async action</param>
        public void NetworkConnectedHandler(IAsyncAction asyncInfo, AsyncStatus status) {
            //Debug.Log("YOU CONNECTED TO: " + networkConnection.Information.RemoteAddress.ToString());

            // Status completed is successful.
            if (status == AsyncStatus.Completed) {
                Debug.Log("PREPARING TO WRITE DATA...");

                DataWriter networkDataWriter;

                // Since we are connected, we can send the data we set aside when establishing the connection.
                using (networkDataWriter = new DataWriter(holoClient.OutputStream)) {
                    Debug.Log("PREPARING TO WRITE DATA");
                    // Then write the data.
                    networkDataWriter.WriteBytes(Database.GetMeshAsBytes());

                    // Again, this is an async operation, so we'll set a callback.
                    DataWriterStoreOperation dswo = networkDataWriter.StoreAsync();
                    dswo.Completed = new AsyncOperationCompletedHandler<uint>(DataSentHandler);
                }

            } else {
                Debug.LogWarning("Failed to establish connection. Error Code: " + asyncInfo.ErrorCode);
                // In the failure case we'll requeue the data and wait before trying again.
                holoClient.Dispose();

            }
        }

        /// <summary>
        /// Called after the Room Mesh data has been sent over the network 
        /// Dispose of the network client (StreamSocket)
        /// </summary>
        /// <param name="operation">Information about the failed connection</param>
        /// <param name="status">The current status of the async action</param>

        public void DataSentHandler(IAsyncOperation<uint> operation, AsyncStatus status) {
            // Always disconnect here since we will reconnect when sending the next mesh
            Debug.Log("CLOSED THE CONNECTION");
            holoClient.Dispose();
        }
#endif
        #endregion
    }
}