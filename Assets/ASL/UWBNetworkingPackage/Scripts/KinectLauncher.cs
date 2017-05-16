using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace UWBNetworkingPackage
{
    /// <summary>
    /// KinectLauncher implements launcher functionality specific to the Microsoft Kinect. Currently, KinectLauncher
    /// just extends ReceivingClientLauncher, but for further development add Kinect specific funtionality here
    /// </summary>
    public class KinectLauncher : ReceivingClientLauncher
    {
        // Ensure not HoloLens
        #if UNITY_EDITOR && !UNITY_WSA_10_0

        // Add Kinect specific functionality here.

#endif
    }
}

