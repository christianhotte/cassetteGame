using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteBoard : MonoBehaviour
{
    public static WhiteBoard main;

    private LineRenderer drawLine;
    private List<Vector3> linePoints;
    private float timer;
    private Transform lineContainer;

    private GameObject newline;
    [SerializeField] private float timerDelay;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float lineWidth;
    public float deployedPosition;
    public float stowedPosition;
    public float slideSpeed;

    private bool hitWhiteboard;
    private bool endLine;
    private Vector3 linesOffset;
    private bool deployed;

    public bool debugToggleStow;

    private void Awake()
    {
        if (main == null) main = this; else Destroy(this); //Singleton-ize this script
        linePoints = new List<Vector3>();
        lineContainer = transform.parent.Find("Lines");
        linesOffset = transform.position - lineContainer.position;
    }

    private void Update()
    {
        //Debug Stuff:
        if (debugToggleStow) { ToggleStow(deployed); debugToggleStow = false; }

        //Dumb Physics Stuff:
        Vector3 targetPosition = transform.localPosition;
        if (deployed)
        {
            targetPosition.y = deployedPosition;
        }
        else
        {
            targetPosition.y = stowedPosition;
        }
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, slideSpeed * Time.deltaTime * 60);
        lineContainer.position = transform.position + linesOffset;

        //Input:
        if (deployed)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Initialization for when a new line is created 
                newline = new GameObject();
                newline.transform.parent = lineContainer;
                drawLine = newline.AddComponent<LineRenderer>();
                drawLine.material = new Material(Shader.Find("Sprites/Default"));
                drawLine.startColor = Color.red;
                drawLine.endColor = Color.red;
                drawLine.startWidth = lineWidth;
                drawLine.endWidth = lineWidth;
                drawLine.useWorldSpace = false;
            }
            if (Input.GetMouseButton(0))
            {
                Debug.DrawRay(Camera.main.ScreenToWorldPoint(Input.mousePosition), GetMousePosition(), Color.red);
                timer -= Time.deltaTime;

                // Prevents too man
                if (timer <= 0)
                {
                    // Trying to only activate drawing ability when ray hits whiteboard. But Doesn't seem to work
                    if (endLine)
                    {
                        endLine = false;
                        linePoints.Clear();
                    }
                    else if (hitWhiteboard)
                    {
                        // Draws a line while left mouse is held down
                        linePoints.Add(GetMousePosition());
                        drawLine.positionCount = linePoints.Count;
                        drawLine.SetPositions(linePoints.ToArray());
                        timer = timerDelay;
                        //throw new NotImplementedException();
                    }
                
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
    }

    private Vector3 GetMousePosition()
    {
        bool prevHitWhiteboard = hitWhiteboard;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        hitWhiteboard = GetComponent<Collider>().Raycast(ray, out hitInfo, 10);
        var pos = hitInfo.point;
        //hitWhiteboard = Physics.Raycast(ray, 10, layerMask);
        //var pos = ray.origin + ray.direction * 10;
        //pos.z = 10;
        if (prevHitWhiteboard != hitWhiteboard && hitWhiteboard == false) endLine = true;
        return pos;

    }
    public void ToggleStow(bool stow)
    {
        //Function: Stows or deploys whiteboard (enabling/disabling its functionality)

        //Initialization:
        if (stow != deployed) return; //Redundancy check
        deployed = !stow; //Toggle deployment state
    }
}
