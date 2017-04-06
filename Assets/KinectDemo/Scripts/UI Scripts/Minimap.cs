using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour {

    //init game object references
    private GameObject PosCanvas;
    private GameObject Main_UI;
    private GameObject CameraRigidBody;
    private GameObject CurrentObject = null;

    //Bool for UI is active
    private bool PosCanvasIsActive = false;

    //init References to UI camera and buttons
    private Camera XZ_Camera;
    private Camera XY_Camera;
    private Camera YZ_Camera;
    private Camera Current;
    private Camera Kinect;
    private Camera MinimapCamera;

    //init Button References
    private Button Camera_Button_1;
    private Button Camera_Button_2;
    private Button[] ButtonArray = new Button[3];
    private Button MinimapButton;

    //init float references for screen size
    private float ScreenHeight;
    private float ScreenWidth;
    public float Main_Depth = -.5f;
    public float PerspectiveDepths = 0f;

    private float Y_Rotation = 0f;
    private float X_Rotation = 0f;
    private float Z_Rotation = 0f;

    private float Y_Zoom = 10f;
    private float X_Zoom = 10f;
    private float Z_Zoom = 10f;

    //init Rect references
    public Rect Main_Display = new Rect ( 0f, 0f, 1f, 1f );
    public Rect Perspective_1;
    public Rect Perspective_2;

    //Int value to keep track of current camera
    //1 = XZ  2 = XY  3 = ZY
    private int CurrentCamera = 1;
    private int SwapCamera_1 = 2;
    private int SwapCamera_2 = 3;

    //Get reference to Camera Rigid Body
    
    private Vector3 CursorPosition;

    
    //init Slider References
    private Slider[] PositionSliders = new Slider[2];

    //init grab bool (mouse click)
    private bool grab = false;

    // Use this for initialization
    void Start () {

        //get Position UI gameobject
        PosCanvas = GameObject.Find("Position_UI");

        //get Main UI gameobject
        Main_UI = GameObject.Find("Main UI");

        //go through all cameras and find specific cameras
        foreach (Camera c in Camera.allCameras)
        {
            //get positional cameras
            if (c.gameObject.name == "XZ Camera")
                XZ_Camera = c;
            if (c.gameObject.name == "YZ Camera")
                YZ_Camera = c;
            if (c.gameObject.name == "XY Camera")
                XY_Camera = c;
            //get Kinect main Camera
            if (c.gameObject.name == "KinectReference")
                Kinect = c;
            //get minimap camera
            if (c.gameObject.name == "Minimap Camera")
                MinimapCamera = c;
        }

        //init screen size variables
        ScreenHeight = (float)Screen.height;
        ScreenWidth = (float)Screen.width;        

        //init perspective Rect variables
        Perspective_1 = new Rect(.85f, .58f, (ScreenHeight / ScreenWidth * .25f), .25f);
        Perspective_2 = new Rect(.85f, .18f, (ScreenHeight / ScreenWidth * .25f), .25f);
        Rect MM = new Rect(0f, .75f, ScreenHeight / ScreenWidth * .25f, .25f);

        //init cameras to specific rects
        XZ_Camera.rect = Main_Display;
        XY_Camera.rect = Perspective_1;
        YZ_Camera.rect = Perspective_2;
        MinimapCamera.rect = MM;

        //get button array of all buttons under positional ui
        ButtonArray = PosCanvas.GetComponentsInChildren<Button>();

        //get rect of all buttons
        RectTransform r0 = ButtonArray[0].GetComponent<RectTransform>();
        RectTransform r1 = ButtonArray[1].GetComponent<RectTransform>();
        RectTransform r2 = ButtonArray[2].GetComponent<RectTransform>();

        //get minimap button ui
        MinimapButton = Main_UI.GetComponentInChildren<Button>();

        //get minimap button rect
        RectTransform r3 = MinimapButton.GetComponent<RectTransform>();

        //change return button to top left of screen
        r0.sizeDelta = new Vector2((ScreenHeight * .255f), ScreenHeight * .255f);
        r0.transform.position = new Vector3(ScreenHeight * .255f / 2, ScreenHeight - ScreenHeight * .255f / 2);

        //change swap 1 button to right side of the screen
        r1.sizeDelta = new Vector2((ScreenHeight * .255f), ScreenHeight * .255f);
        r1.transform.position = new Vector3(ScreenWidth * .85f + ScreenHeight * .25f / 2, ScreenHeight * .58f + ScreenHeight * .25f / 2);

        //change swap 2 button to right side of the screen
        r2.sizeDelta = new Vector2((ScreenHeight * .255f), ScreenHeight * .255f);
        r2.transform.position = new Vector3(ScreenWidth * .85f + ScreenHeight * .25f / 2, ScreenHeight * .18f + ScreenHeight * .25f / 2);

        //set minimap button to the top left of the scrren
        r3.sizeDelta = new Vector2((ScreenHeight * .255f), ScreenHeight * .255f);
        r3.transform.position = new Vector3(ScreenHeight * .255f / 2, ScreenHeight - ScreenHeight * .255f / 2);

        //init current camera to top down
        Current = XZ_Camera;

        //get rigid body for camera maniputlation and set it to camera position and rotation
        CameraRigidBody = GameObject.Find("MainCamera_RigidBody");
        CameraRigidBody.transform.position = Kinect.transform.position;
        CameraRigidBody.transform.rotation = Kinect.transform.rotation;

        //disable the rigidbody
        CameraRigidBody.SetActive(false);

        //get all sliders in the position UI
        PositionSliders = PosCanvas.GetComponentsInChildren<Slider>();

        //set screen location for the sliders
        PositionSliders[0].transform.position = new Vector3(ScreenWidth * .5f, ScreenHeight * .05f);
        PositionSliders[1].transform.position = new Vector3(ScreenWidth * .06f, ScreenHeight * .4f);

        //set camera positions to the kinect camera
        YZ_Camera.transform.position = new Vector3(X_Zoom + CameraRigidBody.transform.position.x, CameraRigidBody.transform.position.y, CameraRigidBody.transform.position.z);
        XZ_Camera.transform.position = new Vector3(CameraRigidBody.transform.position.x, Y_Zoom + CameraRigidBody.transform.position.y, CameraRigidBody.transform.position.z);
        XY_Camera.transform.position = new Vector3(CameraRigidBody.transform.position.x, CameraRigidBody.transform.position.y, Z_Zoom + CameraRigidBody.transform.position.z);

        //disable position ui
        PosCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update () {
        //if positional ui is active
        if (PosCanvasIsActive == true)
        {
            //if user grabs (clicks)
            if (Input.GetMouseButton(0))
            {
                //if first update since grab
                if (grab == false)
                {
                    //raycast to cursor location
                    Ray R = Current.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    Physics.Raycast(R, out hit, 1000);

                    //check what was hit and return the value
                    if (hit.collider != null)
                    {
                        CurrentObject = GameObject.Find(hit.collider.name);
                        if (CurrentObject.name != CameraRigidBody.name)
                            CurrentObject = null;
                    }
                    else //else hit nothing and return null
                        CurrentObject = null;

                    //update cursor position
                    CursorPosition = Input.mousePosition;

                    //set grab to true
                    grab = true;
                }
                else //else not first grab update
                {
                    //get difference since last update in position
                    Vector3 Movement = Input.mousePosition - CursorPosition;

                    //if we hit the rigid body, move the rigid body
                    if (CurrentObject == CameraRigidBody)
                    {
                        MovePosition(Movement);
                    }
                    else if (CurrentObject == null) //else we hit nothing, move environment
                    {
                        MoveCamera(Movement);
                    }

                }

            }
            else //else not in grab this update, set to false;]
            {
                grab = false;
                CurrentObject = null;
            }

            //update cursor position
            CursorPosition = Input.mousePosition;   

            //update rotation and zoom based on sliders
            RotatePosition(PositionSliders[0].value);
            ZoomCamera(PositionSliders[1].value);

            //update kinect position and rotation
            Kinect.transform.position = CameraRigidBody.transform.position;
            Kinect.transform.rotation = CameraRigidBody.transform.rotation;
        }
	}

    void MovePosition (Vector3 Movement)
    {
        //reduce movent to a more realistic number
        Movement = Movement / 100;

        //if top down camera
        if (CurrentCamera == 1)
        {
            //update rigid body position X and Z position
            CameraRigidBody.transform.position = CameraRigidBody.transform.position + new Vector3(Movement[0], 0, Movement[1]);
            //move other cameras the same amount to avoid losing the object
            YZ_Camera.transform.position = YZ_Camera.transform.position + new Vector3(Movement[0], 0, Movement[1]);
            XY_Camera.transform.position = XY_Camera.transform.position + new Vector3(Movement[0], 0, Movement[1]);
        }
        //if front view camera
        if (CurrentCamera == 2)
        {
            //update X and Y positions
            CameraRigidBody.transform.position = CameraRigidBody.transform.position + new Vector3(-Movement[0], Movement[1], 0);
            //move other cameras the same amount to avoid losing the object
            YZ_Camera.transform.position = YZ_Camera.transform.position + new Vector3(-Movement[0], Movement[1], 0);
            XZ_Camera.transform.position = XZ_Camera.transform.position + new Vector3(-Movement[0], Movement[1], 0);
        }
        //if side view camera
        if (CurrentCamera == 3)
        {
            //update Y and Z positions
            CameraRigidBody.transform.position = CameraRigidBody.transform.position + new Vector3(0, Movement[1], Movement[0]);
            //move other cameras the same amount to avoid losing the object
            XZ_Camera.transform.position = XZ_Camera.transform.position + new Vector3(0, Movement[1], Movement[0]);
            XY_Camera.transform.position = XY_Camera.transform.position + new Vector3(0, Movement[1], Movement[0]);
        }
    }

    public void RotatePosition(float num)
    {
        //if top down view rotate Y
        if (CurrentCamera == 1)
        {
            Y_Rotation = num;
        }
        //if front view rotate Z
        if (CurrentCamera == 2)
        {
            Z_Rotation = num;
        }
        //if side view rotate X
        if (CurrentCamera == 3)
        {
            X_Rotation = num;
        }
        //update Rigidbody rotation
        CameraRigidBody.transform.rotation = Quaternion.Euler(X_Rotation, Y_Rotation, Z_Rotation);
    }

    public void ZoomCamera(float num)
    {
        //update top down zoom
        if (CurrentCamera == 1)
        {
            Y_Zoom = num;
        }
        //update front view zoom
        if (CurrentCamera == 2)
        {
            Z_Zoom = num;       
        }
        //update side view zoom
        if (CurrentCamera == 3)
        {
            X_Zoom = num; 
        }
        //update camera position based on rigid body position
        XZ_Camera.transform.position = new Vector3(XZ_Camera.transform.position[0], Y_Zoom + CameraRigidBody.transform.position.y, XZ_Camera.transform.position[2]);
        XY_Camera.transform.position = new Vector3(XY_Camera.transform.position[0], XY_Camera.transform.position[1], Z_Zoom + CameraRigidBody.transform.position.z);
        YZ_Camera.transform.position = new Vector3(X_Zoom + CameraRigidBody.transform.position.x, YZ_Camera.transform.position[1], YZ_Camera.transform.position[2]);
    }

    void MoveCamera (Vector3 Movement)
    {
        //reduce movement to a realistic value
        Movement = Movement / 100;

        //update camera position based on current view
        if (CurrentCamera == 1)
            XZ_Camera.transform.position = XZ_Camera.transform.position - new Vector3(Movement[0], 0, Movement[1]);
        if (CurrentCamera == 2)
            XY_Camera.transform.position = XY_Camera.transform.position - new Vector3(-Movement[0], Movement[1], 0);
        if (CurrentCamera == 3)
            YZ_Camera.transform.position = YZ_Camera.transform.position - new Vector3(0, Movement[1], Movement[0]);
    }

    public void Open ()
    {
        //enable position UI
        PosCanvas.SetActive(true);
        PosCanvasIsActive = true;

        //set rigid body to active
        CameraRigidBody.SetActive(true);
        //disable main UI
        Main_UI.SetActive(false);
        //disable kinect view
        Kinect.depth = -10f;
        //disable minimap view
        MinimapCamera.enabled = false;
    }

    public void Close ()
    {
        //disable position UI
        PosCanvas.SetActive(false);
        PosCanvasIsActive = false;
        //Disable rigidbody
        CameraRigidBody.SetActive(false);
        //enable main ui
        Main_UI.SetActive(true);
        //enable kinect camera
        Kinect.depth = 0f;
        //enable minimap
        MinimapCamera.enabled = true;
    }

    public void Swap(int num)
    {
        //if top swap  camera
        if (num == 1)
        {
            //set current view rect to perspective_1
            Current.rect = Perspective_1;
            //if the camera we are swapping to is the top down view
            if(SwapCamera_1 == 1)
            {
                //change top down view to main display rect
                XZ_Camera.rect = Main_Display;
                Current.depth = PerspectiveDepths;
                Current = XZ_Camera;
                XZ_Camera.depth = Main_Depth;

                //update position sliders
                PositionSliders[0].value = Y_Rotation;
                PositionSliders[1].value = XZ_Camera.transform.position[1];
            }
            else if (SwapCamera_1 == 2) //else if the camera is the front view
            {
                //change front view to main display
                XY_Camera.rect = Main_Display;
                Current.depth = PerspectiveDepths;
                Current = XY_Camera;
                XY_Camera.depth = Main_Depth;

                //update position sliders
                PositionSliders[0].value = Z_Rotation;
                PositionSliders[1].value = XY_Camera.transform.position[2];
            }
            else if (SwapCamera_1 == 3) //else if the camera is the side view
            {
                //change side view to the main display
                YZ_Camera.rect = Main_Display;
                Current.depth = PerspectiveDepths;
                Current = YZ_Camera;
                YZ_Camera.depth = Main_Depth;

                //update position sliders
                PositionSliders[0].value = X_Rotation;
                PositionSliders[1].value = YZ_Camera.transform.position[0];
            }
            else //else error
            {
                Debug.LogError("SwapCamera_1 is not an expected value");
                return;
            }

            //swap int values of currentcamera and swapcamera_1
            int temp = CurrentCamera;
            CurrentCamera = SwapCamera_1;
            SwapCamera_1 = temp;
    
        }
        if (num == 2)
        {
            Current.rect = Perspective_2;

            if (SwapCamera_2 == 1)
            {
                //swap top down camera to main display
                XZ_Camera.rect = Main_Display;
                Current.depth = PerspectiveDepths;
                Current = XZ_Camera;
                XZ_Camera.depth = Main_Depth;

                //update position sliders
                PositionSliders[0].value = Y_Rotation;
                PositionSliders[1].value = XZ_Camera.transform.position[1];
            }
            else if (SwapCamera_2 == 2)
            {
                //swap front camera to main display
                XY_Camera.rect = Main_Display;
                Current.depth = PerspectiveDepths;
                Current = XY_Camera;
                XY_Camera.depth = Main_Depth;

                //update position sliders
                PositionSliders[0].value = Z_Rotation;
                PositionSliders[1].value = XY_Camera.transform.position[2];
            }
            else if (SwapCamera_2 == 3)
            {
                //swap side camera to main display
                YZ_Camera.rect = Main_Display;
                Current.depth = PerspectiveDepths;
                Current = YZ_Camera;
                YZ_Camera.depth = Main_Depth;

                //update position sliders
                PositionSliders[0].value = X_Rotation;
                PositionSliders[1].value = YZ_Camera.transform.position[0];
            }
            else
            {
                Debug.LogError("SwapCamera_2 is not an expected value");
                return;
            }

            //swap int values of currentcamera and swapcamera_2
            int temp = CurrentCamera;
            CurrentCamera = SwapCamera_2;
            SwapCamera_2 = temp;
        }
    }

}
