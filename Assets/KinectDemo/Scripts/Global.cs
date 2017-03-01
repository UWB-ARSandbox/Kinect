/* Global
 * 
 * This class contains static methods and global enums accessable throughout the
 *  entire program. They can be accessed with Global.<method>(). For example,
 *  Global.SendObject(obj). 
 *  
 * The static lists in this class (and their cooresponding methods) also act as a 
 *  relay between the DataSending and DataReceiving classes. The purpose for this
 *  is because DataSending and DataRecieving each have their own managed threads,
 *  and these threads have common data that they need to share with eachother (in
 *  the form of the Dictionary) and data to share with Unity (to create and move
 *  GameObjects managed by Unity).
 */

using UnityEngine;
using System.Collections.Generic;
using System;

public class Global : MonoBehaviour
{
    // Avaliable objects to spawn
    // These live in Resources/Prefabs
    // They must have the exact name of the Prefab
    public enum ObjType
    {
        Cube = 1,
        Sphere = 2,
        Tree = 3
    }

    // Network Flags that denote the type of packet sent in DataSending,
    //  or recieved in DataRecieving. These must be the same on both 
    //  sides of the connection. They are passed as integers in the packet.
    public enum NetFlag
    {
        MESH_FLAG = 1,
        OBJECT_CREATE_FLAG = 2,
        STRING_FLAG = 3,
        OBJECT_MOVE_FLAG = 4,
        CAMERA_FLAG = 5,
        HOLO_HEAD_CREATION_FLAG = 6,
        AVATAR_CREATION_FLAG = 7,
        DELETE_FLAG = 8,
        VIVE_CREATION_FLAG = 9,
        VIVE_MOVE_FLAG = 10
    }

    // Takes a generic Enum type and a basic string value and parses
    //  the string value to it's equivlent Enum value.
    public static T ToEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    //This public enum is used for the below clientStatus.The
    // clientStatus variable is used to exchange the status of the
    // client between the DataSending and DataRecieving objects
    public enum netStatus
    {
        Connected,
        Ready,
        Attempting,
        NotConnected,
        Error
    }
    public static netStatus clientStatus = netStatus.NotConnected;

    //// Contains a list of connection IDs, used for all clients connected to the server
    ////  that are NOT the hololens. The hololens is handled separately.
    //public static List<int> connectionIDs = new List<int>();

    // Contains a reference for every object created by the network (including the Kinect)
    public static Dictionary<int, GameObject> objects = new Dictionary<int, GameObject>();
    public static System.Object objectLock = new System.Object(); // The network is multi-threaded, this is our mutex

    // Contains a list of GameObjects that need to be sent to the network
    // NOTE: If you think you need this list, you should probably use the SendObject() 
    //  global method below.
    public static List<GameObject> newSpawns = new List<GameObject>();
    public static System.Object spawnLock = new System.Object(); // Mutex

    // Contains a list of moved GameObjects that need to be sent to the network
    // NOTE: If you think you need this list, you should probably use the MoveObject() 
    //  global method below.
    public static List<GameObject> moveObjs = new List<GameObject>();
    public static System.Object moveLock = new System.Object(); // Mutex

    public static List<int> deletedObjs = new List<int>();
    public static System.Object deleteLock = new System.Object(); // Mutex

    // Contains a list of bodies to be sent over the network
    // NOTE: If you think you need this list, you should probably use the SendBody() 
    //  global method below.
    public static List<GameObject> newBodies = new List<GameObject>();
    public static List<GameObject> bodies = new List<GameObject>();
    public static System.Object bodyLock = new System.Object(); // Mutex

    //// Contains a list of messages for the Kinect to forward to Hololens and other clients
    //public static List<byte[]> forwardMessages = new List<byte[]>();
    //public static List<int> forwardMessageID = new List<int>();
    //public static System.Object forwardLock = new System.Object(); // Mutex


    // Global Random generator
    private static System.Random rand = new System.Random();

    // Use this method if you've created a new object that needs to be sent over the network!
    // If you use this method, and it sends through the network, the sending code will create
    //  a unique ID for you, so you don't need to call the methods below yourself.
    public static void SendObject(GameObject obj)
    {
        lock (spawnLock)
        {
            Global.newSpawns.Add(obj);
        }
    }

    // Returns true if there are any newly spawned objects. If it returns true, obj is set to
    //  be equal to the first object in the list, and that object is removed from the list. If
    //  the method returns false, obj is set to null.
    public static bool GetSpawnedObject(out GameObject obj)
    {
        lock (spawnLock)
        {
            if (newSpawns.Count > 0)
            {
                obj = newSpawns[0];
                Global.newSpawns.RemoveAt(0);
                return true;
            }
        }

        obj = null;

        return false;
    }

