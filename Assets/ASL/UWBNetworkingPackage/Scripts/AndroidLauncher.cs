using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace UWBNetworkingPackage
{
    /// <summary>
    /// AndroidLauncher implements launcher functionality specific to the Android platform. Currently, AndroidLauncher
    /// just extends ReceivingClientLauncher, but for further development add android specific funtionality here
    /// </summary>
    public class AndroidLauncher : ReceivingClientLauncher
    {
        // Ensure not HoloLens
        #if UNITY_EDITOR && !UNITY_WSA_10_0

        // Add Android specific functionality here.

        #endif
    }
}
