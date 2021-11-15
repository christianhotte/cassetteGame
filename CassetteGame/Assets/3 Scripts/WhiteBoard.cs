using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteBoard : MonoBehaviour
{
    private LineRenderer drawLine;
    private List<Vector3> linePoints;
    private float timer;

    private GameObject newline;
    [SerializeField] private float timerDelay;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float lineWidth;

    private bool hitWhiteboard;

    private void Awake()
    {
        linePoints = new List<Vector3>();
    }

    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            // Initialization for when a new line is created 
            newline = new GameObject();
            drawLine = newline.AddComponent<LineRenderer>();
            drawLine.material = new Material(Shader.Find("Sprites/Default"));
            drawLine.startColor = Color.red;
            drawLine.endColor = Color.red;
            drawLine.startWidth = lineWidth;
            drawLine.endWidth = lineWidth;
        }


        if (Input.GetMouseButton(0))
        {
            Debug.DrawRay(Camera.main.ScreenToWorldPoint(Input.mousePosition), GetMousePosition(), Color.red);
            timer -= Time.deltaTime;

            // Prevents too man
            if (timer <= 0)
            {
                // Trying to only activate drawing ability when ray hits whiteboard. But Doesn't seem to work
                if (hitWhiteboard)
                {
                    throw new NotImplementedException();
                }

                // Draws a line while left mouse is held down
                linePoints.Add(GetMousePosition());
                drawLine.positionCount = linePoints.Count;
                drawLine.SetPositions(linePoints.ToArray());
                timer = timerDelay;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            // clears points fron the list so a new line can be made
            linePoints.Clear();
        }

        if (Input.GetMouseButtonDown(1))
        {
            // Clears all lines from the scene 
            LineRenderer[] lines = GameObject.FindObjectsOfType<LineRenderer>();
            foreach (var line in lines)
            {
                Destroy(line.gameObject);
            }
        }
    }

    private Vector3 GetMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hitWhiteboard = Physics.Raycast(ray, 10, layerMask);
        var pos = ray.origin + ray.direction * 10;
        pos.z = 10;
        return pos;

    }
}
