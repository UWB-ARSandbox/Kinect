using System;
using Photon;
using UnityEngine;
//using UnityEditor;



namespace UWBNetworkingPackage
{
    /// <summary>
    /// NetworkManager adds the correct Launcher script based on the user selected device (in the Unity Inspector)
    /// </summary>
    // Note: For future improvement, this class should: a) Detect the device and add the launcher automatically; 
    // or b) Only allow user to select one device
    [System.Serializable]
    public class NetworkManager : PunBehaviour
    {
        #region Public Properties

        public bool MasterClient = true;

        // Needed for Room Mesh sending
        [Tooltip("A port number for devices to communicate through. The port number should be the same for each set of projects that need to connect to each other and share the same Room Mesh.")]
        public int Port;

        // Needed for Photon 
        [Tooltip("The name of the room that this project will attempt to connect to. This room must be created by a \"Master Client\".")]
        public string RoomName;

#endregion

        /// <summary>
        /// When Awake, NetworkManager will add the correct Launcher script
        /// </summary>
        void Awake()
        {
            //Preprocessor directives to choose which component is added.  Note, master client still has to be hard coded
            //Haven't yet found a better solution for this

#if !UNITY_WSA_10_0 && !UNITY_ANDROID
            if (MasterClient)
            {
                gameObject.AddComponent<MasterClientLauncher>();
            }
            else
            {
                gameObject.AddComponent<ReceivingClientLauncher>();
            }
#endif

#if UNITY_WSA_10_0
            gameObject.AddComponent<HoloLensLauncher>();
#endif
#if UNITY_ANDROID
            gameObject.AddComponent<AndroidLauncher>();
#endif
        }

        /// <summary>
        /// This is a HoloLens specific method
        /// This method allows a HoloLens developer to send a Room Mesh when triggered by an event
        /// This is here because HoloLensLauncher is applied at runtime
        /// In the HoloLensDemo, this method is called when the phrase "Send Mesh" is spoken and heard by the HoloLens
        /// </summary>
#if UNITY_WSA_10_0
        public void HoloSendMesh()
        { 
            gameObject.GetComponent<HoloLensLauncher>().SendMesh();

        }
#endif
    }
}