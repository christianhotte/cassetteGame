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

    private CassetteTape currentTape;
    private AudioSource audioSource;

    #endregion

    #region Settings



    #endregion

    #region Memory Vars

    private bool paused = false;
    private int pitchIndex;

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
        // if statements are an attempt to cover edge causes were the buttons are not pressed in the right order which causes things to break.
        if (audioSource.isPlaying)
        {
            return;
        }
        
        if (audioSource.clip == null)
        {
            audioSource.clip = currentTape.AudioFile;
        }


        if (paused)
        {
            audioSource.UnPause();
            paused = false;
            return;
        }

        audioSource.pitch = 1;
        audioSource.Play();

    }
    public void OnRewind()
    {
        CyclePitch(-1, 1);
    }
    public void OnFastForward()
    {
        CyclePitch(2, 1);
    }

    public void OnStopEject()
    {
        audioSource.Stop();
        audioSource.clip = null;
        currentTape = null;
    }
    public void OnPause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            paused = true;
        }
        
    }

    #endregion

    public void InsertTape(CassetteTape tape)
    {
        currentTape = tape;
    }

    private void CyclePitch(int pitch1, int pitch2)
    {
        // Cycles between the passed in pitches each time the method gets called
        int[] speeds = new[] { pitch1, pitch2 };
        audioSource.pitch = speeds[pitchIndex];
        pitchIndex += 1;
        if (pitchIndex > speeds.Length - 1)
        {
            pitchIndex = 0;
        }
    }
}
