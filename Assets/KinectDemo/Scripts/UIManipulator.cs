using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManipulator : MonoBehaviour {

    public GameObject trophyPrefab;

    // Update is called once per frame
    void Update ()
    {

        if (Input.GetKeyDown(KeyCode.R))
        {
            var newTrophy = Instantiate(trophyPrefab);
            var temp = newTrophy.transform.position;
            temp.z = 3f;
            newTrophy.transform.position = temp;
        }
        //Vector3 boxPos = myPrimaryCamera.WorldToScreenPoint(myCube.transform.position);

        //if (mouseCursor == null)
        //{
        //    mouseCursor = GameObject.Find("MouseCursor");
        //}

        //if (backButton == null )
        //{
        //    backButton = GameObject.Find("Back Button");
        //}

        //if (myHand == null)
        //{
        //    myHand = GameObject.FindObjectOfType<HandBehaviour>();
        //}

        //if (myHand != null && mouseCursor != null)
        //{
        //    mouseCursor.transform.position = myPrimaryCamera.WorldToScreenPoint(myHand.transform.position);
        //}
        //if (backButton != null)
        //{
        //    if (backButton.GetComponent<Collider2D>().IsTouching(mouseCursor.GetComponent<Collider2D>()))
        //    {
        //        myButton.gameObject.GetComponent<Image>().color = Color.red;
        //    }
        //    else
        //    {
        //        myButton.gameObject.GetComponent<Image>().color = Color.blue;
        //    }
        //}


        //myButton.transform.position = myPrimaryCamera.WorldToScreenPoint(myCube.transform.position);

        //Debug.Log("my pos" + myPrimaryCamera.WorldToScreenPoint(myCube.transform.position));

        //Debug.Log("This is the screeen space position for MiniMap Button: " + buttonPos);
        //Debug.Log("This is the screeen space position for Cube: " + boxPos);
        //if (boxPos.x < (myButton.transform.position.x + 100) && boxPos.x > (myButton.transform.position.x - 100) && boxPos.y < (myButton.transform.position.y + 100) && boxPos.y > (myButton.transform.position.y - 100))
        //{
        //    myButton.gameObject.GetComponent<Image>().color = Color.red;
        //}
        //else
        //{
        //    myButton.gameObject.GetComponent<Image>().color = Color.blue;
        //}

    }
}
