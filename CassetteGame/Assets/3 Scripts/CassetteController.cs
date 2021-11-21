using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CassetteController : MonoBehaviour, IHoldable
{
    //Function: Goes on Cassette Tape object, contains data and methods for interacting with tape

    //Objects & Components:
    public string clipName;          //Name of this recording (to display to player)
    private AudioSource audioSource; //The audio source used to play this tape
    private Transform model;         //Model to move
    internal TouchManager.TouchData holdingTouch; //Touch holding this tape (if tape is being held)

    //Positions:
    [Header("Positional References:")]
    public Transform heldPosition;   //Positional reference for when tape is being held
    public Transform originPosition; //Positional reference for tape's original placement

    //Settings:
    public float holdLerpSpeed; //How fast tape snaps to finger position when held

    //Status Vars:
    [ShowOnly] public float progress = 0; //How wound/unwound this tape currently is (between 0 and 1)
    internal bool inserted = false;       //Whether or not tape is inserted into player

    //Runtime Methods:
    private void Awake()
    {
        //Get Components:
        audioSource = GetComponent<AudioSource>(); //Get audio source on tape object
        model = transform.GetChild(0);             //Get transform of cassette model
    }
    private void Update()
    {
        if (holdingTouch != null) //Cassette tape is currently being held
        {

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
    }
    public void Release()
    {
        //Function: Called by InputManager when this tape is released (from a holding touch)

        //Handshake:
        holdingTouch.heldObject = null; //Dissociate this script from releasing touch
        holdingTouch = null;            //Dissociate releasing touch from this script
    }

    //Audio Methods:

}
