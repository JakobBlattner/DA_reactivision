using System;
using System.Collections;
using System.Collections.Generic;
using UniducialLibrary;
using UnityEngine;

public class LoopController : MonoBehaviour
{

    private TuioManager m_tuioManager;
    private TokenPosition m_tokenPosition;
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
    private Vector3 newPos;

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
        m_tokenPosition = TokenPosition.Instance;
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

                if (m_tuioManager.IsCursorAlive(cursor.GetComponent<TouchController>().CursorID) && //if cursor is active 
                    (((xTouchPos - xPos) > 0 && (xTouchPos - xPos) < touchArea) || //touch right of loopMarker 
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

            #region Grid Snapping
            newPos = Camera.main.WorldToScreenPoint(this.transform.position);
            newPos = Camera.main.ScreenToWorldPoint(new Vector3(m_tokenPosition.CalculateXPosition(newPos), newPos.y, newPos.z));

            this.transform.position = newPos;
            #endregion
        }
        //moves cursor
        else if (moving)
        {
            otherLoopMarkerXPos = Camera.main.WorldToScreenPoint(otherLoopMarker.transform.position).x;
            float m_CursorX = Camera.main.WorldToScreenPoint(movingCursor.transform.position).x;

            //cant move loop marker so that minimum loop area is undershot
            if ((startMarker && otherLoopMarkerXPos > (m_CursorX + minLoopArea)) //for the start marker
                || (!startMarker && otherLoopMarkerXPos < (m_CursorX - minLoopArea))) //for the end marker
                this.transform.position = new Vector3(movingCursor.transform.position.x, this.transform.position.y, this.transform.position.z);
        }

        #endregion

        if (startMarker)
        {
            //Put in different scripts
            //TODO: visualize bar between start and end Loop Marker
            //TODO: color area between start and end loop marker
            //TODO: move loop area
        }

    }
}
