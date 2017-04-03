/* DataReceiving
 * 
 * The DataRecieving script must live in an object (usually an empty object) in the scene. The Server IP address in 
 *  the Unity editor must be filled out as the IP address of the current device. THIS device is the Server, so put
 *  in THIS device's IP address. 
 * This script handles connecting to devices, and recieving data from them in the form of packages. The packages
 *  are deconstructed and acted on in this script. These include the initial mesh push from the hololens, creation
 *  of new objects, amoung others. 
 */

using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

#if UNITY_EDITOR
using System.Net;
using System.Net.Sockets;
#endif

public class DataReceiving
{
    // FOR SERVER?
    //    // Server IP and Connection. 
    //    // Note 1: This file is being ran FROM the server (with the Kinect) attatched. Therefore, this information
    //    //  is the connection information of the current computer. E.g. This is YOUR IP address.
    //    // Note 2: This information appears hardcoded here, but the unity editor will overwrite these values.
    //    public string ServerIP;
    //    public int ConnectionPort = 45000;

    //    // Listens for network connections over TCP.
    //    private TcpListener networkListener;

    //    // Keeps client information when a connection happens.
    //    private TcpClient networkClient;

    //    // Tracks if the hololens client is connected.
    //    private bool ClientConnected = false;

    // List of all meshes. This is NOT a list of the objects in the scene. This is
    //  a list of the meshes sent from the Hololens. The purpose of this list is for
    //  saving the meshes recieved from the hololens, and loading them again.
    List<Mesh> globalMeshes; // NEEDED?

    // True after 'Start Server' is pressed
    private bool _isStarted = false;

    // Non-Hololens Client/Server variables
    public string KinectIp = "192.168.0.3";
    int port = 45045;
    private int m_ConnectionId = 0;
    private int m_WebSocketHostId = 0;
    private int m_GenericHostId = 0;
    private byte m_CommunicationChannel = 0;

    // Non-Hololens Server Configuration
    private ConnectionConfig m_Config = null;

    // Use this for initialization.
    void Start()
    {
        globalMeshes = new List<Mesh>();

        // Configuration for Server Settings for the Non-Hololens clients
        m_Config = new ConnectionConfig();                                         //create configuration containing one reliable channel
        m_CommunicationChannel = m_Config.AddChannel(QosType.Reliable);
    }

    //    // The OnGUI method provides the Connections Setting popup when the app first runs.
    //    //  With the isStarted boolean, the Update() method won't proceed until the Start
    //    //  Server button is pressed.
    void OnGUI()
    {
        if (!_isStarted)
        {
            GUI.Box(new Rect(5, 5, 310, 150), "Connection Settings");
            GUI.Label(new Rect(10, 35, 40, 30), "IP");
            KinectIp = GUI.TextField(new Rect(50, 35, 250, 30), KinectIp, 25);
            GUI.Label(new Rect(10, 65, 40, 30), "Port");
            port = Convert.ToInt32(GUI.TextField(new Rect(50, 65, 250, 30), port.ToString(), 25));

#if !(UNITY_WEBGL && !UNITY_EDITOR)
            if (GUI.Button(new Rect(30, 115, 250, 30), "Start Client"))
            {
                _isStarted = true;
                NetworkTransport.Init();

                // NEEDED?
                if (m_Config == null)
                {
                    m_Config = new ConnectionConfig();                                         //create configuration containing one reliable channel
                    m_CommunicationChannel = m_Config.AddChannel(QosType.Reliable);
                }

                HostTopology topology = new HostTopology(m_Config, 12);
                m_GenericHostId = NetworkTransport.AddHost(topology, 0); //any port for udp client, for websocket second parameter is ignored, as webgl based game can be client only
                byte error;
                m_ConnectionId = NetworkTransport.Connect(m_GenericHostId, KinectIp, port, 0, out error);
            }
#endif
        }
    }

    // Reused variables in Update
    int recHostId;
    int connectionId;
    int channelId;
    int bufferSize = 1024;
    int dataSize;
    byte error;

    // Struct to keep track of a Vive machine's particular headset and controller ID's,
    // in case more than one Vive machine is connected and one of them decides to disconnect
    struct ViveMachine
    {
        public int connectionID;
        public int headsetID;
        public int leftControllerID;
        public int rightControllerID;
    }

    // List of Vive machines for future lookup if/when one of them disconnects
    List<ViveMachine> ViveMachines = new List<ViveMachine>();

