using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace UWBNetworkingPackage
{
    /// <summary>
    /// ViveLauncher implements launcher functionality specific to the HTC Vive. Currently, ViveLauncher
    /// just extends ReceivingClientLauncher, but for further development add Vive specific funtionality here
    /// </summary>
    public class ViveLauncher : ReceivingClientLauncher {

// Ensure not HoloLens
#if UNITY_EDITOR && !UNITY_WSA_10_0

        // Add Vive specific functionality here 

#endif
    }
}