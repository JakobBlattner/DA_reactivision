﻿using System;
using System.Collections;
using System.Collections.Generic;
using TUIO;
using UniducialLibrary;
using UnityEngine;

public class TokenPosition
{
    private int beats;
    private int tunes;
    private int heightOffsetInPx_top;
    private int heightOffsetInPx_bottom;
    private int widthOffsetInPx;

    //in px
    private Camera m_MainCamera;
    private float gridHeightInPx;
    private float gridWidthInPx;
    private float cellWidthInPx;
    public Vector2 cellSizeWorld;
    private float cellHeightInPx;

    //in World coords
    private Vector2 minWorldCoords;
    private Vector2 maxWorldCoords;
    private Vector2 worldDiff;

    //for Movement threshold
    private Vector2 movementThreshold;
    private Vector3 realOldPos;

    private TuioManager m_tuioManager;
    private Settings m_settings;
    private LastComeLastServe m_lastComeLastServe;
    private static TokenPosition m_Instance;

    public static TokenPosition Instance
    {
        get
        {
            if (m_Instance == null)
            {
                new TokenPosition();
            }

            return m_Instance;
        }
    }

    public TokenPosition()
    {
        if (m_Instance != null)
        {
            Debug.LogError("Trying to create two instances of singleton.");
            return;
        }

        m_Instance = this;

        //init variables
        m_tuioManager = TuioManager.Instance;
        m_settings = Settings.Instance;
        m_lastComeLastServe = GameObject.FindObjectOfType<LastComeLastServe>();
        m_MainCamera = GameObject.FindGameObjectWithTag(m_settings.mainCameraTag).GetComponent<Camera>();

        beats = m_settings.beats;
        tunes = m_settings.tunes;

        //variables in world coordinates
        widthOffsetInPx = m_settings.widthOffSetInPx;
        heightOffsetInPx_top = m_settings.heightOffSetInPx_top;
        heightOffsetInPx_bottom = m_settings.heightOffSetInPx_bottom;
        minWorldCoords = m_settings.minWorldCoords;
        maxWorldCoords = m_settings.maxWorldCoords;
        worldDiff = m_settings.worldDiff;

        //calculate snapping grid
        gridHeightInPx = m_settings.gridHeightInPx;
        gridWidthInPx = m_settings.gridWidthInPx;
        cellHeightInPx = m_settings.cellHeightInPx;
        cellWidthInPx = m_settings.cellWidthInPx;
        cellSizeWorld = m_settings.cellSizeWorld;

        movementThreshold = m_settings.movementThreshold;
    }

    public int GetNote(Vector2 pos)
    {
        var relativeYpos = (pos.y - minWorldCoords.y) / (cellSizeWorld.y * tunes);
        return (int)Mathf.Floor(relativeYpos * tunes);
    }
    public int GetTactPosition(Vector2 pos)
    {
        var relativeXpos = (pos.x - minWorldCoords.x) / (cellSizeWorld.x * beats);
        return relativeXpos < 0 ? 0 : (relativeXpos >= 1 ? 15 : (int)Mathf.Floor(relativeXpos * beats));
    }

    public float GetTactPositionForLoopBarMarker(Vector2 pos)
    {
        var relativeXpos = (pos.x - minWorldCoords.x) / (cellSizeWorld.x * beats);
        return relativeXpos * beats;
    }