    // Update is called once per frame.
    void Update()
    {
        if (!_isStarted)
            return;

        //        // *******
        //        // The rest of the Update() look handles recieving data from the
        //        //  Non-Hololens clients. Both Hololens and Non-Hololens clients
        //        //  messages go through interpretIncomingPackage()
        //        // *******
        byte[] recBuffer = new byte[bufferSize];

        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {

            case NetworkEventType.Nothing:
                break;

            case NetworkEventType.ConnectEvent:
                {
                    Debug.Log(String.Format("Connection to host {0}, connection {1}", recHostId, connectionId));
                    // SEND ID OF KINECT?
                    break;
                }

            case NetworkEventType.DataEvent:
            {

                // Strip out the sent message size
                int messageSize = BitConverter.ToInt32(recBuffer, 0);
                // Create an array of that size
                byte[] messageData = new byte[messageSize - 4];
                // Copy the data we have into said array
                System.Buffer.BlockCopy(recBuffer, 4, messageData, 0, dataSize - 4);

                // While we haven't recieved all data..
                int givenDataSize = dataSize;
                while(givenDataSize < messageSize)
                {
                    // Recieve more, put it into the messageData array, add to our size, repeat..
                    NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
                    System.Buffer.BlockCopy(recBuffer, 0, messageData, givenDataSize, dataSize);
                    givenDataSize += dataSize;
                }

                if(messageSize < givenDataSize)
                {
                    Debug.LogError("Recieved more bytes than sent by client! Recieved " + givenDataSize + " bytes, expected " + messageSize + " bytes");
                }
                //Debug.Log(String.Format("Received event and Sent message: host {0}, connection {1}, message length {2}", recHostId, connectionId, messageData.Length));

                // Now, send that data along to the interpret function
                interpretIncomingPackage(messageData, messageData.Length);
                break;

            }
            case NetworkEventType.DisconnectEvent:
            {
                    Debug.Log(String.Format("Disconnect from host {0}, connection {1}", recHostId, connectionId));

                    break;
            }
        }
    }

    // This is the meat of DataRecieving. Takes the full package and interprets
    //  the package by taking out the first 4 bytes which denotes the package flag.
    //  Types of package flags can be found in the Global.NetFlag enum. The method
    //  then correctly acts on the flag (such as creating a new object in the scene).

