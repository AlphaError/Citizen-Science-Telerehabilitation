﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using OVRTouchSample;
using System;
using UnityEngine.UI;
using System.Runtime.Remoting.Messaging;
//using UnityEditor.Build.Content;

public class VRUser : MonoBehaviour
{
    //main stuff
    public StateManager state;

    //main controls
    public GameObject VRPerson;
    public OVRInput.Controller playerController;
    //public OVRCameraRig playerHead;
    public GameObject playerHead;
    public GameObject playerArms;
    public UserBounds playerPos;

    public static Color nothing = new Color(1, 1, 1, 0);
    public static Color selectedColor;
    public static Color highlightColor = Color.magenta; // new Color(1, 253/255, 126/255, 1);

    public static GameObject cursorPos;
    //TODO fix tutorial text coloring
    public static Color cursorHighlight = Color.blue;//new Color(88/255, 6/255, 140/255, 159/255);//HomeScreen.nyuPurple;
    public static Color cursorHighlight2 = Color.green;//Color.red;
    public static Color cursorHighlight3 = Color.yellow; // Color.magenta;
    public static Color tagColor = new Color(1, 1, 1, 101 / 255);
    public static Color binColor = new Color(134 / 255, 150 / 255, 167 / 255, 186 / 255);
    public static Color binColor2 = new Color(167 / 255, 134 / 255, 143 / 255, 186 / 255);
    public static Color binUnhighlight = new Color(167 / 255, 167 / 255, 167 / 255, 167 / 255);

    public static GameObject trueCursor;

    public static GameObject centerer; //centering cursor
    public static GameObject farUp; //suplimental coordinate system for controllers
    public static GameObject farRight;
    public static GameObject farForward;

    //revised true cursor bounds** - y[-67,55], x[-79,87]
    public static float cursorBoundUp = 55f;
    public static float cursorBoundDown = -67f;
    public static float cursorBoundLeft = -79f;
    public static float cursorBoundRight = 87f;

    public static bool extraControls = false; //for keyboard controls and other developer stuff
    private static bool buildExclusiveFunc = true; //bool to set elements of code active (to be set true on build and false otherwise)
    /* Fixes some unknown issues that exist between build and the Unity Editor versions (especially with tag tracking) :( */

    public static List<GameObject> interactables = new List<GameObject>();
    public static Vector3 uiButtonOffset = new Vector3(-5f, 0f, 0f); //offset needed for button accuracy with uiButton methods within clickaction

    public static Vector3 change; //modified change of controller movement

    public static bool showMoveStats = false; //bool to debug.log calibration stats

    public static float moveThreshold1 = 0.20f; //percentages for (1)reading movement & (2)displaying movement (+haptics)
    public static float moveThreshold2 = 0.75f; //placeholder values*** - ref UserInfo.updateDifficulty()

    public static float baseZCalibration = 1.1f; //var that signifies how far the user is supposed to reach (z) given no calibration data

    public float totalTime = 0f;

    public static float controllerVibration = 0f;

    public static bool specialClick = false; //for tutorial showing...

    public static GameObject clickColor;
    public static GameObject clickLock;
    private static Color showLock;
    private static Color hideLock;
    public static bool noLock = false;

    public static bool forceLock = false;

    public static bool isRightHanded = true; //static refrence for user's dominant hand (within UserInfo class)
    //Note: True = using right touch / secondary controller, False = left touch / primary controller