    // Use this method if you've moved an object that needs to be sent over the network!
    public static void MoveObject(GameObject obj)
    {
        lock (moveLock)
        {
            Global.moveObjs.Add(obj);
        }
    }

    // Returns true if there are any newly spawned objects. If it returns true, obj is set to
    //  be equal to the first object in the list, and that object is removed from the list. If
    //  the method returns false, obj is set to null.
    public static bool GetMovedObject(out GameObject obj)
    {
        lock (moveLock)
        {
            if (moveObjs.Count > 0)
            {
                obj = moveObjs[0];
                Global.moveObjs.RemoveAt(0);
                return true;
            }
        }

        obj = null;

        return false;
    }

    // Takes a GameObject and retuns the game object's ID. Adds that GameObject
    //  to the dictionary. This is used in DataSending for object creation.
    public static int AddObject(GameObject obj)
    {
        // Generate a unique Object ID
        int objId = rand.Next();
        GameObject throwAway;
        while (objects.TryGetValue(objId, out throwAway))
            objId = rand.Next();

        // Add this to the hashmap
        lock (Global.objectLock)
        {
            Global.objects.Add(objId, obj);
        }

        // Return our unique Object ID
        return objId;
    }

    // Takes a unique ID and a GameObject and adds them to the Dictionary. This
    //  is used in DataRecieving when a newly created object is recieved. 
    public static void AddObject(int id, GameObject obj)
    {
        lock (Global.objectLock)
        {
            Global.objects.Add(id, obj);
        }
    }

    // Takes a unique ID and returns the cooresponding GameObject in the Dictionary.
    //  This is used in DataRecieving for moving an object.
    public static GameObject GetObject(int id)
    {
        lock (Global.objectLock)
        {
            return Global.objects[id];
        }
    }

    public static void DeleteObject(int id)
    {
        lock (Global.objectLock)
        {
            GameObject obj = Global.objects[id];
            Global.objects.Remove(id);
            Destroy(obj);
        }
    }

    public static void AddDeleteObject(int objId)
    {
        lock (Global.objectLock)
        {
            GameObject obj = Global.objects[objId];
            Global.objects.Remove(objId);
        }
        lock (deleteLock)
        {
            deletedObjs.Add(objId);
        }
    }

    public static int GetDeletedObj()
    {
        int returnId = 0;

        lock (deleteLock)
        {
            if (deletedObjs.Count > 0)
            {
                returnId = deletedObjs[0];
                Global.deletedObjs.RemoveAt(0);
            }
        }

        return returnId;
    }

    // Takes a unique ID and a GameObject and adds them to the Dictionary. This
    //  is used in DataRecieving when a newly created object is recieved. 
    public static void SendNewBody(GameObject obj)
    {
        lock (bodyLock)
        {
            Global.newBodies.Add(obj);
        }
    }

    public static void SendMovedBody(GameObject obj)
    {
        lock (bodyLock)
        {
            Global.bodies.Add(obj);
        }
    }

    public static void DeleteBody(GameObject obj)
    {
        int id = 0;
        lock (objectLock)
        {
            id = Int32.Parse(obj.name.Substring(obj.name.Length - 5, 5));
        }
        AddDeleteObject(id);
    }

    // Returns true if there are any newly spawned bodies. If it returns true, obj is set to
    //  be equal to the first body in the list, and that body is removed from the list. If
    //  the method returns false, obj is set to null.
    public static bool GetNewBody(out GameObject obj)
    {
        lock (bodyLock)
        {
            if (newBodies.Count > 0)
            {
                obj = newBodies[0];
                Global.newBodies.RemoveAt(0);
                return true;
            }
        }

        obj = null;

        return false;
    }

    // Returns true if there are any newly moved bodies. If it returns true, obj is set to
    //  be equal to the first body in the list, and that body is removed from the list. If
    //  the method returns false, obj is set to null.
    public static bool GetMovedBody(out GameObject obj)
    {
        lock (bodyLock)
        {
            if (bodies.Count > 0)
            {
                obj = bodies[0];
                Global.bodies.RemoveAt(0);
                return true;
            }
        }

        obj = null;

        return false;
    }

    //public static void AddForwardMessage(byte[] message, int connectionId)
    //{
    //    lock (forwardLock)
    //    {
    //        Global.forwardMessages.Add(message);
    //        Global.forwardMessageID.Add(connectionId);
    //    }
    //}

    //public static bool GetForwardMessage(out byte[] message, out int connectionId)
    //{
    //    lock (forwardLock)
    //    {
    //        if (forwardMessages.Count > 0)
    //        {
    //            message = forwardMessages[0];
    //            Global.forwardMessages.RemoveAt(0);
    //            connectionId = forwardMessageID[0];
    //            Global.forwardMessageID.RemoveAt(0);
    //            return true;
    //        }
    //    }

    //    message = null;
    //    connectionId = -1;

    //    return false;
    //}
}
