using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalBounds : MonoBehaviour
{

    public GameObject trakedBodyParent;
    public GameObject boundBox;
    public Camera mainCamera;                   // main Camera in scene


    // Use this for initialization
    void Start ()
    {
        if (!mainCamera) mainCamera = GameObject.Find("KinectReference").GetComponent<Camera>();
        if (!trakedBodyParent) trakedBodyParent = GameObject.Find("User_PositionRig");
        updateBoundPositions();
    }
	
	// Update is called once per frame
	public void updateBoundPositions()
    {
        if (!trakedBodyParent) return;
        if (!boundBox) return;
        if (!mainCamera) return;

        var originalPos = trakedBodyParent.gameObject.transform.position;

        var horizontalBound = originalPos;
        horizontalBound.x += 1.4f;
        horizontalBound = mainCamera.WorldToScreenPoint(horizontalBound);

        var verticalBound = originalPos;
        verticalBound.y += 1f;
        verticalBound = mainCamera.WorldToScreenPoint(verticalBound);

        var origin = mainCamera.WorldToScreenPoint(originalPos);

        boundBox.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(((horizontalBound.x - origin.x) * 2),( verticalBound.y - origin.y)*2);

        origin = originalPos;
        origin.y -= .4f;
        origin = mainCamera.WorldToScreenPoint(origin);
        boundBox.transform.position = origin;

        Debug.Log("made it here");
    }
}
