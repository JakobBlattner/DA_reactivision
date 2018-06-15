using System;
using System.Collections;
using System.Collections.Generic;
using UniducialLibrary;
using UnityEngine;

public class LoopController : MonoBehaviour
{

    private TuioManager m_tuioManager;

    public bool startMarker = false;
    public int touchArea = 1;
    public int minLoopArea = 10;
    private GameObject otherLoopMarker;
    private bool moving = false;
    private float xPos;
    private float xTouchPos;
    private float otherLoopMarkerXPos;
    private GameObject movingCursor;
    private GameObject[] cursors;

    void Start()
    {
        GameObject[] loopMarkers = GameObject.FindGameObjectsWithTag("LoopGO");
        int counter = 0;

        foreach (GameObject loopMarker in loopMarkers)
        {
            if (loopMarker.GetComponent<LoopController>().startMarker)
                counter++;

            //save other LoopMarker
            if (loopMarker.GetComponent<LoopController>().startMarker != this.startMarker)
                otherLoopMarker = loopMarker;

            if (counter > 1)
            {
                Debug.LogError("More than one Loop Start Gameobject defined. Must be exactely one.");
                break;
            }
        }
        if (counter == 0)
            Debug.LogError("No Loop Start GameObject defined. Must be exactely one.");

        m_tuioManager = TuioManager.Instance;
    }

    void Update()
    {
        cursors = GameObject.FindGameObjectsWithTag("Cursor");

        #region Move Loop Markers

        if (!moving)
        {
            foreach (GameObject cursor in cursors)
            {
                //converts from World to Screen space --> no negative values
                xTouchPos = Camera.main.WorldToScreenPoint(cursor.transform.position).x;
                xPos = Camera.main.WorldToScreenPoint(this.transform.position).x;

                if ((((xTouchPos - xPos) > 0 && (xTouchPos - xPos) < touchArea) || //touch right of loopMarker 
                    ((xPos - xTouchPos) > 0 && (xPos - xTouchPos) < touchArea)) //touch left of loopMarker
                    && !cursor.GetComponent<TouchController>().movingLoopMarker) //cursor is not already moving another loopMarker
                {
                    moving = true;
                    movingCursor = cursor;
                    movingCursor.GetComponent<TouchController>().movingLoopMarker = true;
                    break; //prevents pingpong between more than one cursor
                }
            }
        }
        //sets moving to false if cursor is not active anymore
        else if (moving && !m_tuioManager.IsCursorAlive(movingCursor.GetComponent<TouchController>().CursorID))
        {
            moving = false;
            movingCursor.GetComponent<TouchController>().movingLoopMarker = false;
            movingCursor = null;
        }
        //moves cursor
        else if (moving)
        {
            otherLoopMarkerXPos = Camera.main.WorldToScreenPoint(otherLoopMarker.transform.position).x;
            float m_CursorX = Camera.main.WorldToScreenPoint(movingCursor.transform.position).x;

            Debug.Log(m_CursorX + " " + otherLoopMarkerXPos);

            //cant move cursor so that minimum loop area is undershot
            if ((startMarker && otherLoopMarkerXPos > (m_CursorX + minLoopArea)) //for the start Marker
                || (!startMarker && otherLoopMarkerXPos < (m_CursorX - minLoopArea))) //for the end Marker
                this.transform.position = new Vector3(movingCursor.transform.position.x, this.transform.position.y, this.transform.position.z);
        }

        #endregion

        if (startMarker)
        {
            //TODO: visualize bar between start and end Loop Marker
            //TODO: color area between start and end loop marker

            //Put in different scripts?

            #region Move Loop Area
            //TODO: move loop area

            #endregion
        }

    }
}
