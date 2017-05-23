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

    //init user gameobject reference
    private GameObject UserRigidBody;

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
    public float zoomRatio = 30f;

    //init Rect references
    public Rect Main_Display = new Rect ( 0f, 0f, 1f, 1f );
    public Rect Perspective_1;
    public Rect Perspective_2;
    public Rect MM;

    //Int value to keep track of current camera
    //1 = XZ  2 = XY  3 = ZY
    private int CurrentCamera = 1;
    private int SwapCamera_1 = 2;
    private int SwapCamera_2 = 3;

    //Get reference to Camera Rigid Body
    
    private Vector3 CursorPosition;

    //init grab bool (mouse click)
    private bool grab = false;
    public Image[] allHands = new Image[2];
    public Rect[] allButtons = new Rect[3];
    public int SelectedButton;
    public int SelectedHand;
    public int CountDown;
    public Image[] interactableButtons = new Image[3];

    public bool lockCameraPosition = false;
    public int lockCameraPos = 1;

    private Button FrontButton;
    private Button ThirdPersonButton;
    private Button LockButton;
    
    struct Hand
    {
        public bool grab;
        public Vector3 CursorPosition;
        public GameObject CurrentObject;
        public Vector3 Movement;
    }

    Hand rightHand;
    Hand leftHand;

    Image[] I;
    Image Left;
    Image Right;
    bool leftInput = false;
    bool rightInput = false;

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
        MM = new Rect(0f, .75f, ScreenHeight / ScreenWidth * .25f, .25f);

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

        for(int i = 0; i < ButtonArray.Length; i++)
        {
            if (ButtonArray[i].name == "Front Camera")
                FrontButton = ButtonArray[i];
            if (ButtonArray[i].name == "Third Person")
                ThirdPersonButton = ButtonArray[i];
            if (ButtonArray[i].name == "Lock Camera")
                LockButton = ButtonArray[i];
        }

        LockButton.GetComponent<RectTransform>().sizeDelta = new Vector2((ScreenHeight * .15f), ScreenHeight * .15f);
        LockButton.GetComponent<RectTransform>().position = new Vector3(ScreenWidth * .21f, ScreenHeight * .1f);

        ThirdPersonButton.GetComponent<RectTransform>().sizeDelta = new Vector2((ScreenHeight * .15f), ScreenHeight * .15f);
        ThirdPersonButton.GetComponent<RectTransform>().position = new Vector3(ScreenWidth * .13f, ScreenHeight * .1f);

        FrontButton.GetComponent<RectTransform>().sizeDelta = new Vector2((ScreenHeight * .15f), ScreenHeight * .15f);
        FrontButton.GetComponent<RectTransform>().position = new Vector3(ScreenWidth * .05f, ScreenHeight *.1f);

        ThirdPersonButton.gameObject.SetActive(false);
        FrontButton.gameObject.SetActive(false);

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

        UserRigidBody = GameObject.Find("User_PositionRig");
        UserRigidBody.transform.position = GameObject.Find("BodySourceManager").transform.position;
        UserRigidBody.transform.rotation = GameObject.Find("BodySourceManager").transform.rotation;

        //disable the rigidbody
        CameraRigidBody.SetActive(false);
        UserRigidBody.GetComponent<MeshRenderer>().enabled = false;

        //set camera positions to the kinect camera
        YZ_Camera.transform.position = new Vector3(X_Zoom + CameraRigidBody.transform.position.x, CameraRigidBody.transform.position.y, CameraRigidBody.transform.position.z);
        XZ_Camera.transform.position = new Vector3(CameraRigidBody.transform.position.x, Y_Zoom + CameraRigidBody.transform.position.y, CameraRigidBody.transform.position.z);
        XY_Camera.transform.position = new Vector3(CameraRigidBody.transform.position.x, CameraRigidBody.transform.position.y, Z_Zoom + CameraRigidBody.transform.position.z);

        I = PosCanvas.GetComponentsInChildren<Image>();
        for(int i = 0; i < I.Length; i++)
        {
            if (I[i].name == "MouseCursorR")
            {
                Right = I[i];
            }
            else
            {
                Left = I[i];
            }
        }

        Image[] allHands = new Image[2];
        allHands[0] = Right;
        allHands[1] = Left;

        Rect[] allButtons = new Rect[3];
        allButtons[0] = Perspective_1;
        allButtons[1] = Perspective_2;
        allButtons[2] = MM;

        interactableButtons = new Image[3];

        SelectedButton = -1;
        CountDown = 100;
        //disable position ui
        PosCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update () {
        //if positional ui is active
        if (PosCanvasIsActive == true)
        {
            //update cursor position
            //CursorPosition = Input.mousePosition;   
            updateLeft();
            updateRight();

            updateUI();
            ImageUpdate();

            //Debug.LogError(rightHand.CurrentObject.name);

            //Debug.LogError(leftHand.CurrentObject.name);

            //update rotation and zoom based on sliders
            ZoomCamera();

            //update kinect position and rotation
            Kinect.transform.position = CameraRigidBody.transform.position;
            Kinect.transform.rotation = CameraRigidBody.transform.rotation;

            //// I have to do this becuse I loose all the references in start and button switch
            //GameObject[] allHands = new GameObject[2];
            //allHands[0] = GameObject.Find("MouseCursorR");
            //allHands[1] = GameObject.Find("MouseCursorL");
            //Rect[] allButtons = new Rect[3];
            //allButtons[0] = MM;
            //allButtons[1] = Perspective_1;
            //allButtons[2] = Perspective_2;      

            //for (int j = 0; j < allHands.Length; j++)
            //{
            //    var hand = allHands[j];
            //    var Xval = hand.gameObject.transform.position.x / ScreenWidth;
            //    var Yval = hand.gameObject.transform.position.y / ScreenHeight;
            //    for (int i = 0; i < allButtons.Length; i++)
            //    {
            //        var currRect = allButtons[i];
            //        if (Xval > currRect.x && Xval < (currRect.x + currRect.width) && Yval > currRect.y && Yval < (currRect.y + currRect.height))
            //        {
            //            Debug.Log("In Box");
            //            SelectedHand = j;
            //            SelectedButton = i;
            //            ButtonArray[i].gameObject.GetComponent<Image>().color = Color.green;
            //            CountDown--;
            //            break;
            //        }
            //        else if ( i == SelectedButton && SelectedHand == j)
            //        {
            //            ButtonArray[i].gameObject.GetComponent<Image>().color = Color.white;
            //            SelectedButton = -1;
            //            CountDown = 100;
            //        }
            //    }
            //}

            //// timer runs out the button is clicked
            //if (CountDown < 0)
            //{
            //    CountDown = 200;
            //    if (SelectedButton == 0) Close();
            //    if (SelectedButton == 1) Swap(1);
            //    if (SelectedButton == 2) Swap(2);
            //}

            
            //init perspective Rect variables
            Perspective_1 = new Rect(.85f, .58f, (ScreenHeight / ScreenWidth * .25f), .25f);
            Perspective_2 = new Rect(.85f, .18f, (ScreenHeight / ScreenWidth * .25f), .25f);
            MM = new Rect(0f, .75f, ScreenHeight / ScreenWidth * .25f, .25f);

            if (lockCameraPosition == true)
            {
                if (lockCameraPos == 1)
                {
                    CameraRigidBody.transform.position = UserRigidBody.transform.FindChild("Front_Camera_Position").transform.position;

                    CameraRigidBody.transform.LookAt(UserRigidBody.transform);
                }
                else if (lockCameraPos == 2)
                {
                    CameraRigidBody.transform.position = UserRigidBody.transform.FindChild("ThirdPerson_Camera_Position").transform.position;

                    CameraRigidBody.transform.LookAt(UserRigidBody.transform);
                }
                else if(lockCameraPos == 3)
                {
                    //todo insert first person
                }
            }
        }
	}

    public void CameraPosition(int i)
    {
        lockCameraPos = i;
    }

    public bool getCameraLock()
    {
        return lockCameraPosition;
    }

    public void LockCameraToggle()
    {
        if (lockCameraPosition == true)
        {
            lockCameraPosition = false;
            
            ThirdPersonButton.gameObject.SetActive(false);
            FrontButton.gameObject.SetActive(false);
        }
        else
        {
            lockCameraPosition = true;
            ThirdPersonButton.gameObject.SetActive(true);
            FrontButton.gameObject.SetActive(true);
        }
    }

    void MovePosition (Hand hand)
    {
        //reduce movent to a more realistic number
        //Movement = Movement / 100;

        //if top down camera
        if (CurrentCamera == 1)
        {
            Vector3 v3 = hand.CursorPosition;
            v3.z = XZ_Camera.transform.position.y - hand.CurrentObject.transform.position.y;
            v3 = XZ_Camera.ScreenToWorldPoint(v3);

            hand.CurrentObject.transform.position = v3;

            //update rigid body position X and Z position
            //move other cameras the same amount to avoid losing the object
            YZ_Camera.transform.position = new Vector3(YZ_Camera.transform.position.x, hand.CurrentObject.transform.position.y, hand.CurrentObject.transform.position.z);
            XY_Camera.transform.position = new Vector3(hand.CurrentObject.transform.position.x, hand.CurrentObject.transform.position.y, XY_Camera.transform.position.z);
        }
        //if front view camera
        if (CurrentCamera == 2)
        {
            Vector3 v3 = hand.CursorPosition;
            v3.z = XY_Camera.transform.position.z - hand.CurrentObject.transform.position.z;
            v3 = XY_Camera.ScreenToWorldPoint(v3);

            hand.CurrentObject.transform.position = v3;

            //update X and Y positions
            //move other cameras the same amount to avoid losing the object
            YZ_Camera.transform.position = new Vector3(YZ_Camera.transform.position.x, hand.CurrentObject.transform.position.y, hand.CurrentObject.transform.position.z);
            XZ_Camera.transform.position = new Vector3(hand.CurrentObject.transform.position.x, XZ_Camera.transform.position.y, hand.CurrentObject.transform.position.z);
        }
        //if side view camera
        if (CurrentCamera == 3)
        {
            Vector3 v3 = hand.CursorPosition;
            v3.z = YZ_Camera.transform.position.x - hand.CurrentObject.transform.position.x;
            v3 = YZ_Camera.ScreenToWorldPoint(v3);

            hand.CurrentObject.transform.position = v3;

            //update Y and Z positions
            //move other cameras the same amount to avoid losing the object
            XZ_Camera.transform.position = new Vector3(hand.CurrentObject.transform.position.x, XZ_Camera.transform.position.y, hand.CurrentObject.transform.position.z);
            XY_Camera.transform.position = new Vector3(hand.CurrentObject.transform.position.x, hand.CurrentObject.transform.position.y, XY_Camera.transform.position.y);
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

    public void ZoomCamera(/*float num*/)
    {
        
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
        UserRigidBody.GetComponent<MeshRenderer>().enabled = true;
        //disable main UI
        Main_UI.SetActive(false);
        //disable kinect view
        Kinect.depth = -10f;
        //disable minimap view
        MinimapCamera.enabled = false;

        var firstHand = FindObjectOfType<MouseCursor>();
        if ( firstHand != null )
        {
            firstHand.setActive(true);
        }
        else
        {
            Debug.LogError("Hand Object Not Found :/");
        }

        //Image[] allHands = new Image[2];
        //allHands[0] = GameObject.Find("MouseCursorR").GetComponent<Image>();
        //allHands[1] = GameObject.Find("MouseCursorR").GetComponent<Image>();

        //Rect[] allButtons = new Rect[3];
        //allButtons[0] = Perspective_1;
        //allButtons[1] = Perspective_2;
        //allButtons[2] = MM;
    }

    public void Close ()
    {
        var firstHand = FindObjectOfType<MouseCursor>();
        if (firstHand != null)
        {
            firstHand.setActive(false);
        }
        else
        {
            Debug.LogError("Hand Object Not Found :/");
        }

        //disable position UI
        PosCanvas.SetActive(false);
        PosCanvasIsActive = false;
        //Disable rigidbody
        CameraRigidBody.SetActive(false);
        UserRigidBody.GetComponent<MeshRenderer>().enabled = false;
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
            }
            else if (SwapCamera_1 == 2) //else if the camera is the front view
            {
                //change front view to main display
                XY_Camera.rect = Main_Display;
                Current.depth = PerspectiveDepths;
                Current = XY_Camera;
                XY_Camera.depth = Main_Depth;
            }
            else if (SwapCamera_1 == 3) //else if the camera is the side view
            {
                //change side view to the main display
                YZ_Camera.rect = Main_Display;
                Current.depth = PerspectiveDepths;
                Current = YZ_Camera;
                YZ_Camera.depth = Main_Depth;
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
            }
            else if (SwapCamera_2 == 2)
            {
                //swap front camera to main display
                XY_Camera.rect = Main_Display;
                Current.depth = PerspectiveDepths;
                Current = XY_Camera;
                XY_Camera.depth = Main_Depth;
            }
            else if (SwapCamera_2 == 3)
            {
                //swap side camera to main display
                YZ_Camera.rect = Main_Display;
                Current.depth = PerspectiveDepths;
                Current = YZ_Camera;
                YZ_Camera.depth = Main_Depth;
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

    void updateRight()
    {
        if (rightInput)//Input.GetMouseButton(0))
        {
            //if first update since grab
            if (rightHand.grab == false)
            {
                //raycast to cursor location
                Ray R = Current.ScreenPointToRay(Right.transform.position);
                RaycastHit hit;

                Physics.Raycast(R, out hit, 1000);

                //check what was hit and return the value
                if (hit.collider != null)
                {
                    rightHand.CurrentObject = GameObject.Find(hit.collider.name);
                    if (rightHand.CurrentObject.name != CameraRigidBody.name && rightHand.CurrentObject.name != UserRigidBody.name)
                        rightHand.CurrentObject = null;
                }
                else //else hit nothing and return null
                    rightHand.CurrentObject = null;

                //update cursor position
                rightHand.CursorPosition = Right.transform.position;

                //set grab to true
                rightHand.grab = true;
            }
            else //else not first grab update
            {
                //get difference since last update in position
                rightHand.Movement = Right.transform.position - rightHand.CursorPosition;
            }
            
        }
        else //else not in grab this update, set to false;]
        {
            rightHand.grab = false;
            rightHand.CurrentObject = null;
        }

        //update cursor position
        rightHand.CursorPosition = Right.transform.position;
    }

    void updateLeft()
    {
        if (leftInput)//Input.GetMouseButton(0))
        {
            //if first update since grab
            if (leftHand.grab == false)
            {
                //raycast to cursor location
                Ray R = Current.ScreenPointToRay(Left.transform.position);
                RaycastHit hit;

                Physics.Raycast(R, out hit, 1000);

                //check what was hit and return the value
                if (hit.collider != null)
                {
                    leftHand.CurrentObject = GameObject.Find(hit.collider.name);
                    if (leftHand.CurrentObject.name != CameraRigidBody.name && leftHand.CurrentObject.name != UserRigidBody.name)
                        leftHand.CurrentObject = null;
                }
                else //else hit nothing and return null
                    leftHand.CurrentObject = null;

                //update cursor position
                leftHand.CursorPosition = Left.transform.position;//Input.mousePosition;

                //set grab to true
                leftHand.grab = true;
            }
            else //else not first grab update
            {
                //get difference since last update in position
                leftHand.Movement = Left.transform.position - leftHand.CursorPosition;
            }
        }
        else //else not in grab this update, set to false;]
        {
            leftHand.grab = false;
            leftHand.CurrentObject = null;
        }

        //update cursor position
        leftHand.CursorPosition = Left.transform.position;
    }

    void updateUI()
    {
        switch (rightHand.grab) {
            case true:
                switch (leftHand.grab)
                {
                    case true:
                        if (rightHand.CurrentObject != null)
                        {
                            if (rightHand.CurrentObject.name == CameraRigidBody.name || rightHand.CurrentObject.name == UserRigidBody.name)
                            {
                                UpdateRotation(leftHand, rightHand);
                            }
                        }
                        else if (leftHand.CurrentObject != null)
                        {
                            if (leftHand.CurrentObject.name == CameraRigidBody.name || leftHand.CurrentObject.name == UserRigidBody.name)
                            {
                                UpdateRotation(rightHand, leftHand);
                            }
                        }
                        else
                        {
                            UpdateZoom();
                        }
                        break;
                    case false:
                        UpdatePosition(rightHand);
                        break;
                }
                break;
            case false:
                switch (leftHand.grab)
                {
                    case true:
                        UpdatePosition(leftHand);
                        break;
                    case false:
                        break;
                }
                break;
        }
    }

    void UpdateZoom()
    {
        Vector3 MiddleScreen = new Vector3(ScreenWidth / 2, ScreenHeight / 2, leftHand.CursorPosition.z);
        Vector3 LDir = leftHand.CursorPosition - MiddleScreen;
        Vector3 RDir = rightHand.CursorPosition - MiddleScreen;
        Vector3 LMove = leftHand.CursorPosition - leftHand.Movement - MiddleScreen;
        Vector3 RMove = rightHand.CursorPosition - rightHand.Movement - MiddleScreen;

        switch (CurrentCamera)
        {
            case 1:
                Y_Zoom += (LDir.magnitude - LMove.magnitude) / zoomRatio;
                Y_Zoom += (RDir.magnitude - RMove.magnitude) / zoomRatio;
                if (Y_Zoom < 5)
                    Y_Zoom = 5;
                if (Y_Zoom > 20)
                    Y_Zoom = 20;
                break;
            case 2:
                Z_Zoom += (LDir.magnitude - LMove.magnitude) / zoomRatio;
                Z_Zoom += (RDir.magnitude - RMove.magnitude) / zoomRatio;
                if (Z_Zoom < 5)
                    Z_Zoom = 5;
                if (Z_Zoom > 20)
                    Z_Zoom = 20;
                break;
            case 3:
                X_Zoom += (LDir.magnitude - LMove.magnitude) / zoomRatio;
                X_Zoom += (RDir.magnitude - RMove.magnitude) / zoomRatio;
                if (X_Zoom < 5)
                    X_Zoom = 5;
                if (X_Zoom > 20)
                    X_Zoom = 20;
                break;
        }
    }

    void UpdateRotation(Hand hand, Hand otherHand)
    {
        Vector3 v3 = hand.CursorPosition;
        Vector3 v3m = hand.CursorPosition - hand.Movement;

        Vector3 Dir;
        Vector3 Move;
        Vector3 cross;

        float rot;

        switch (CurrentCamera) {
            case 1:
                v3.z = XZ_Camera.transform.position.y - otherHand.CurrentObject.transform.position.y;
                v3 = XZ_Camera.ScreenToWorldPoint(v3);
                v3m.z = XZ_Camera.transform.position.y - otherHand.CurrentObject.transform.position.y;
                v3m = XZ_Camera.ScreenToWorldPoint(v3m);

                Dir = v3 - otherHand.CurrentObject.transform.position;
                Move = v3m - otherHand.CurrentObject.transform.position;

                Dir.Normalize();
                Move.Normalize();

                rot = -Vector3.Angle(Dir, Move);

                cross = Vector3.Cross(Dir, Move);

                if (cross.y < 0) //change direction based on Z value
                {
                    rot = -rot;
                }

                otherHand.CurrentObject.transform.Rotate(0, rot, 0, Space.World);

                break;
            case 2:
                v3.z = XY_Camera.transform.position.z - otherHand.CurrentObject.transform.position.z;
                v3 = XY_Camera.ScreenToWorldPoint(v3);
                v3m.z = XY_Camera.transform.position.z - otherHand.CurrentObject.transform.position.z;
                v3m = XY_Camera.ScreenToWorldPoint(v3m);

                Dir = v3 - otherHand.CurrentObject.transform.position;
                Move = v3m - otherHand.CurrentObject.transform.position;

                Dir.Normalize();
                Move.Normalize();

                rot = -Vector3.Angle(Dir, Move);

                cross = Vector3.Cross(Dir, Move);

                if (cross.z < 0) //change direction based on Z value
                {
                    rot = -rot;
                }

                Debug.LogError(Dir + " " + Move + " " + rot);

                otherHand.CurrentObject.transform.Rotate(0, 0, rot, Space.World);

                break;
            case 3:
                v3.z = YZ_Camera.transform.position.x - otherHand.CurrentObject.transform.position.x;
                v3 = YZ_Camera.ScreenToWorldPoint(v3);
                v3m.z = YZ_Camera.transform.position.x - otherHand.CurrentObject.transform.position.x;
                v3m = YZ_Camera.ScreenToWorldPoint(v3m);

                Dir = v3 - otherHand.CurrentObject.transform.position;
                Move = v3m - otherHand.CurrentObject.transform.position;

                Dir.Normalize();
                Move.Normalize();

                rot = -Vector3.Angle(Dir, Move);

                cross = Vector3.Cross(Dir, Move);

                if (cross.x < 0) //change direction based on Z value
                {
                    rot = -rot;
                }

                otherHand.CurrentObject.transform.Rotate(rot, 0, 0, Space.World);

                break;
        }
    }

    void UpdatePosition(Hand hand)
    {
        //if we hit the rigid body, move the rigid body
        if (hand.CurrentObject != null)
        {
            if (hand.CurrentObject.name == CameraRigidBody.name || hand.CurrentObject.name == UserRigidBody.name)
            {
                MovePosition(hand);
            }
        }
        else //else we hit nothing, move environment
        {
            MoveCamera(hand.Movement);
        }
    }

    float getAngle(Vector3 Dir, Vector3 Move)
    {
        float cosTheta = Vector2.Dot(Dir, Move); //get angle

        Vector3 cross = Vector3.Cross(Move, Dir);

        float rad = Mathf.Acos(cosTheta);  //get radian rotation

        if (cross.z < 0) //change direction based on Z value
        {
            rad = -rad;
        }

        return rad;
    }

    void ImageUpdate()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Right.transform.position = new Vector3(Right.transform.position.x - 2, Right.transform.position.y, Right.transform.position.z);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Right.transform.position = new Vector3(Right.transform.position.x + 2, Right.transform.position.y, Right.transform.position.z);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Right.transform.position = new Vector3(Right.transform.position.x, Right.transform.position.y + 2, Right.transform.position.z);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            Right.transform.position = new Vector3(Right.transform.position.x, Right.transform.position.y - 2, Right.transform.position.z);
        }
        if (Input.GetKey(KeyCode.A))
        {
            Left.transform.position = new Vector3(Left.transform.position.x - 2, Left.transform.position.y, Left.transform.position.z);
        }
        if (Input.GetKey(KeyCode.D))
        {
            Left.transform.position = new Vector3(Left.transform.position.x + 2, Left.transform.position.y, Left.transform.position.z);
        }
        if (Input.GetKey(KeyCode.W))
        {
            Left.transform.position = new Vector3(Left.transform.position.x, Left.transform.position.y + 2, Left.transform.position.z);
        }
        if (Input.GetKey(KeyCode.S))
        {
            Left.transform.position = new Vector3(Left.transform.position.x, Left.transform.position.y - 2, Left.transform.position.z);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (leftInput != true)
            {
                leftInput = true;
                Left.color = Color.cyan;
            }
            else
            {
                leftInput = false;
                Left.color = Color.blue;
            }
        }

        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            if (rightInput != true)
            {
                rightInput = true;
                Right.color = Color.yellow;
            }
            else
            {
                rightInput = false;
                Right.color = Color.red;
            }
        }
    }

    public void toggleLeftHandGrab(bool isGrab)
    {
        leftInput = isGrab;

    }

    public void toggleRightHandGrab(bool isGrab)
    {
        rightInput = isGrab;

    }
}