    //TODO: maybe fix floating feeling with flatform at user feet (make camera lower, put platform right under, set to floor lvl instead of eye lvl)
    // Start is called before the first frame update
    private void Awake()
    { //Ctrl+K+C = comment (+K+U = uncomment)
        //(general player stuff) player head and controllers set within Unity Scene (VRPerson's children)
        VRPerson = GameObject.Find("VRPerson");
        playerHead = GameObject.Find("CenterEyeAnchor");
        state = GameObject.Find("Canvas").GetComponent<StateManager>();
        cursorPos = GameObject.Find("CursorSphere");
        trueCursor = GameObject.Find("exampleCursor");
        playerArms = GameObject.Find("arms");
        playerPos = new UserBounds(playerHead.transform.position - new Vector3(0f,3f,0f), playerHead.transform.position);

        //headset anchors
        centerer = GameObject.Find("cursorCenter");
        farUp = GameObject.Find("headsetUp");
        farRight = GameObject.Find("headsetRight");
        farForward = GameObject.Find("headsetForward");

        //interactables
        interactables.Add(GameObject.Find("NextButtonPanel")); //0
        foreach (GameObject tag in GameObject.FindGameObjectsWithTag("interactableTag"))
        {
            interactables.Add(tag); //1-4 tags
        }
        /*interactables.Add(GameObject.Find("Tag1"));
        interactables.Add(GameObject.Find("Tag2"));
        interactables.Add(GameObject.Find("Tag3"));
        interactables.Add(GameObject.Find("Tag4"));*/
        interactables.Add(GameObject.Find("HomeButtonPanel")); //5
        interactables.Add(GameObject.Find("Bin")); //6
        tagColor = interactables[1].GetComponent<Image>().color; //precausion
        binColor = interactables[6].GetComponent<Image>().color;

        //color stuff
        clickColor = GameObject.Find("showClick");
        clickLock = GameObject.Find("showLock");
        showLock = GameObject.Find("showLock").GetComponent<RawImage>().color;
        hideLock = showLock;
        hideLock.a = 0;
        cursorHighlight.a = 60f / 255f; //unlocking
        cursorHighlight2.a = 40f / 255f; //clicking
        cursorHighlight3.a = 50f / 255f; //locked

        //(init stuff)  threshold val changes
        state.user.updateDifficulty();
        isRightHanded = state.user.getIsRightHanded();
    }

    // Update is called once per frame
    void Update()
    {
        if (state.getState() != 0)
        {
            OVRInput.Update();
            OVRInput.FixedUpdate();

            Debug.Log("SystTime: (1)" + System.DateTime.Now.ToString("HH:mm:ss.fff"));// + "\n(2)" //time checks
            //+ System.DateTime.Now.ToString("hh:mm:ss.") + System.DateTime.Now.Millisecond + "\n(3)" 
            //+ System.DateTime.Now.ToString("hh:mm:ss.ffff"));
            totalTime += Time.deltaTime;
            //Debug.Log("Total Elapsed Time: " + totalTime + ", System: " + System.DateTime.Now.ToString("hh:mm:ss"));
            state.user.addMovement(totalTime, System.DateTime.Now.ToString("HH:mm:ss.fff"), state.userControlActive, playerHead.transform,
                OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand), OVRInput.GetLocalControllerRotation(OVRInput.Controller.RHand).eulerAngles,
                OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand), OVRInput.GetLocalControllerRotation(OVRInput.Controller.LHand).eulerAngles);
            //state.user.moveDataConfirm();

            //vrInfo();
            
            state.user.showMoveBounds(); //Move Data Ex: (29.0044, -29.00861, 13.51058, -14.32116, 0), (3.651338, 6.311625, 4.176139, 7.143209, 0)
            //Debug.Log("[" + (state.userControlActive ? "1":"0") + "]Hand Tracking: " + handTracking(true) + ", Offset By: " + playerPos.arms + ", unfactored: " + handTracking(false));
            Debug.Log("Cont. Clicking: " + state.userIsClicking + ", Clicking: " + state.userClick); //continuous, noncontinuous clicking

            /* movement correction ideas:
             * correct local rotation by normal rotation
             * transform positions/orientations of parent relative to headset roll
             */
            //arms position editing
            //armsFixes(); //testing
            //playerArms.transform.localRotation = Quaternion.Euler( 0f, 0f, -playerHead.transform.rotation.z); //override parent transform rotation?
            //GameObject.Find("headsetForward").transform.position = new Vector3(GameObject.Find("headsetForward").transform.position.x, 0f, GameObject.Find("headsetForward").transform.position.z);
            //GameObject.Find("headsetRight").transform.position -= new Vector3(GameObject.Find("headsetRight").transform.position.x, 0f, GameObject.Find("headsetRight").transform.position.z);
            //GameObject.Find("headsetUp").transform.position -= new Vector3(0f, GameObject.Find("headsetUp").transform.position.y, 0f);

            controllerVibration = 0f;

            //FORCE QUIT
            if (Input.GetKey(KeyCode.Q) && extraControls)
            {
                state.setState(0);
            }

