using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CassetteController : MonoBehaviour, IHoldable
{
    //Function: Goes on Cassette Tape object, contains data and methods for interacting with tape

    //Objects & Components:
    public string clipName;          //Name of this recording (to display to player)
    internal AudioSource audioSource; //The audio source used to play this tape
    internal Transform model;        //Model to move
    private GameObject bounds;       //Object containing bounding box
    internal TouchManager.TouchData holdingTouch; //Touch holding this tape (if tape is being held)

    //Positions:
    [Header("Positional References:")]
    public Transform heldPosition;     //Positional reference for when tape is being held
    public Transform originPosition;   //Positional reference for tape's original placement
    public Transform insertedPosition; //Positional reference for tape's inserted placement

    //Settings:
    [Header("Settings:")]
    public float holdOrientLerpSpeed; //How fast tape snaps to target depth and rotation when held
    public float holdPosLerpSpeed;    //How fast tape snaps to finger position when held
    public float insertLerpSpeed;     //How fast tape snaps to inserted position when held over cassette player
    public float releaseLerpSpeed;    //How fast tape snaps back to origin position when released
    public float moveSnapThresh;      //How close to target tape must be to snap into place

    //Status Vars:
    internal float progress = 0;    //How far along recording currently is (between 0 and 1)
    internal bool inserted = false; //Whether or not tape is inserted into player
    private bool posSnapped = true; //Determines whether or not tape needs to move on the upcoming frame

    //Runtime Methods:
    private void Awake()
    {
        //Get Components:
        audioSource = GetComponent<AudioSource>(); //Get audio source on tape object
        model = transform.GetChild(0);             //Get transform of cassette model
        bounds = GetComponentInChildren<Collider>().gameObject; //Find tape's bounding object in hierarchy and get reference
    }
    private void Update()
    {
        if (!posSnapped) //Only move tape when necessary
        {
            //Initializations (eckgh gross I know):
            Vector3 posTarget = new Vector3();       //Initialize positional target
            Quaternion rotTarget = new Quaternion(); //Initialize rotational target
            Vector3 scaleTarget = new Vector3();     //Initialize scalar target
            float posLerpSpeed = 0;                  //Initialize lerp speed setting
            float orientLerpSpeed = 0;               //Initialize lerp speed setting

            //Get Move Targets:
            if (holdingTouch != null) //Cassette tape is currently being held
            {
                //Determine Hold Type:
                bounds.SetActive(false); //Temporarily disable bounding area while checking for player
                Collider hitCollider = TouchManager.inputManager.CheckTouchedCollider(holdingTouch); //Try to find an object behind the held tape
                if (hitCollider != null && hitCollider.CompareTag("Recorder")) //Tape is being held over recorder
                {
                    //Triggers:
                    if (!inserted) //Tape is not currently inserted in player
                    {
                        if (CPController.main.tape != null) //Player already has a tape in it
                        {
                            CPController.main.tape.Eject(); //Pop existing tape out of player
                        }
                        Insert(); //Insert tape into player
                    }

                    //Target Inserted Position:
                    posTarget = insertedPosition.position; //Set inserted target
                    rotTarget = insertedPosition.rotation; //Set inserted rotation
                    scaleTarget = Vector3.Scale(insertedPosition.localScale, insertedPosition.parent.localScale); //Set inserted scale (apply secondary scale of parent)
                    posLerpSpeed = insertLerpSpeed;    //Set insertion speed
                    orientLerpSpeed = insertLerpSpeed; //Set insertion speed
                }
                else //Tape is not being held over recorder
                {
                    //Triggers:
                    if (inserted) Eject(); //Pop tape out of player

                    //Target Touch Position:
                    posTarget = Camera.main.ScreenToWorldPoint(new Vector3(holdingTouch.position.x, holdingTouch.position.y, heldPosition.position.z)); //Apply X and Y values of holding touch to target position
                    posTarget.z = heldPosition.position.z; //Get target depth directly from heldPosition transform (stays consistent)
                    rotTarget = heldPosition.rotation;     //Set target rotation
                    scaleTarget = heldPosition.localScale; //Set target scale
                    posLerpSpeed = holdPosLerpSpeed;       //Set speed factor for lerping tape position toward target
                    orientLerpSpeed = holdOrientLerpSpeed; //Set speed factor for lerping tape orientation toward target
                }
                bounds.SetActive(true); //Re-enable bounding area as soon as check is complete
            }
            else //Cassette tape is not being held, but is moving
            {
                if (inserted) //Tape was inserted in player when it was last released
                {
                    //Target Inserted Position:
                    posTarget = insertedPosition.position; //Set inserted position
                    rotTarget = insertedPosition.rotation; //Set inserted rotation
                    scaleTarget = Vector3.Scale(insertedPosition.localScale, insertedPosition.parent.localScale); //Set inserted scale (apply secondary scale of parent)
                    //NOTE: Lerps are ignored here because tape snaps immediately into player when released
                }
                else //Tape was not inserted in player when it was last released
                {
                    //Target Original Position:
                    posTarget = originPosition.position;     //Set origin position
                    rotTarget = originPosition.rotation;     //Set origin rotation
                    scaleTarget = originPosition.localScale; //Set origin scale
                    posLerpSpeed = releaseLerpSpeed;         //Set speed factor for lerping tape position toward target
                    orientLerpSpeed = releaseLerpSpeed;      //Set speed factor for lerping tape orientation toward target
                }
            }

            //Check For Position Snap:
            if (!inserted &&          //Do not snap non-inserted tapes
                holdingTouch == null) //Do not snap held tapes
            {
                if (Vector3.Distance(model.position, posTarget) < moveSnapThresh) //Tape is very close to target
                {
                    posSnapped = true; //Snap tape to target position and allow it to stop moving
                }
            }

            //Move Tape:
            if (posSnapped || inserted && holdingTouch == null) //Tape should snap to its target position
            {
                //Immediately Snap Tape To Position:
                model.position = posTarget;     //Set position
                model.rotation = rotTarget;     //Set rotation
                model.localScale = scaleTarget; //Set scale
            }
            else //Tape should lerp toward its target position
            {
                //Additional Pre-Move Calculations:
                posLerpSpeed *= Time.deltaTime;    //Apply deltaTime to lerp speed
                orientLerpSpeed *= Time.deltaTime; //Apply deltaTime to lerp speed
                Vector3 newPosition = Vector3.Lerp(model.position, posTarget, posLerpSpeed); //Get new position as lerp between current position and target
                newPosition.z = Mathf.Lerp(model.position.z, posTarget.z, orientLerpSpeed);  //Use different lerp speed for orientation

                //Lerp Tape:
                model.position = newPosition;                                                    //Move tape to new position
                model.rotation = Quaternion.Lerp(model.rotation, rotTarget, orientLerpSpeed);    //Lerp rotation toward target
                model.localScale = Vector3.Lerp(model.localScale, scaleTarget, orientLerpSpeed); //Lerp scale toward target
            }
            
        }
    }

    //Interaction Methods:
    public void TryHold(TouchManager.TouchData touch)
    {
        //Function: Called by InputManager when this tape is touched

        //Check if Tape Can Be Held:
        if (holdingTouch != null) return;                    //Ignore if tape is already being held
        if (inserted && !CPController.main.doorOpen) return; //Ignore if tape is inserted into player and door is closed

        //Handshake:
        holdingTouch = touch;    //Associate given touch with this script
        touch.heldObject = this; //Associate this script with given touch

        //Triggers:
        if (CPController.main.stowed) //Cassette player is on the table
        {
            CPController.main.ToggleDoor(true); //Open cassette door
        }

        //Cleanup:
        posSnapped = false; //Unlock movement
        TouchManager.inputManager.GetComponent<AudioSource>().Play();
    }
    public void Release()
    {
        //Function: Called by InputManager when this tape is released (from a holding touch)

        //Handshake:
        holdingTouch.heldObject = null; //Dissociate this script from releasing touch
        holdingTouch = null;            //Dissociate releasing touch from this script

        //Triggers:
        if (CPController.main.stowed) //Cassette player is on the table
        {
            bool safeToClose = true; //Initialize variable to track whether there are any other held tapes in scene
            foreach (TouchManager.TouchData touch in TouchManager.inputManager.touchDataList) //Iterate through all current touches
            {
                if (touch.heldObject != null) { safeToClose = false; break; } //If another tape is currently being held, do not close door
            }
            if (safeToClose) CPController.main.ToggleDoor(false); //Close cassette door (if necessary)
        }

        //Cleanup:
        posSnapped = false; //Unlock movement
    }
    public void Insert()
    {
        //Function: Inserts tape into cassette player

        //Handshake:
        CPController.main.tape = this;               //Indicate to player that this tape is now inserted
        CPController.main.SetTrackerUI();            //Update tracker UI on player

        //Cleanup:
        inserted = true; //Indicate that tape is now inserted

        //Triggers:
        CPController.main.audioSource.PlayOneShot(CPController.main.tapeInsertSound); //Play insert sound
    }
    public void Eject()
    {
        //Function: Pops tape out of cassette player

        //Handshake:
        CPController.main.tape = null;                  //Indicate to player that it no longer has a tape inserted
        CPController.main.SetTrackerUI();               //Reset player UI

        //Cleanup:
        inserted = false; //Indicate that tape is no longer in player

        //Triggers:
        CPController.main.audioSource.PlayOneShot(CPController.main.tapeEjectSound); //Play eject sound
    }
}
