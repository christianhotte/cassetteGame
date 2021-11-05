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

    // Due to the audio system being not the best I need two audio sources or else things break easily. 0 = normal audio clips, 1 = OneShot sound effects.
    private AudioSource[] audioSources;

    #endregion

    #region Settings



    #endregion

    #region Memory Vars

    private bool paused = false;
    private int pitchIndex;

    [SerializeField] private List<AudioClip> miscSounds = new List<AudioClip>();

    #endregion

    #region RUNTIME METHODS

    private void Awake()
    {
        audioSources = GetComponents<AudioSource>();
    }


    #endregion

    #region USER INPUT METHODS

    public void OnRecord()
    {
        audioSources[1].PlayOneShot(miscSounds[2]);
    }
    public void OnPlay()
    {
        // order matters, if this line is placed at the end of the function then the sound does not play 
        audioSources[1].PlayOneShot(miscSounds[2]);

        // if statements are an attempt to cover edge causes were the buttons are not pressed in the right order which causes things to break.
        if (audioSources[0].isPlaying || currentTape == null)
        {
            return;
        }
        
        if (audioSources[0].clip == null)
        {
            audioSources[0].clip = currentTape.AudioFile;
        }


        if (paused)
        {
            audioSources[0].UnPause();
            paused = false;
            return;
        }
        
        audioSources[0].pitch = 1;
        audioSources[0].Play();

    }
    public void OnRewind()
    {
        audioSources[1].PlayOneShot(miscSounds[2]);
        CyclePitch(-1, 1);
    }
    public void OnFastForward()
    {
        audioSources[1].PlayOneShot(miscSounds[2]);
        CyclePitch(2, 1);
    }

    public void OnStopEject()
    {
        audioSources[0].Stop();
        audioSources[0].clip = null;
        currentTape = null;
        audioSources[1].PlayOneShot(miscSounds[1]);
    }
    public void OnPause()
    {
        audioSources[1].PlayOneShot(miscSounds[2]);
        if (audioSources[0].isPlaying)
        {
            audioSources[0].Pause();
            paused = true;

        }


    }

    #endregion

    public void InsertTape(CassetteTape tape)
    {
        currentTape = tape;
        audioSources[1].PlayOneShot(miscSounds[0]);
    }

    private void CyclePitch(int pitch1, int pitch2)
    {
        // Cycles between the passed in pitches each time the method gets called
        int[] speeds = new[] { pitch1, pitch2 };
        audioSources[0].pitch = speeds[pitchIndex];
        pitchIndex += 1;
        if (pitchIndex > speeds.Length - 1)
        {
            pitchIndex = 0;
        }
    }
}