            //UI HIGHLIGHTING
            if (state.isGaming())
            {
                int converted = buttonConversion();
                switch (converted)
                {
                    case 8: //nothing detected
                        interactables[0].GetComponent<Image>().color = new Color(1, 1, 1, 1); //next
                        interactables[1].GetComponent<Image>().color = tagColor; //tags
                        interactables[2].GetComponent<Image>().color = tagColor;
                        interactables[3].GetComponent<Image>().color = tagColor;
                        interactables[4].GetComponent<Image>().color = tagColor;
                        interactables[5].GetComponent<Image>().color = new Color(1, 1, 1, 1); //home
                        interactables[6].GetComponent<Image>().color = binColor;
                        break;
                    case 0: //next
                        interactables[0].GetComponent<Image>().color = highlightColor;
                        //controllerVibration += .15f;
                        break;
                    case 6: // home
                        interactables[5].GetComponent<Image>().color = highlightColor;
                        //controllerVibration += .15f;
                        break;
                    case 7: //trash
                        interactables[6].GetComponent<Image>().color = highlightColor;
                       // controllerVibration += .1f;
                        break;
                    default:
                        //tags (converted 1-4)
                        interactables[converted].GetComponent<Image>().color = highlightColor;
                        //controllerVibration += .1f;
                        break;
                }
            }

            /*  RESET Mechanic:
             *  Cursor starts at the center (cursorCenter) position and cannot move until...
             *  the user presses the isResetting() hand triggers and the user's head/hands pos is taken
             *  the user then can move the cursor relative to the saved vals
             *  the only exception is when the user changes states or the user presses the hand triggers
             * */
            if (((cursorRelock() || state.makeCursReset) && !noLock) || forceLock)
            {
                trueCursor.transform.position = centerer.transform.position;
                state.userControlActive = false;
                state.makeCursReset = false;
                ClickAction.dropObject();
                controllerVibration += .6f;
                if (forceLock)
                {
                    forceLock = false;
                }
            }
            if (isResetting() && !noLock) //user resets cursor via hand triggers
            {
                //sets arm bounds to location of hands
                //playerArms.transform.position = handPos; 
                playerArms.transform.position = handTracking(false);

                //stores coords of arms relative to headset bounds
                //playerPos.arms = new Vector3((farRight.transform.position - handPos).magnitude, (farUp.transform.position - handPos).magnitude, (farForward.transform.position - handPos).magnitude);
                playerPos.arms = handTracking(true);

                //stores present position of head
                playerPos.head = playerHead.transform.position;
                //activates user control
                state.userControlActive = true;
                //resets position of cursor to the center of the user's vision
                trueCursor.transform.position = centerer.transform.position;
                controllerVibration += .2f; //feedback to tell user is being reset
                state.userIsClicking = false;
                state.userClick = false;
                ClickAction.dropObject();
            }
            if (state.userControlActive == false) //saftey
            {
                trueCursor.transform.position = centerer.transform.position;
            }

