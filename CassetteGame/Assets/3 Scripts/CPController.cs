using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPController : MonoBehaviour
{
    //Description: Governs functions relating to the cassette player

    //Objects & Components:
    public static CPController main; //Singleton instance of this script in scene
    private CassetteTape tape;       //Tape currently in cassette player (if any)

    [Header("Player Components:")]
    public Transform model;                        //Transform of model (which gets moved around and animated and stuff)
    public Transform door;                         //Cassette door model
    public Transform[] buttons = new Transform[6]; //Button models
    [Space()]

    //Positions:
    [Header("Component Positions:")]
    public Transform playerStowedPos;   //Position of cassette player when stowed away
    public Transform playerDeployedPos; //Position of cassette player when deployed and fully visible
    public Transform doorClosedPos;     //Position of cassette door when closed
    public Transform doorOpenPos;       //Position of cassette door when opened
    public float buttonPushDepth;       //How far buttons travel when pushed

    //Settings:
    [Header("Settings:")]
    public float deploySpeed; //How fast cassette player moves when deployed
    public float stowSpeed;   //How fast cassette player moves when stowed
    public float deploySnapThresh; //How close player can be to target position before it snaps into place
    [Space()]
    public float doorOpenSpeed;  //How fast cassette door opens
    public float doorCloseSpeed; //How fast cassette door closes
    public float doorSnapThresh; //How close door can be to target position before it snaps into place

    //Memory Vars:
    internal bool stowed = true; //Whether or not the cassette player is stowed
    internal bool doorOpen;      //Whether or not cassette door is open
    private bool[] buttonPushed = new bool[6]; //Whether or not each button (at given index) is currently being pushed
    private bool stowPosSnapped = true; //Whether or not model has snapped to target deployment position and become static
    private bool doorPosSnapped = true; //Whether or not door model has snapped to target position and become static

    //TEMP Debug Stuff:
    [Space()]
    public bool debugToggleStow;
    public bool debugToggleDoor;

    //Game Methods:
    private void Awake()
    {
        //Initialization:
        if (main == null) main = this; else Destroy(this); //Singleton-ize this script
    }
    private void Update()
    {
        //Debug Inputs:
        if (debugToggleStow) { debugToggleStow = false; ToggleStow(!stowed); }
        if (debugToggleDoor) { debugToggleDoor = false; ToggleDoor(!doorOpen); }

        //Animate Deployment:
        if (!stowPosSnapped) //Only perform movement when necessary
        {
            //Find Targets:
            Vector3 targetPos = playerStowedPos.position;     //Get target position (assume player is stowed)
            Quaternion targetRot = playerStowedPos.rotation;  //Get target rotation (assume player is stowed)
            Vector3 targetScale = playerStowedPos.localScale; //Get target scale (assume player is stowed)
            float lerpSpeed = stowSpeed;                      //Get speed to lerp toward target (assume player is stowed)
            if (!stowed) //Player is being deployed instead
            {
                targetPos = playerDeployedPos.position;     //Get deployed position
                targetRot = playerDeployedPos.rotation;     //Get deployed rotation
                targetScale = playerDeployedPos.localScale; //Get deployed scale
                lerpSpeed = deploySpeed;                    //Change lerp speed to deployment setting
            }
            lerpSpeed *= Time.deltaTime; //Apply deltaTime to lerp speed

            //Move Model:
            if (Vector3.Distance(model.position, targetPos) < deploySnapThresh) //Model is close enough to target to snap into position
            {
                //Snap Model to Target:
                model.position = targetPos;     //Set position to target
                model.rotation = targetRot;     //Set rotation to target
                model.localScale = targetScale; //Set scale to target

                //Cleanup:
                stowPosSnapped = true; //Stop animation once finished
            }
            else //Model is still far from target
            {
                //Incrementally Move Model Closer to Target:
                model.position = Vector3.Lerp(model.position, targetPos, lerpSpeed);       //Lerp position toward target
                model.rotation = Quaternion.Lerp(model.rotation, targetRot, lerpSpeed);    //Lerp rotation toward target
                model.localScale = Vector3.Lerp(model.localScale, targetScale, lerpSpeed); //Lerp scale toward target
            }
        }

        //Animate Door:
        if (!doorPosSnapped) //Only perform movement when necessary
        {
            //Find Targets:
            Vector3 targetPos = doorClosedPos.position;    //Get target position (assume door is closed)
            Quaternion targetRot = doorClosedPos.rotation; //Get target rotation (assume door is closed)
            float lerpSpeed = doorCloseSpeed;              //Get speed to lerp toward target (assume door is closed)
            if (doorOpen) //Door is being opened instead
            {
                targetPos = doorOpenPos.position; //Get open position
                targetRot = doorOpenPos.rotation; //Get open rotation
                lerpSpeed = doorOpenSpeed;        //Change lerp speed to open setting
            }
            lerpSpeed *= Time.deltaTime; //Apply deltaTime to lerp speed

            //Move Model:
            if (Vector3.Distance(door.position, targetPos) < doorSnapThresh) //Door is close enough to target to snap into position
            {
                //Snap Door to Target:
                door.position = targetPos; //Set position to target
                door.rotation = targetRot; //Set rotation to target

                //Cleanup:
                doorPosSnapped = true; //Stop animation once finished
            }
            else //Door is still far from target
            {
                //Incrementally Move Door Closer to Target:
                door.position = Vector3.Lerp(door.position, targetPos, lerpSpeed);    //Lerp position toward target
                door.rotation = Quaternion.Lerp(door.rotation, targetRot, lerpSpeed); //Lerp rotation toward target
            }
        }

    }

    //Interaction Methods:
    public void ToggleStow(bool stow)
    {
        //Function: Slides player on or off screen

        //Initialization:
        if (stow == stowed) return; //Redundancy check
        stowed = stow; //Toggle state
        stowPosSnapped = false; //Unlock deployment animation

        //State Change Triggers:
        if (stowed) //Events which trigger upon player being stowed
        {
            
        }
        else //Events which trigger upon player being deployed
        {

        }
    }
    public void ToggleDoor(bool open)
    {
        //Function: Opens or closes cassette door

        //Initialization:
        if (open == doorOpen) return; //Redundancy check
        doorOpen = open; //Toggle state
        doorPosSnapped = false; //Unlock door animation

        //State Change Triggers:
        if (doorOpen) //Events which trigger upon door being opened
        {

        }
        else //Events which trigger upon door being closed
        {

        }
    }
    public void PushButton(int buttonIndex)
    {
        //Function: Pushes button at button index (performs animation and programmatic functions)

        //Initialization:
        if (buttonPushed[buttonIndex]) return; //Redundancy check
        buttonPushed[buttonIndex] = true; //Indicate that button is now being pushed

        //Execute Button Function:

    }
    public void ReleaseButton(int buttonIndex)
    {
        //Function: Returns button at specified index to its original position

        //Initialization:
        if (!buttonPushed[buttonIndex]) return; //Redundancy check
        buttonPushed[buttonIndex] = false; //Indicate that button has been released
    }
}
