using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorFunctions : MonoBehaviour {

    /// <summary>
    /// Moves the camera based on the movement of the cursor since last update relative to the current camera
    /// </summary>
    /// <param name="Movement"></param>
    public void MoveCamera(Vector3 Movement, Camera currentCamera)
    {
        //reduce movement to a realistic value
        Movement = Movement / 75;

        currentCamera.transform.position = currentCamera.transform.position - currentCamera.transform.up * Movement.y;
        currentCamera.transform.position = currentCamera.transform.position - currentCamera.transform.right * Movement.x;
    }

    /// <summary>
    /// Moves a GameObject based change in cursor position since last update relative to the current camera
    /// </summary>
    /// <param name="CurrentPosition"></param>
    /// <param name="PreviousPosition"></param>
    /// <param name="currentCamera"></param>
    /// <param name="objectToRotate"></param>
    public void UpdateObjectRotation(Vector3 CurrentPosition, Vector3 PreviousPosition, Camera currentCamera, GameObject objectToRotate)
    {
        Vector3 v3 = CurrentPosition;
        Vector3 v3m = PreviousPosition;

        Vector3 Dir;
        Vector3 Move;
        Vector3 cross;

        float rot;

        v3.z = Vector3.Distance(currentCamera.transform.position, objectToRotate.transform.position) *
            Mathf.Cos(Mathf.Deg2Rad * (Vector3.Angle(currentCamera.transform.forward,
            Vector3.Normalize(objectToRotate.transform.position - currentCamera.transform.position))));
        v3 = currentCamera.ScreenToWorldPoint(v3);

        v3m.z = Vector3.Distance(currentCamera.transform.position, objectToRotate.transform.position) *
            Mathf.Cos(Mathf.Deg2Rad * (Vector3.Angle(currentCamera.transform.forward,
            Vector3.Normalize(objectToRotate.transform.position - currentCamera.transform.position))));
        v3m = currentCamera.ScreenToWorldPoint(v3m);

        Dir = v3 - objectToRotate.transform.position;
        Move = v3m - objectToRotate.transform.position;

        Dir.Normalize();
        Move.Normalize();

        rot = Vector3.Angle(Dir, Move);

        cross = Vector3.Cross(Dir, Move);

        if (cross.x + cross.y + cross.z < 0) //change direction based on Z value
        {
            rot = -rot;
        }

        objectToRotate.transform.Rotate(currentCamera.transform.forward, rot, Space.World);
    }

    /// <summary>
    /// Moves the GameObject to the cursors position relative to the camera
    /// </summary>
    /// <param name="CursorPosition"></param>
    /// <param name="currentCamera"></param>
    /// <param name="objectToMove"></param>
    public void MoveObjectPosition(Vector3 CursorPosition, Camera currentCamera, GameObject objectToMove)
    {
        Vector3 v3 = CursorPosition;

        v3.z = Vector3.Distance(currentCamera.transform.position, objectToMove.transform.position) *
            Mathf.Cos(Mathf.Deg2Rad * (Vector3.Angle(currentCamera.transform.forward,
            Vector3.Normalize(objectToMove.transform.position - currentCamera.transform.position))));

        v3 = currentCamera.ScreenToWorldPoint(v3);

        objectToMove.transform.position = v3;
    }

    /// <summary>
    /// Returns the first object found at the cursor position based on the current camera
    /// </summary>
    /// <param name="cursor"></param>
    /// <param name="currentCamera"></param>
    /// <returns></returns>
    public GameObject getObjectAtCursor(Vector3 cursor, Camera currentCamera)
    {
        Ray R = currentCamera.ScreenPointToRay(cursor);

        RaycastHit hit;
        
        Physics.Raycast(R, out hit, 1000);

        //check what was hit and return the value
        if (hit.collider != null)
        {
            return GameObject.Find(hit.collider.name);
        }
        else
            return null;
    }

}
