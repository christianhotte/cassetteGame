using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CassetteController : MonoBehaviour
{
    //Function: Goes on Cassette Tape object, contains data and methods for interacting with tape

    //Objects & Components:
    public string clipName;          //Name of this recording (to display to player)
    private AudioSource audioSource; //The audio source used to play this tape
    private Transform model;         //Model to move
    internal TouchManager.TouchData holdingTouch; //Touch holding this tape (if tape is being held)

    //Settings:
    public float holdLerpSpeed; //How fast tape snaps to finger position when held

    //Status Vars:
    [ShowOnly] public float progress = 0; //How wound/unwound this tape currently is (between 0 and 1)
    internal bool availableToHold = true; //Whether or not tape is available to be held
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


    //Audio Methods:

}