    // VOID RETURN ON VIVE
    void interpretIncomingPackage(byte[] thePackage, int sizeOfPackage)
    {
        //a byte[] to hold the actual data
        byte[] theActualData = new byte[sizeOfPackage - sizeof(int)];

        //a byte[] to hold the flag data
        byte[] flagData = new byte[sizeof(int)];

        //GET THE FLAG OF THE PACKET
        Array.Copy(thePackage, flagData, sizeof(int));

        //convert the flag data into the actual int flag
        int flagInt = BitConverter.ToInt32(flagData, 0);
        //convert the flagInt into the actual flag
        Global.NetFlag flag = (Global.NetFlag)flagInt;

        //GET THE ACTUAL DATA OF THE PACKET
        //copy only the actual data into the actual data byte array
        for (int i = 0; i < theActualData.Length; i++)
        {
            theActualData[i] = thePackage[i + 4]; //start at the place of the actual data and do not grab the flag data that was in the front of the package
        }

        //Do work based on the flag found
        if (flag == Global.NetFlag.OBJECT_CREATE_FLAG)
        { //Receiving an object creation flag
          // Get the Position info
            float posX = BitConverter.ToSingle(theActualData, 0);
            float posY = BitConverter.ToSingle(theActualData, 4);
            float posZ = BitConverter.ToSingle(theActualData, 8);
            // Get the Rotation info
            float rotX = BitConverter.ToSingle(theActualData, 12);
            float rotY = BitConverter.ToSingle(theActualData, 16);
            float rotZ = BitConverter.ToSingle(theActualData, 20);
            // Create a new Position and Rotation for the object based on the passed info
            Quaternion newRotation = Quaternion.Euler(rotX, rotY, rotZ);
            Vector3 newPosition = new Vector3(posX, posY, posZ);
            // Determine the prefab of the object (See: Global.ObjType)
            Global.ObjType objType = (Global.ObjType)BitConverter.ToInt32(theActualData, 24);
            // Get the Object ID info
            int objId = BitConverter.ToInt32(theActualData, 28);
            // Create the correct prefab
            GameObject newObj = GameObject.Instantiate(Resources.Load("Prefabs/" + objType.ToString())) as GameObject;
            // Set the position and rotation
            newObj.transform.position = newPosition;
            newObj.transform.rotation = newRotation;
            // Set the name equal to the object ID
            newObj.name = objId.ToString();
            // Add this to the hashmap
            Global.AddObject(objId, newObj);

            Debug.Log(objType.ToString() + " created");
        }
        else if (flag == Global.NetFlag.STRING_FLAG)
        {  //The packet was sending a string message

            //decode the message
            string something = Encoding.ASCII.GetString(theActualData);

            //print out the message
            UnityEngine.Debug.Log("Message: " + something);
        }
        else if (flag == Global.NetFlag.OBJECT_MOVE_FLAG)
        { //Receiving an object move flag
            float posX = BitConverter.ToSingle(theActualData, 0);
            float posY = BitConverter.ToSingle(theActualData, 4);
            float posZ = BitConverter.ToSingle(theActualData, 8);

            float rotX = BitConverter.ToSingle(theActualData, 12);
            float rotY = BitConverter.ToSingle(theActualData, 16);
            float rotZ = BitConverter.ToSingle(theActualData, 20);

            Quaternion newRotation = Quaternion.Euler(rotX, rotY, rotZ);
            Vector3 newPosition = new Vector3(posX, posY, posZ);

            int objId = BitConverter.ToInt32(theActualData, 24);
            GameObject newObj = Global.GetObject(objId);
            newObj.transform.position = newPosition;
            newObj.transform.rotation = newRotation;

            //Debug.Log(newObj.name + " Moved");
        }
        else if (flag == Global.NetFlag.CAMERA_FLAG)
        { // The packet is sending the camera position
          // Set our camera position equal to the sent position
            float posX = BitConverter.ToSingle(theActualData, 0);
            float posY = BitConverter.ToSingle(theActualData, 4);
            float posZ = BitConverter.ToSingle(theActualData, 8);
            // Get the Rotation info
            float rotX = BitConverter.ToSingle(theActualData, 12);
            float rotY = BitConverter.ToSingle(theActualData, 16);
            float rotZ = BitConverter.ToSingle(theActualData, 20);
            // Create a new Position and Rotation for the object based on the passed info
            Quaternion newRotation = Quaternion.Euler(rotX, rotY, rotZ);
            Vector3 newPosition = new Vector3(posX, posY, posZ);

            Camera projCamera = GameObject.Find("KinectReference").GetComponent<Camera>();
            Camera topCamera = GameObject.Find("Top-Down").GetComponent<Camera>();

            projCamera.transform.position = newPosition;
            projCamera.transform.rotation = newRotation;

            Quaternion topRotation = Quaternion.Euler(90, rotY, 0);
            topCamera.transform.rotation = topRotation;
        }
        else if (flag == Global.NetFlag.HOLO_HEAD_CREATION_FLAG)
        {
            float posX = BitConverter.ToSingle(theActualData, 0);
            float posY = BitConverter.ToSingle(theActualData, 4);
            float posZ = BitConverter.ToSingle(theActualData, 8);

            float rotX = BitConverter.ToSingle(theActualData, 12);
            float rotY = BitConverter.ToSingle(theActualData, 16);
            float rotZ = BitConverter.ToSingle(theActualData, 20);

            Quaternion newRotation = Quaternion.Euler(rotX, rotY, rotZ);
            Vector3 newPosition = new Vector3(posX, posY, posZ);

            int objId = BitConverter.ToInt32(theActualData, 24);
            GameObject newObj = GameObject.Instantiate(Resources.Load("Prefabs/HoloHead")) as GameObject;

            newObj.transform.position = newPosition;
            newObj.transform.rotation = newRotation;

            newObj.name = objId.ToString();
            Global.AddObject(objId, newObj);

            UnityEngine.Debug.Log("HoloHead Created!");
        }
        else if (flag == Global.NetFlag.DELETE_FLAG)
        {
            int objId = BitConverter.ToInt32(theActualData, 0);

            Global.DeleteObject(objId);
        }
        else if (flag == Global.NetFlag.VIVE_CREATION_FLAG)
        {
            int hmdId = BitConverter.ToInt32(theActualData, 0);
            int leftCId = BitConverter.ToInt32(theActualData, 4);
            int rightCId = BitConverter.ToInt32(theActualData, 8);

            GameObject hmd = GameObject.Instantiate(Resources.Load("Prefabs/HMD")) as GameObject;
            GameObject leftC = GameObject.Instantiate(Resources.Load("Prefabs/Controller")) as GameObject;
            GameObject rightC = GameObject.Instantiate(Resources.Load("Prefabs/Controller")) as GameObject;

            Global.AddObject(hmdId, hmd);
            Global.AddObject(leftCId, leftC);
            Global.AddObject(rightCId, rightC);

            UnityEngine.Debug.Log("Vive Avatar Created!");

            // Store information on this Vive machine for future lookup (to differentiate
            // between multiple Vive machines that coulc be connected in case one of them
            // disconnects
            ViveMachine currentlyConnectingViveMachine;
            currentlyConnectingViveMachine.connectionID = connectionId;
            currentlyConnectingViveMachine.headsetID = hmdId;
            currentlyConnectingViveMachine.leftControllerID = leftCId;
            currentlyConnectingViveMachine.rightControllerID = rightCId;
            ViveMachines.Add(currentlyConnectingViveMachine);

        }
        else if (flag == Global.NetFlag.VIVE_MOVE_FLAG)
        {
            // HMD
            float posX = BitConverter.ToSingle(theActualData, 0);
            float posY = BitConverter.ToSingle(theActualData, 4);
            float posZ = BitConverter.ToSingle(theActualData, 8);

            float rotX = BitConverter.ToSingle(theActualData, 12);
            float rotY = BitConverter.ToSingle(theActualData, 16);
            float rotZ = BitConverter.ToSingle(theActualData, 20);

            int hmdId = BitConverter.ToInt32(theActualData, 24);
            GameObject hmd = Global.GetObject(hmdId);
            hmd.transform.position = new Vector3(posX, posY, posZ);
            hmd.transform.rotation = Quaternion.Euler(rotX, rotY, rotZ);

            // Left Controller
            posX = BitConverter.ToSingle(theActualData, 28);
            posY = BitConverter.ToSingle(theActualData, 32);
            posZ = BitConverter.ToSingle(theActualData, 36);

            rotX = BitConverter.ToSingle(theActualData, 40);
            rotY = BitConverter.ToSingle(theActualData, 44);
            rotZ = BitConverter.ToSingle(theActualData, 48);

            int leftCId = BitConverter.ToInt32(theActualData, 52);
            GameObject leftC = Global.GetObject(leftCId);
            leftC.transform.position = new Vector3(posX, posY, posZ);
            leftC.transform.rotation = Quaternion.Euler(rotX, rotY, rotZ);

            // Right Controller
            posX = BitConverter.ToSingle(theActualData, 56);
            posY = BitConverter.ToSingle(theActualData, 60);
            posZ = BitConverter.ToSingle(theActualData, 64);

            rotX = BitConverter.ToSingle(theActualData, 68);
            rotY = BitConverter.ToSingle(theActualData, 72);
            rotZ = BitConverter.ToSingle(theActualData, 76);

            int rightCId = BitConverter.ToInt32(theActualData, 80);
            GameObject rightC = Global.GetObject(rightCId);
            rightC.transform.position = new Vector3(posX, posY, posZ);
            rightC.transform.rotation = Quaternion.Euler(rotX, rotY, rotZ);
        }
        else
        {
            //PACKAGE TYPE DID NOT HAVE A VALID FLAG IDENTIFIER IN THE FRONT OF PACKAGE
            UnityEngine.Debug.LogError("UNKNOWN PACKAGE RECIEVED");
        }
    }

    //// <summary>
    //// Reads an int from the next 4 bytes of the supplied stream.
    //// </summary>
    //// <param name = "stream" > The stream to read the bytes from.</param>
   //// <returns>An integer representing the bytes.</returns>

    //    int ReadInt(Stream stream)
    //{
    //    The bytes arrive in the wrong order, so swap them.
    //        byte[] bytes = new byte[4];
    //    stream.Read(bytes, 0, 4);
    //    byte t = bytes[0];
    //    bytes[0] = bytes[3];
    //    bytes[3] = t;

    //    t = bytes[1];
    //    bytes[1] = bytes[2];
    //    bytes[2] = t;

    //    Then bitconverter can read the int32.
    //    return BitConverter.ToInt32(bytes, 0);
    //}

    //    /// <summary>
    //    /// Called when a client connects.
    //    /// </summary>
    //    /// <param name="result">The result of the connection.</param>
    //    void OnClientConnect(IAsyncResult result)
    //    {
    //        if (result.IsCompleted)
    //        {
    //            networkClient = networkListener.EndAcceptTcpClient(result);
    //            if (networkClient != null)
    //            {
    //                //Debug.Log("Connected for Recieve");
    //                ClientConnected = true;
    //            }
    //        }
}

