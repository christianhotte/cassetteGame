using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CassetteTape : MonoBehaviour
{
    // This is the script for the cassette tape object, contains reference to the audio file as well as click and drag functionality 

    #region Public
    

    #endregion

    #region Privite

    // Variables for click and drag system 
    private Camera cam1;

    private float startPosX;
    private float startPosY;
    private bool isBeingHeld = false;

    #endregion

    #region Inspector Fields
    [field: SerializeField]
    public AudioClip AudioFile { get; set; }

    #endregion

    private void Awake()
    {
        cam1 = Camera.main;
    }

    // Update is called once per frame
    private void Update()
    {
        MoveCassette();
    }

    #region Click And Drag

    // Let's player click and drag the cassette tape around

    private void MoveCassette()
    {
        if (isBeingHeld)
        {
            var mousePos = GetMousePos();

            // Sets the object to the point the user is clicking
            transform.localPosition = new Vector3(mousePos.x - startPosX, mousePos.y - startPosY, 0);
        }
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var mousePos = GetMousePos();

            var pos = transform.localPosition;

            // Get's the point the user is clicking instead of the center of the object. This avoids snapping.
            startPosX = mousePos.x - pos.x;
            startPosY = mousePos.y - pos.y;

            isBeingHeld = true;
        }
    }

    private void OnMouseUp()
    {
        isBeingHeld = false;
    }

    // Helper Methods
    private Vector3 GetMousePos()
    {
        // Converts mouse position to vector 3
        var mousePos = Input.mousePosition;
        mousePos = cam1.ScreenToWorldPoint(mousePos);

        return mousePos;
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Recorder"))
        {
            // When the cassette tape is dragged on to the recorder the cassette gets passed as the current tape
            var recorder = other.gameObject.GetComponent<RecorderController>();
            recorder.InsertTape(this);
        }
    }
}
