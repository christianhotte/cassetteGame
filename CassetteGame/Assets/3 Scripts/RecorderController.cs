using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecorderController : MonoBehaviour
{
    //Description: Processes inputs and actions for the player's tape recorder.  Handles methods for manipulating tape objects

    // Replaced Comment categories with regions because regions > comments
    #region Classes, Enums & Structs



    #endregion

    #region Objects & Components

    public CassetteTape currentTape;

    private AudioSource audioSource;

    #endregion

    #region Settings



    #endregion

    #region Memory Vars



    #endregion

    #region RUNTIME METHODS

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }


    #endregion

    #region USER INPUT METHODS

    public void OnRecord()
    {

    }
    public void OnPlay()
    {
        // checks to make sure audio is not already playing then gets the audio file from the current tape
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(currentTape.AudioFile);
        }

    }
    public void OnRewind()
    {

    }
    public void OnFastForward()
    {

    }
    public void OnStopEject()
    {

    }
    public void OnPause()
    {

    }

    #endregion

}
