using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CPController : MonoBehaviour
{
    //Description: Governs functions relating to the cassette player

    //Objects & Components:
    public static CPController main;  //Singleton instance of this script in scene
    internal CassetteController tape; //Tape currently in cassette player (if any)
    private AudioSource audioSource;  //Audio source used to play sound effects

    [Header("Player Components:")]
    public Transform model;     //Transform of model (which gets moved around and animated and stuff)
    public Transform door;      //Cassette door model
    public Transform[] buttons; //Button models
    [Space()]
    public RectTransform trackerBar;  //UI element representing tracker bar
    public RectTransform progressBar; //UI element representing progress of current tape
    public RectTransform recordBar;   //UI element representing current recorded area
    public RectTransform timeTick;    //UI element representing tick where end of progress bar is
    public TMP_Text namePlate;        //UI element for displaying inserted tape name
    public TMP_Text timeStamp;        //UI element for displaying progression of time in clip
    private GameObject bounds;         //Player hitbox while stowed
    private GameObject deployedBounds; //Player hitbox while deployed

    //Positions:
    [Header("Component Positions:")]
    public Transform playerStowedPos;   //Position of cassette player when stowed away
    public Transform playerDeployedPos; //Position of cassette player when deployed and fully visible
    public Transform doorClosedPos;     //Position of cassette door when closed
    public Transform doorOpenPos;       //Position of cassette door when opened
    public float buttonPushDepth;       //How far buttons travel when pushed
    private Vector3[] buttonOriginPos = new Vector3[6]; //Local positions buttons start at

    //Settings:
    [Header("Settings:")]
    public float deploySpeed;      //How fast cassette player moves when deployed
    public float stowSpeed;        //How fast cassette player moves when stowed
    public float deploySnapThresh; //How close player can be to target position before it snaps into place
    [Space()]
    public float doorOpenSpeed;  //How fast cassette door opens
    public float doorCloseSpeed; //How fast cassette door closes
    public float doorSnapThresh; //How close door can be to target position before it snaps into place
    [Space()]
    [Range(0, 1)] public float buttonTriggerThresh; //How far a button has to be pressed before it actually triggers that button's function
    public float buttonPressSpeed;   //How fast buttons take to fully move to pressed position
    public float buttonReleaseSpeed; //How fast buttons take to return to origin once released
    public float buttonSnapThresh;   //How close buttons can be to target position before they snap into place

    //Status Vars:
    internal bool stowed = true;                   //Whether or not the cassette player is stowed
    internal bool doorOpen;                        //Whether or not cassette door is open
    internal bool[] buttonPushed = new bool[6];    //Whether or not each button (at given index) is currently being pushed
    private bool stowPosSnapped = true;            //Whether or not model has snapped to target deployment position and become static
    private bool doorPosSnapped = true;            //Whether or not door model has snapped to target position and become static
    private bool[] buttonPosSnapped = new bool[6]; //Whether or not button has snapped to target position and become static
    private bool playing = false;                  //Whether or not inserted tape is currently being played
    private bool recording = false;                //Whether or not player is currently recording clip from inserted tape
    private float recordStart = -1;                //Starting point of current record area (negative if NA)
    private float recordLength;                    //Length of current recorded area

    //TEMP Debug Stuff:
    [Space()]
    public bool debugToggleStow;
    public bool debugToggleDoor;
    public bool debugToggleButton;
    public int debugButtonSelector;

    //Runtime Methods:
    private void Awake()
    {
        //Initialization:
        if (main == null) main = this; else Destroy(this); //Singleton-ize this script

        //Get Origin Positions:
        for (int i = 0; i < buttons.Length; i++) //Iterate through list of buttons
        {
            buttonOriginPos[i] = buttons[i].localPosition; //Log origin position of button (as starting local position)
        }

        //Get Components:
        audioSource = GetComponent<AudioSource>();     //Get audio source object on player
        bounds = model.GetChild(0).gameObject;         //Get bounds object
        deployedBounds = model.GetChild(1).gameObject; //Get deployed bounds object
    }
    private void Update()
    {
        //Debug Inputs:
        if (debugToggleStow) { debugToggleStow = false; ToggleStow(!stowed); }
        if (debugToggleDoor) { debugToggleDoor = false; ToggleDoor(!doorOpen); }
        if (debugToggleButton)
        {
            debugToggleButton = false;
            ToggleButton(debugButtonSelector, !buttonPushed[debugButtonSelector]);
        }

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

                //Triggers:
                if (doorOpen && stowed && tape != null) tape.Eject(); //Special trigger for releasing tape when pressing the eject button

                //Cleanup:
                stowPosSnapped = true; //Stop animation once finished
            }
            else //Model is still far from target
            {
                //Incrementally Move Model Closer to Target:
                model.position = Vector3.Slerp(model.position, targetPos, lerpSpeed);      //Lerp position toward target
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

                //Triggers:
                if (tape != null && !doorOpen) tape.model.gameObject.SetActive(false); //Deactivate tape model to prevent weird clipping (and maybe raycast conflicts I dunno)

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

        //Animate Buttons:
        for (int i = 0; i < buttons.Length; i++) //Iterate through array of buttons
        {
            //Initialization:
            if (buttonPosSnapped[i]) continue; //Only perform movement when necessary
            Transform button = buttons[i];     //Get reference to current button transform

            //Find Target:
            Vector3 targetPos = buttonOriginPos[i]; //Initialize position target as starting pos (assume button is being released)
            float lerpSpeed = buttonReleaseSpeed;   //Get speed of lerp toward target (assume button is being released)
            if (buttonPushed[i]) //Button is being pushed instead
            {
                targetPos.z -= buttonPushDepth; //Apply push depth to base position of button to get pushed position
                lerpSpeed = buttonPressSpeed;   //Change lerp speed to press setting
            }
            lerpSpeed *= Time.deltaTime; //Apply deltaTime to lerp speed

            //Move Model:
            float prevPushAmount = Mathf.InverseLerp(buttonOriginPos[i].z, targetPos.z, button.localPosition.z); //Calculate how much button has been pushed (between 0 and 1)
            if (Vector3.Distance(button.localPosition, targetPos) < buttonSnapThresh) //Button is close enough to target to snap into position
            {
                //Snap Button to Target:
                button.localPosition = targetPos; //Set position to target
                buttonPosSnapped[i] = true; //Stop animation once finished
            }
            else //Button is still far from target
            {
                //Incrementally Move Button Closer to Target:
                button.localPosition = Vector3.Lerp(button.localPosition, targetPos, lerpSpeed); //Lerp position toward target

                //Check for Trigger Condition:
                if (buttonPushed[i] && prevPushAmount < buttonTriggerThresh && //Only check while button is being pushed (and has not already triggered function)
                    tape != null)                                              //And while a tape is inserted
                {
                    float pushAmount = Mathf.InverseLerp(buttonOriginPos[i].z, targetPos.z, button.localPosition.z); //Calculate push amount again, after motion
                    if (pushAmount >= buttonTriggerThresh) //Button has just passed trigger threshold
                    {
                        switch (i) //Determine function to trigger depending on button index
                        {
                            case 0: OnRecord(); break;
                            case 1: OnRewind(); break;
                            case 2: OnPlay(); break;
                            case 3: OnPause(); break;
                            case 4: OnFastForward(); break;
                            case 5: OnEject(); break;
                        }
                    }
                }
                
            }
        }

        //Check Tape Progress:
        if (tape != null && playing) //Player currently contains a tape (which is being played)
        {
            //Update Progress:
            float progress = tape.audioSource.time / tape.audioSource.clip.length; //Get progress through current tape as float between 0 and 1
            tape.progress = progress; //Record progress on tape

            //Check For End Triggers:
            if (tape.audioSource.pitch > 0 && progress >= 1) //Tape has reached its end (while moving forward)
            {
                //End Trigger:
                progress = 1;               //Lock progress to 0
                playing = false;            //Indicate that tape is no longer being played
                tape.audioSource.pitch = 1; //Reset playback speed
                tape.audioSource.Pause();   //Pause clip at end
            }
            else if (tape.audioSource.pitch < 0 && progress <= 0) //Tape has reached its beginning (while moving backward)
            {
                //End (Beginning) Trigger:
                progress = 0;               //Lock progress to 0
                playing = false;            //Indicate that tape is no longer being played
                tape.audioSource.pitch = 1; //Reset playback speed
                tape.audioSource.Stop();    //Stop clip at beginning
            }

            //Update Tracker Bars (and Time Tick):
            progressBar.sizeDelta = new Vector2(Mathf.Lerp(0, trackerBar.rect.width, progress), trackerBar.rect.height); //Set position of progress bar based on current progress through tape
            if (recording) //Player is currently recording
            {
                float startPosition = Mathf.Lerp(0, trackerBar.rect.width, recordStart); //Get X position of record bar left bound
                float endPosition = Mathf.Lerp(0, trackerBar.rect.width, progress);      //Get X position of record bar right bound
                float newWidth = endPosition - startPosition;                            //Get width record bar should be
                recordBar.sizeDelta = new Vector2(newWidth, trackerBar.rect.height);     //Set new width
            }

            //Update Timestamp:
            UpdateTimeStamp(); //Perform packaaged timestamp update calculation
        }
    }

    //Cassette Tape Methods:
    public void OnRecord()
    {
        //Function: Called when Record button is pressed

        //Record Start or End Behavior:
        if (recording) //Player is currently recording
        {
            //End Recording:
            recording = false; //Indicate that player is no longer recording

        }
        else //Player is not currently recording
        {
            //Start Recording:
            recording = true; //Indicate that player is now recording
            recordStart = tape.progress; //Start recording at current position on tape
            recordLength = 0; //Reset recording length (for new recording)

            //Position Record Bar:
            Vector3 newPosition = trackerBar.localPosition; //Get local position of tracker bar to base record bar position off of
            newPosition.x = Mathf.Lerp(newPosition.x, newPosition.x + trackerBar.rect.width, recordStart); //Set position to beginning of recording area
            recordBar.localPosition = newPosition; //Move record bar to new position
            recordBar.sizeDelta = new Vector2(recordLength, trackerBar.rect.height); //Apply reset record bar length
            
        }
    }
    public void OnRewind()
    {
        //Function: Called when Rewind button is pressed

        //Decrease Speed:
        if (tape.audioSource.pitch > 0) tape.audioSource.pitch = -1; //Reverse at normal speed if tape is already playing
        else tape.audioSource.pitch *= 2; //Double reverse speed if already reversing

        //Play Clip:
        if (!playing) //Tape is not already playing
        {
            if (tape.progress > 0) //Only play if tape is not in starting position
            {
                tape.audioSource.UnPause(); //Un-pause tape
                playing = true; //Indicate that tape is now playing
            }
            //NOTE: Does not play when tape is at 0 because tape cannot rewind
        }
    }
    public void OnPlay()
    {
        //Function: Called when Play button is pressed

        //Initialization:
        if (tape.progress == 1) return; //Ignore if player is at the end of the tape
        tape.audioSource.pitch = 1; //Set pitch to 1 (normal speed)
        if (playing) return; //Ignore if player is already playing the tape

        //Play Clip:
        if (tape.progress > 0) tape.audioSource.UnPause(); //Un-pause tape
        else tape.audioSource.Play(); //Play tape

        //Cleanup:
        playing = true; //Indicate that player is now playing a tape
    }
    public void OnPause()
    {
        //Function: Called when Pause button is pressed

        //Initialization:
        if (!playing) return; //Ignore if player is not currently playing a tape

        //Pause Clip:
        tape.audioSource.Pause(); //Pause clip

        //Cleanup:
        //tape.audioSource.pitch = 1; //Reset pitch
        playing = false;            //Indicate that tape is no longer playing
    }
    public void OnFastForward()
    {
        //Function: Called when FastForward button is pressed

        //Increase Speed:
        if (tape.audioSource.pitch < 0) tape.audioSource.pitch = 2; //Play at double speed if reversing currently
        tape.audioSource.pitch *= 2; //Double speed

        //Play Clip:
        if (!playing) //Tape is not already playing
        {
            if (tape.progress == 1) return;                         //Ignore if tape is already at its end
            else if (tape.progress > 0) tape.audioSource.UnPause(); //Un-pause tape
            else tape.audioSource.Play();                           //Play tape
            playing = true;                                         //Indicate that tape is now being played
        }
    }
    public void OnEject()
    {
        //Function: Called when Eject button is pressed

        //Stop Tape:
        if (playing) //Tape is currently playing
        {
            //Change Status:
            playing = false;   //Indicate that tape is no longer being played
            recording = false; //Indicate that tape is no longer being recorded

            //Stop Clip:
            tape.audioSource.Pause(); //Pause clip
        }

        //Ejection Procedure:
        ToggleDoor(true); //Pop door open
        ToggleStow(true); //Return player to table
    }

    //Interaction Methods:
    public void SetTrackerUI()
    {
        //Function: Updates tracker UI to match status of currently-inserted tape

        //Update Nameplate:
        if (tape != null) namePlate.text = tape.clipName; //Set nameplate to name of tape
        else namePlate.text = "";                         //Clear nameplate

        //Reset Record Bar:
        recordBar.sizeDelta = new Vector2(0, trackerBar.rect.height); //Make bar invisible by setting width to zero
        recordBar.localPosition = trackerBar.localPosition;           //Move bar to beginning of tracker

        //Update Progress Bar:
        if (tape != null) progressBar.sizeDelta = new Vector2(Mathf.Lerp(0, trackerBar.rect.width, tape.progress), trackerBar.rect.height); //Set position of progress bar based on current progress through tape
        else progressBar.sizeDelta = new Vector2(0, trackerBar.rect.height); //If no tape is inserted, simply clear progress bar

        //Initialize TimeStamp:
        UpdateTimeStamp(); //Perform timestamp update function to initialize readout

        //Update Status Vars:
        recordStart = -1; //Clear recording data
        recordLength = 0; //Clear recording data
    }
    public void ToggleStow(bool stow)
    {
        //Function: Slides player on or off screen

        //Initialization:
        if (stow == stowed) return; //Redundancy check
        if (stowed && tape == null) return; //Prevent deployment while empty
        stowed = stow; //Toggle state
        stowPosSnapped = false; //Unlock deployment animation

        //State Change Triggers:
        if (stowed) //Events which trigger upon player being stowed
        {
            //Swap Hitboxes:
            bounds.SetActive(true);          //Enable normal bounds
            deployedBounds.SetActive(false); //Disable deployed bounds

            //Release All Buttons:
            for (int i = 0; i < buttons.Length; i++) ReleaseButton(i); //Iterate through list of buttons and release each one
        }
        else //Events which trigger upon player being deployed
        {
            //Swap Hitboxes:
            bounds.SetActive(false);        //Disable normal bounds
            deployedBounds.SetActive(true); //Enable deployed bounds
        }
    }
    public void ToggleDoor(bool open)
    {
        //Function: Opens or closes cassette door

        //Initialization:
        if (open == doorOpen) return; //Redundancy check
        doorOpen = open; //Toggle state
        doorPosSnapped = false; //Unlock door animation

        //Triggers:
        if (doorOpen) //Events which trigger upon door being opened
        {
            if (tape != null) tape.model.gameObject.SetActive(true); //Re-activate tape model
        }
        else //Events which trigger upon door being closed
        {

        }
    }
    public void ToggleButton(int buttonIndex, bool press)
    {
        //Function: Pushes or releases button at button index

        if (press) PushButton(buttonIndex);
        else ReleaseButton(buttonIndex);
    }
    public void PushButton(int buttonIndex)
    {
        //Function: Pushes button at button index (performs animation and programmatic functions)

        //Initialization:
        if (buttonPushed[buttonIndex]) return; //Redundancy check
        buttonPushed[buttonIndex] = true;      //Indicate that button is now being pushed
        buttonPosSnapped[buttonIndex] = false;  //Unlock button animation

        //Initial Button Press Triggers:

    }
    public void ReleaseButton(int buttonIndex)
    {
        //Function: Returns button at specified index to its original position

        //Initialization:
        if (!buttonPushed[buttonIndex]) return; //Redundancy check
        buttonPushed[buttonIndex] = false;      //Indicate that button has been released
        buttonPosSnapped[buttonIndex] = false;  //Unlock button animation

        //Execute Release Triggers:

    }

    //Utility Methods:
    public void UpdateTimeStamp()
    {
        //Function: Updates timestamp to match length and progress of inserted tape

        //Initial Check:
        if (tape == null) //No tape is inserted
        {
            timeTick.localPosition = trackerBar.localPosition; //Reset position of tick
            timeStamp.text = "0:00/0:00"; //Clear timestamp text
            return; //Skip all other checks
        }

        //Update Tick Position:
        Vector3 tickPosition = trackerBar.localPosition; //Reference local position of tracker bar
        tickPosition.x = Mathf.Lerp(tickPosition.x, tickPosition.x + trackerBar.rect.width, tape.progress); //Find target location along tracker bar
        timeTick.localPosition = tickPosition; //Set new position

        //Update Text:
        float progSecs = Mathf.RoundToInt(tape.audioSource.time);         //Get seconds progressed
        float totalSecs = Mathf.RoundToInt(tape.audioSource.clip.length); //Get seconds in total clip
        float progMins = progSecs / 60; progMins = (int)progMins;         //Get minutes from seconds
        float totalMins = totalSecs / 60; totalMins = (int)totalMins;     //Get minutes from seconds
        progSecs = progSecs % 60;   //Shave off seconds which were converted into minutes
        totalSecs = totalSecs % 60; //Shave off seconds which were converted into minutes
        string progSecString = ((int)progSecs).ToString();   //Get string for seconds progressed
        string totalSecString = ((int)totalSecs).ToString(); //Get string for total seconds
        if (progSecString.Length == 1) progSecString = "0" + ((int)progSecs).ToString();    //Add zero ahead of number if necessary
        if (totalSecString.Length == 1) totalSecString = "0" + ((int)totalSecs).ToString(); //Add zero ahead of number if necessary
        timeStamp.text = progMins.ToString() + ":" + progSecString + "/" + totalMins.ToString() + ":" + totalSecString; //Generate and set timeStamp text
    }
}
