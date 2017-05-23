using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UWBNetworkingPackage
{
    /// <summary>
    /// OculusLauncher implements launcher functionality specific to the Oculus. Currently, OculusLauncher
    /// just extends ReceivingClientLauncher, but for further development add Oculus specific funtionality here
    /// </summary>
    public class OculusLauncher : ReceivingClientLauncher
    {
// Ensure not HoloLens
#if UNITY_EDITOR && !UNITY_WSA_10_0

        // Add Oculus specific functionality here 

#endif
    }
}
