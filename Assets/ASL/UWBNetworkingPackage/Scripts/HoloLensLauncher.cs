using System;
using UnityEngine;
using System.Collections;
using Photon;

#if !UNITY_EDITOR && UNITY_WSA_10_0
using HoloToolkit.Unity;
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

        // Only included if HoloLens
#if !UNITY_EDITOR && UNITY_WSA_10_0
        
        public static SpatialMappingManager SpatialMappingManager;  // Needed for Room Scanning / Room Mesh
        private StreamSocket holoClient;    // Async network client (asynchronous I/O needed for HoloLens)
        private IAsyncAction connection;    // Used for creating an asynchronous connection to the Master Client
#endif

#if !UNITY_EDITOR && UNITY_WSA_10_0
        
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
            //photonView.RPC("SendHoloLensBundles", PhotonTargets.MasterClient, PhotonNetwork.player.ID);
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

        /// <summary>
        /// Loads the mesh currently saved on the HoloLens, adds the mesh to the Database, and attempts to send the mesh to the 
        /// Master Client by establishing a new network connection (set up via RPC)
        /// </summary>
        public void SendAddMesh()
        {
            Database.UpdateMesh(gameObject.AddComponent<MeshDisplay>().LoadMesh());
            photonView.RPC("ReceiveAddMesh", PhotonTargets.MasterClient, PhotonNetwork.player.ID);
        }

        /// <summary>
        /// Makes a call to delete mesh across network
        /// </summary>
        public void DeleteMesh()
        {
            photonView.RPC("DeleteMesh", PhotonTargets.MasterClient, PhotonNetwork.player.ID);
        }

                /// <summary>
        ///// Receive Bundles from the Master client.  Loads all assets from these bundles.
        ///// </summary>
        ///// <param name="networkConfig"></param>
        //[PunRPC]
        //public void ReceiveBundles(string networkConfig)
        //{
        //    var networkConfigArray = networkConfig.Split(':');

        //    TcpClient client = new TcpClient();
        //    Debug.Log(Int32.Parse(networkConfigArray[1]));
        //    client.Connect(IPAddress.Parse(networkConfigArray[0]), Int32.Parse(networkConfigArray[1]));

        //    using (var stream = client.GetStream())
        //    {
        //        byte[] data = new byte[1024];

        //        Debug.Log("Start receiving bundle.");
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            int numBytesRead;
        //            while ((numBytesRead = stream.Read(data, 0, data.Length)) > 0)
        //            {
        //                ms.Write(data, 0, numBytesRead);
        //            }
        //            Debug.Log("Finish receiving bundle: size = " + ms.Length);
        //            client.Close();

        //            AssetBundle newBundle = AssetBundle.LoadFromMemory(ms.ToArray());
        //            newBundle.LoadAllAssets();
        //            Debug.Log("You loaded the bundle successfully.");

        //        }
        //    }

        //    client.Close();
            

        //}

        #region RPC Method

        // Only included if HoloLens



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

        /// <summary>
        /// Asynchronously sends the Room Mesh to the specified network configuration
        /// </summary>
        /// <param name="networkConfig">The network information used for sending the mesh (IP address and port)</param>
        [PunRPC]
        public override void ReceiveBundles(string networkConfig)
        {
            Debug.Log("Master client has created a listener and would like us to receive bundle");

            holoClient = new StreamSocket();
            string[] networkConfigArray = networkConfig.Split(':');
            connection = holoClient.ConnectAsync(new HostName(networkConfigArray[0]), networkConfigArray[1]);
            var aach = new AsyncActionCompletedHandler(NetworkConnectedHandlerBundles);
            connection.Completed = aach;
        }  

        #endregion
#endif



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
//                    networkDataWriter.WriteBytes(Database.GetMeshAsBytes());

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

        public void NetworkConnectedHandlerBundles(IAsyncAction asyncInfo, AsyncStatus status) {
            //Debug.Log("YOU CONNECTED TO: " + networkConnection.Information.RemoteAddress.ToString());

            // Status completed is successful.
            if (status == AsyncStatus.Completed) {
                DataReader networkDataReader;

                // Since we are connected, we can send the data we set aside when establishing the connection.
                using (networkDataReader = new DataReader(holoClient.InputStream)) {
                    Debug.Log("PREPARING TO READ DATA");
                    // Then write the data.
                    byte[] bytes = networkDataReader.ReadBytes();
                    this.networkAssets = AssetBundle.LoadFromMemory(bytes);
                    // Again, this is an async operation, so we'll set a callback.
                    DataReaderLoadOperation drlo = networkDataWriter.LoadAsync();
                    drlo.Completed = new AsyncOperationCompletedHandler<uint>(DataSentHandler);
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