            //EXTRA (thumbstick) cursor control
            Vector2 cursorMove = new Vector2(0f, 0f); //var to add stick positional stuff to cursor movement
            if (extraControls)
            {
                //add stick controls
                cursorMove = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.Touch) + OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, OVRInput.Controller.Touch);
            }

            //adding hand controls
            if (state.userControlActive && !isResetting())
            {
                change = playerPos.arms - handTracking(true);
                change -= playerPos.head - playerHead.transform.position; //compensatory movement
                Debug.Log("Compensatory Offset: " + (playerPos.head - playerHead.transform.position));
            }
            else
            {
                change = new Vector3(0f, 0f, 0f);
            }

            //set add controller stuff to cursorMove
            Debug.Log("Move Change: " + change + ", Threshold: " + state.user.formattedMoveBounds() + ", " + state.user.formattedMoveBounds(moveThreshold1));
            if (state.user.getPracticeLevelState()[0]) //has started practice level, aka has finished calibration
            { //Note: with the factored handpos, all axis' are reversed* (signs changed)
                //lowerbound threshold - controls when the cursor can move based on a % of the calibration values (x&y)
                if (state.cursorXMove && change.x < (state.user.getMovementBounds(1) * moveThreshold1)) //(-x,x,-y,y,z) (1,2,3,4,5)
                {
                    cursorMove += new Vector2(change.x - (state.user.getMovementBounds(1) * moveThreshold1 / 2f), 0);
                    //cursorMove += new Vector2(change.x, 0);
                    Debug.Log("MoveChange:Left");
                }
                else if (state.cursorXMove && change.x > (state.user.getMovementBounds(2) * moveThreshold1))
                {
                    cursorMove += new Vector2(change.x - (state.user.getMovementBounds(2) * moveThreshold1 / 2f), 0);
                    //cursorMove += new Vector2(change.x, 0);
                    Debug.Log("MoveChange:Right");
                }
                else
                {
                    cursorMove.x = 0;
                    Debug.Log("MoveChange:XNothing");
                }

                if (state.cursorYMove && change.y < (state.user.getMovementBounds(3) * moveThreshold1))
                {
                    cursorMove += new Vector2(0, change.y - (state.user.getMovementBounds(3) * moveThreshold1 / 2f));
                    //cursorMove += new Vector2(0, change.y);
                    Debug.Log("MoveChange:Down");
                }
                else if (state.cursorYMove && change.y > (state.user.getMovementBounds(4) * moveThreshold1))
                {
                    cursorMove += new Vector2(0, change.y - (state.user.getMovementBounds(4) * moveThreshold1 / 2f));
                    //cursorMove += new Vector2(0, change.y);
                    Debug.Log("MoveChange:Up");
                }
                else
                {
                    cursorMove.y = 0;
                    Debug.Log("MoveChange:YNothing");
                }

                //upperbound haptics - tells user when they are close to their max range xy&z
                /*float addHapt = 0f;
                if (state.cursorXMove && change.x < (state.user.getMovementBounds(1) * moveThreshold2)) //x
                {
                    addHapt += (change.x / (state.user.getMovementBounds(1) * moveThreshold2)) / 5;
                }
                else if (change.x > (state.user.getMovementBounds(2) * moveThreshold2))
                {
                    addHapt += (change.x / (state.user.getMovementBounds(2) * moveThreshold2)) / 5;
                }
                if (state.cursorYMove && change.y < (state.user.getMovementBounds(3) * moveThreshold2)) //y
                {
                    addHapt += (change.y / (state.user.getMovementBounds(3) * moveThreshold2)) / 5;
                }
                else if (change.y > (state.user.getMovementBounds(4) * moveThreshold2))
                {
                    addHapt += (change.y / (state.user.getMovementBounds(5) * moveThreshold2)) / 5;
                }
                if (change.z > (state.user.getMovementBounds(5) * moveThreshold2)) //z
                {
                    addHapt += (change.z / (state.user.getMovementBounds(5) * moveThreshold2)) / 4;
                }
                Debug.Log("MovementBounds Haptic Adding: " + addHapt);
                controllerVibration += addHapt;*/

                //clicking - click if user is a certain % of their max z range
                //Debug.Log("Added Z Stuff: " + change.z + " vs. " + (state.user.getMovementBounds(5) * moveThreshold3));
                if ((change.z - (state.user.getMovementBounds(5) * moveThreshold2)) > 0) //TODO divide bounds by factor so user isnt always expected to go to their full range of motion
                {
                    state.userClick = true;
                    state.userIsClicking = true;
                }
                else if (change.z > state.user.getMovementBounds(5) * moveThreshold2)
                {
                    state.userIsClicking = true;
                    state.userClick = false;
                }
                else
                {
                    state.userIsClicking = false;
                    state.userClick = false;
                }
            }
            else //if no calibration data (assuming we're in calibration step)
            {
                if (state.cursorXMove) //give user access to x&y movements if these booleans say so
                {
                    cursorMove += new Vector2(change.x, 0);
                }
                if (state.cursorYMove)
                {
                    cursorMove += new Vector2(0, change.y);
                }

                //clicking - same clicking methodology but based on an easy-to-reach position instead of calibrated data
                if ((Math.Floor(change.z) - (baseZCalibration * moveThreshold2)) > 0)
                {
                    state.userClick = true;
                    state.userIsClicking = true;
                }
                else if (change.z > baseZCalibration * moveThreshold2)
                {
                    state.userIsClicking = true;
                    state.userClick = false;
                }
                else
                {
                    state.userIsClicking = false;
                    state.userClick = false;
                }
            }
            //failsafe for clicking
            /*if (extraControls)
            {
                if (userStickButton(false))
                {
                    state.userClick = true;
                }
                if (userStickButton(true))
                {
                    state.userIsClicking = true;
                }
            }*/

            //adds hand position stuff to cursor movements
            cursorMove += new Vector2(state.cursorAdd.x, state.cursorAdd.y);

            state.cursorAdd = new Vector3(0f, 0f, 0f); //resetting additive property

            //moves cursor by factor of all the above*****
            trueCursor.transform.position += ((1.4f + ((5 - state.user.getSettingData()[0]) / 10f)) * (state.user.getSettingData()[2]/2.2f) *
                Time.deltaTime * ((trueCursor.transform.up * cursorMove.y) + (trueCursor.transform.right * cursorMove.x)));

            //Cursor cannot move past screen borders (bondaries) -- cursor bounds  y[-67,55], x[-79,87]
            Debug.Log("**True Cursor Location: " + trueCursor.transform.localPosition.ToString());
            if (trueCursor.transform.localPosition.x > cursorBoundRight)
            {
                trueCursor.transform.localPosition = new Vector3(cursorBoundRight, trueCursor.transform.localPosition.y, trueCursor.transform.localPosition.z);
            }
            else if (trueCursor.transform.localPosition.x < cursorBoundLeft)
            {
                trueCursor.transform.localPosition = new Vector3(cursorBoundLeft, trueCursor.transform.localPosition.y, trueCursor.transform.localPosition.z);
            }
            if (trueCursor.transform.localPosition.y > cursorBoundUp)
            {
                trueCursor.transform.localPosition = new Vector3(trueCursor.transform.localPosition.x, cursorBoundUp, trueCursor.transform.localPosition.z);
            }
            else if (trueCursor.transform.localPosition.y < cursorBoundDown)
            {
                trueCursor.transform.localPosition = new Vector3(trueCursor.transform.localPosition.x, cursorBoundDown, trueCursor.transform.localPosition.z);
            }
            
            //highlights the cursor based on certain actions
            if (isResetting(true) || isResetting(false)) //blue = resetting
            {
                //clickColor.GetComponent<Image>().color = cursorHighlight;
                clickLock.GetComponent<RawImage>().color = hideLock;
            }
            else if (!state.userControlActive) //yellow = locked
            {
                if (!noLock)
                {
                    //clickColor.GetComponent<Image>().color = cursorHighlight3;
                    clickLock.GetComponent<RawImage>().color = showLock;
                }
            }
            else if ((state.userIsClicking || state.userClick) && state.getState() != 5 && state.getState() != 6) //green = clicking
            {
                //clickColor.GetComponent<Image>().color = cursorHighlight2;
                clickLock.GetComponent<RawImage>().color = hideLock;
            }
            else
            {
                clickColor.GetComponent<Image>().color = nothing;
                clickLock.GetComponent<RawImage>().color = hideLock;
            }
            //Debug.Log("isSelecting2: " + specialClick.ToString()); //test
            if (specialClick)
            {
                //clickColor.GetComponent<Image>().color = cursorHighlight2;
                clickLock.GetComponent<RawImage>().color = hideLock;
                //Debug.Log("isSelecting3: " + specialClick.ToString());
            }

            //extra haptics with thumbsticks
            float scaledVal = 0f;
            if (extraControls)
            {
                scaledVal = Math.Abs(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.Touch).y + OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, OVRInput.Controller.Touch).y +
                                     OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.Touch).x + OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, OVRInput.Controller.Touch).x) / 4f;
            }
            //Debug.Log("scaled val: " + scaledVal);
            controllerVibration += scaledVal * 0.8f;

            //CAMERA CONTROL & CLICKING
            switch (state.getState()) //state camera control (positions of camera at various states)
            {
                case 0: //QUIT
                    VRPerson.SetActive(false);
                    break;
                case 1: //HOME
                    //VRPerson.transform.position = new Vector3(10f, 0f, 0f);
                    VRPerson.transform.position = new Vector3(0f, 0f, 0f);
                    break;
                case 2: //IN-GAME
                    VRPerson.transform.position = new Vector3(0f, 0f, 0f);
                    break;
                case 3: //PROFILE
                    //VRPerson.transform.position = new Vector3(20f, 0f, 0f);
                    VRPerson.transform.position = new Vector3(0f, 0f, 0f);
                    break;
                case 4: //CALIBRATE
                    //VRPerson.transform.position = new Vector3(0f, 10f, 0f);
                    VRPerson.transform.position = new Vector3(0f, 0f, 0f);
                    break;
                case 5: //TUTORIAL
                    VRPerson.transform.position = new Vector3(0f, 0f, 0f);
                    break;
                case 6: //ABOUT PROJECT
                    VRPerson.transform.position = new Vector3(-10f, 0f, 0f);
                    break;
                case 7: //PRACTICE LVL
                    VRPerson.transform.position = new Vector3(0f, 0f, 0f);
                    break;
                case 8: //SURVEY
                    VRPerson.transform.position = new Vector3(-20f, 0f, 0f); //TODO: edit for diff survey canvas later
                    break;
                default:
                    Debug.Log("VR State Error");
                    break;
            }
            //raycasting attempts on hold for now...
            /*if (state.getSelected() != null)
            {
                Debug.Log("*Raycast starting up...");
                Ray cursorRay = new Ray(state.getCursorPosition(), (state.getCursorPosition() - playerHead.transform.position).normalized);
                RaycastHit[] hits = Physics.RaycastAll(cursorRay, (state.getCursorPosition() - playerHead.transform.position).magnitude);
                foreach (RaycastHit hit in hits)
                {
                    Debug.Log("**" + hit.collider.gameObject.name + ": " + hit.collider.gameObject.transform.position);
                    //RaycastHit hit = new RaycastHit();
                    if (Physics.Raycast(state.getCursorPosition(), (state.getCursorPosition() - playerHead.transform.position).normalized))
                    {
                        //Instantiate(state.getSelected(), hit.point, Quaternion.identity);
                        Debug.Log("*** raycast working???");
                    }
                }
            }*/

            //Set Haptics
            OVRInput.SetControllerVibration(controllerVibration, controllerVibration, OVRInput.Controller.RTouch); //set haptics
            OVRInput.SetControllerVibration(controllerVibration, controllerVibration, OVRInput.Controller.LTouch);
        }
    }
    
    public void vrInfo()
    {
        //pos & rotation = [0,1], triggers = [0,1], sticks = [-10,10]
        Debug.Log("Player Pos: " + playerPos.arms + ", " + playerPos.head); ;
        Debug.Log("rightPos: " + OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand) + ", leftPos: " + OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand));
        Debug.Log("rightRot: " + OVRInput.GetLocalControllerRotation(OVRInput.Controller.RHand).eulerAngles + ", leftRot: " + OVRInput.GetLocalControllerRotation(OVRInput.Controller.LHand).eulerAngles);

        // returns a float of the Hand Trigger’s current state on the Left Oculus Touch controller.
        Debug.Log("LHandTrigger: " + OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.Touch) + ", RHandTrigger: " + OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger, OVRInput.Controller.Touch));
        Debug.Log("LIndTrigger: " + OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Touch) + ", RIndTrigger: " + OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch));

        Debug.Log("LStick: " + OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.Touch) + ", RStick: " + OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, OVRInput.Controller.Touch));
        Debug.Log("LStickP: " + OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch) + ", RStickP: " + OVRInput.Get(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch));

        if (OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.Touch) || OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.Touch))
        {
            Debug.Log("USER PUSHING RIGHT");
        }
        if (OVRInput.Get(OVRInput.Button.Three, OVRInput.Controller.Touch) || OVRInput.Get(OVRInput.Button.Four, OVRInput.Controller.Touch))
        {
            Debug.Log("USER PUSHING LEFT");
        }

        if (showMoveStats)
        {
            state.user.showMoveBounds();
        }
    }

    public static bool userContinue(bool isContinuous = false) //controls progression through tutorial instructions and ppt
    {
        if (!isContinuous) 
        {
            if (isRightHanded)
            {
                return OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
            }
            else
            {
                return OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch) || OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch);
            }
        }
        else
        {
            if (isRightHanded)
            {
                return OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch) || OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.RTouch);
            }
            else
            {
                return OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTouch) || OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch);
            }
        }
     }
    public static bool userSkip(bool isContinuous = false) //controls user's skipping throughout tutorial && locking of the cursor
    {
        /*if (isContinuous)
        {
            return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Touch) >= .99 || OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch) >= .99;
        }
        else
        {
            return (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Touch) < .9 && OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Touch) >= .15) ||
                (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch) >= .9 && OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch) >= .15);
        }*/
        return userStickButton(isContinuous);
    }
    public static bool userStickButton(bool isContinuous = false)
    {
        if (!isContinuous)
        {
            if (!isRightHanded){
                return OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch);
            }
            else
            {
                return OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch);
            }
        }
        else
        {
            if (!isRightHanded)
            {
                return OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.Touch); //thumbstickdown?
            }
            else
            {
                return OVRInput.Get(OVRInput.Button.SecondaryThumbstick, OVRInput.Controller.Touch);
            }
        }
    }
    public static bool cursorRelock(bool isContinuous = true)
    {
        /*if (OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch) && OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            return true;
        }
        else if (OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTouch) && OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            return true;
        }
        return false;*/
        return userSkip(isContinuous);
    }
    public static bool isResetting(bool isContinuous = false) //Resets baseline for cursor movement
    {
        if (!isContinuous)
        {
            if (!isRightHanded) //TODO: Should this be according to handedness or both by default
            {
                return (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Touch) > .2 && OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Touch) < 1.9);
            }
            else
            {
                return (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch) > .2 && OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch) < 1.9);
            }
        }
        else
        {
            if (!isRightHanded)
            {
                return (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Touch) > 1.9);
            }
            else
            {
                return (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch) > 1.9);
            }
        }
        //return clickDown(isContinuous);
    }
    public static bool isNotResetting()
    {
        if (!isRightHanded)
        {
            return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.Touch) == 0;
        }
        else
        {
            return OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger, OVRInput.Controller.Touch) == 0;
        }
    }

    public static Vector3 handTracking(bool factored = true)
    {
        if (!factored)
        {
            return (OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand) + OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand)) / 2f;
        }
        else //returns hand pos value factored for headset
        {
            Vector3 hand = (OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand) + OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand)) / 2f;
            Vector3 move = new Vector3((farRight.transform.position - hand).magnitude, (farUp.transform.position - hand).magnitude, (farForward.transform.position - hand).magnitude);
            return move * StateManager.cursorSpeed; //getSettingData()[2]
        }
    }

    public static int getStickState()  //not in use atm
    {
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.Touch) + OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, OVRInput.Controller.Touch);
        if (stick.x > .25 && stick.x < .8) //large range of stick vals but not fully pressed
        {
            return 2; //left
        }
        else if (stick.x < -.25 && stick.x > -.8)
        {
            return -2; //right
        }
        else if (stick.y > .25 && stick.y < .8) //large range of stick vals but not fully pressed
        {
            return 1; //up
        }
        else if (stick.y < -.25 && stick.y > -.8)
        {
            return -1; //down
        }
        
        else { return 0; }
    }

    public static int buttonConversion() //8 = nothing
    {
        /*int i = ClickAction.tagClose();
        if (i != 0)
        {
            return (i + 1); //2-5
        }*/
        if (ClickAction.tag1Close()) //tags [1,4] (top to bottom)
        {
            if (buildExclusiveFunc)
            {
                return 4;
            }
            return 1;
        }
        else if (ClickAction.tag2Close())
        {
            if (buildExclusiveFunc)
            {
                return 1;
            }
            return 2;
        }
        else if (ClickAction.tag3Close())
        {
            return 3;
        }
        else if (ClickAction.tag4Close())
        {
            if (buildExclusiveFunc)
            {
                return 2;
            }
            return 4;
        }
        else if (ClickAction.uiButtonClose()) //next
        {
            return 0;
        }
        else if (ClickAction.uiButtonClose2()) //home
        {
            return 5;
        }
        else if (ClickAction.binClose())
        {
            return 7;
        }
        return 8;
    }

    public static void armsFixes()
    {
        GameObject.Find("headsetUp").transform.localPosition -= new Vector3(0f, GameObject.Find("headsetUp").transform.localPosition.y - 3.6f, 0f); //constant 3.6 y
        GameObject.Find("headsetRight").transform.localPosition -= new Vector3(0f, GameObject.Find("headsetRight").transform.localPosition.y, 0f); // constant 0 y
        GameObject.Find("headsetForward").transform.localPosition -= new Vector3(0f, GameObject.Find("headsetForward").transform.localPosition.y, 0f); // constant 0 y
    }

    public class UserBounds //used for later instances of compensatory motion tracking
    {
        public Vector3 head;
        public Vector3 arms;
        public UserBounds(Vector3 armsPos, Vector3 headPos)
        {
            head = headPos;
            arms = armsPos;
        }
    }
}