    public Vector3 CalculateGridPosition(int markerID, float cameraOffset, bool isLoopBarMarker, bool isJoker, FiducialController fiducialController, Vector3 oldPositionInScreen)
    {
        //does not change position if the marker is currently being played
        if (!isLoopBarMarker && m_lastComeLastServe.IsBeingPlayed(markerID))
        {
            return fiducialController.gameObject.transform.position;
        }

        TuioObject m_obj = m_tuioManager.GetMarker(markerID);
        Vector3 position = new Vector3(m_obj.getX() * (Screen.width), isLoopBarMarker ? 0.5f * Screen.height : (1 - m_obj.getY()) * Screen.height, cameraOffset);
        //when the marker is snapped... 
        if (fiducialController.IsSnapped())
        {
            position.x = this.CalculateXPosition(position, isLoopBarMarker, m_settings.GetMarkerWidthMultiplier(markerID), false); // calculate x position while not moving
            //reads correctOldPos if marker is a JokerMarkers
            if (isJoker)
                realOldPos = new Vector3(oldPositionInScreen.x, fiducialController.gameObject.GetComponent<JokerMarker>().GetRealOldYPosition(), oldPositionInScreen.z);

            //...and the new position is NOT far away enough from the old position (different for Joker Markers), then set position to oldPosition 
            if (isJoker ? !this.MovedFurtherThanThreshold(position, realOldPos, isJoker) : !this.MovedFurtherThanThreshold(position, oldPositionInScreen, isJoker))
                position = oldPositionInScreen;
            //...and the new position is far away enoug from the old position, set snapped to false
            else if (this.MovedFurtherThanThreshold(position, oldPositionInScreen, isJoker))
                fiducialController.SetIsSnapped(false);
        }
        //otherwise, if marker is NOT snapped...
        else if (!fiducialController.IsSnapped())
        {
            //...and motion speed is zero, snap him to nearest grid position, set snapped to true and save the time of snapping (for lastcomelastserve algorithm)
            if (m_obj.getMotionSpeed() == 0)
            {
                #region X-Axis
                position.x = this.CalculateXPosition(position, isLoopBarMarker, m_settings.GetMarkerWidthMultiplier(markerID), false); // calculate x position while not moving
                #endregion

                #region Y-Axis
                //suggests the y Position because it's a joker marker
                if (isJoker)
                    position.y = fiducialController.gameObject.GetComponent<JokerMarker>().CalculateYPosition(position, fiducialController, this.GetTactPosition(Camera.main.ScreenToWorldPoint(position)));
                else if (!isLoopBarMarker)
                {
                    float snappingDistance = -cellHeightInPx / 2;

                    //if marker is below grid area
                    if (position.y < heightOffsetInPx_top + snappingDistance)
                        position.y = 0;
                    //if marker is above grid area
                    else if (position.y > gridHeightInPx + heightOffsetInPx_bottom - snappingDistance)
                        position.y = gridHeightInPx + heightOffsetInPx_bottom - cellHeightInPx;
                    //if marker is on grid area
                    else
                    {
                        float yPos = position.y - heightOffsetInPx_bottom - snappingDistance;
                        float markerYOffset = yPos % cellHeightInPx;
                        if (markerYOffset < cellHeightInPx / 2)
                            position.y = yPos - markerYOffset;
                        else
                            position.y = yPos - markerYOffset + cellHeightInPx;
                    }
                    position.y += (heightOffsetInPx_bottom + snappingDistance);
                }
                #endregion

                //check if another tune is currently being played, if so: snap marker
                if(!m_lastComeLastServe.IsOtherMarkerBeingPlayedAtThisBeat(this.GetTactPosition(m_lastComeLastServe.markers[beats].transform.position))){
                    fiducialController.SetIsSnapped(true);
                    fiducialController.SetLastTimeSnapped(Time.time);
                }
            }
            else
            {
                position.x = this.CalculateXPosition(position, isLoopBarMarker, m_settings.GetMarkerWidthMultiplier(markerID), true); // calculate x position while moving 
            }
        }
        return this.m_MainCamera.ScreenToWorldPoint(position);
    }

    //In screen space
    public float CalculateXPosition(Vector3 position, bool isLoopBarMarker, float markerWidthMultiplier, bool isMoving)
    {
        float snappingDistance = cellWidthInPx * markerWidthMultiplier;//different marker sizes have effects on snapping distances

        if (isLoopBarMarker) snappingDistance = 0;

        //if marker is left of grid area
        if (position.x < widthOffsetInPx + snappingDistance)
            position.x = widthOffsetInPx + snappingDistance;
        //if marker is right of grid area
        else if (position.x > gridWidthInPx + widthOffsetInPx - snappingDistance)
        {
            position.x = gridWidthInPx - 2 * snappingDistance;
            position.x += (widthOffsetInPx + snappingDistance);
        }
        //if marker is on grid area
        else
        {
            //and not moving
            if (!isMoving)
            {
                float xPos = position.x - widthOffsetInPx - snappingDistance;
                float markerXOffset = xPos % cellWidthInPx;
                if (markerXOffset < cellWidthInPx / 2)
                    position.x = xPos - markerXOffset;
                else
                    position.x = xPos - markerXOffset + cellWidthInPx;
                position.x += (widthOffsetInPx + snappingDistance);
            }
        }

        return position.x;
    }

    //checks if token has moved further than a certain threshold
    public bool MovedFurtherThanThreshold(Vector3 pos1, Vector3 pos2, bool isJoker)
    {
        return isJoker ? Math.Abs(pos1.x - pos2.x) > movementThreshold.x : (Math.Abs(pos1.x - pos2.x) > movementThreshold.x || Math.Abs(pos1.y - pos2.y) > movementThreshold.y);
    }

    public float GetXPosForBeat(int beat)
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(beat * cellWidthInPx + widthOffsetInPx, 0, 0)).x;
    }

    public float GetYPosForTune(int tune)
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(0, tune * cellHeightInPx + heightOffsetInPx_bottom + cellHeightInPx / 2, 0)).y;
    }
}
