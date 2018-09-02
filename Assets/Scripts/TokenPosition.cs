﻿using System;
using System.Collections;
using System.Collections.Generic;
using TUIO;
using UniducialLibrary;
using UnityEngine;

public class TokenPosition
{
    private int bars = 16; // Anzahl der Takte
    private int tunes = 24; //Anzahl der Töne
    private int heightOffset = 20; //in pixels
    private int widthOffset = 20; //in pixels

    private Camera m_MainCamera;
    private float gridHeight;
    private float gridWidth;
    private float cellWidth;
    public Vector2 cellSizeWorld;
    private float cellHeight;

    //--von Raphael--
    private Vector2 minWorldCoords;
    private Vector2 maxWorldCoords;
    private Vector2 worldDiff;
    //---------------

    //for Movement threshold
    private float movementThreshold;

    private TuioManager tuioManager;
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
        tuioManager = TuioManager.Instance;
        m_MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        //variables in world coordinates
        widthOffset = m_MainCamera.pixelWidth / 64;
        heightOffset = m_MainCamera.pixelHeight / 64;
        minWorldCoords = this.m_MainCamera.ScreenToWorldPoint(new Vector2(0 + (widthOffset /** 2*/), 0 + heightOffset));//why widhtOffset *2? --> outcommented
        maxWorldCoords = this.m_MainCamera.ScreenToWorldPoint(new Vector2(m_MainCamera.pixelWidth - (widthOffset /** 2*/), m_MainCamera.pixelHeight - heightOffset));//why widthOffset * 2? --> outcommented
        worldDiff = maxWorldCoords - minWorldCoords;

        //calculate snapping grid
        gridHeight = m_MainCamera.pixelHeight - heightOffset * 2;
        gridWidth = m_MainCamera.pixelWidth - widthOffset * 2;
        cellHeight = gridHeight / tunes;
        cellWidth = gridWidth / bars;

        cellSizeWorld = Vector2.zero;
        cellSizeWorld.x = worldDiff.x / bars;
        cellSizeWorld.y = worldDiff.y / tunes;

        //TODO add to settings class
        var movementThresholdInWorld = (new Vector2(this.GetCellWidthInWorldLength() / 2, this.GetCellHeightInWorldLength() / 2));
        this.movementThreshold = Vector2.Distance(Camera.main.WorldToScreenPoint(Vector2.zero), Camera.main.WorldToScreenPoint(movementThresholdInWorld));//new Vector3(movementThresholdInWorld.x * Screen.width, (1 - movementThresholdInWorld.y) * Screen.height, this.CameraOffset);

    }

    public int GetNote(Vector2 pos)
    {
        var relativeYpos = (pos.y - minWorldCoords.y) / (cellSizeWorld.y * tunes);
        return (int)Mathf.Floor(relativeYpos * tunes);
    }
    public int GetTactPosition(Vector2 pos)
    {
        var relativeXpos = (pos.x - minWorldCoords.x) / (cellSizeWorld.x * bars);
        return (int)Mathf.Floor(relativeXpos * bars);
    }

    public Vector3 CalculateGridPosition(int markerID, float cameraOffset, bool isLoopBarMarker, bool isJoker, FiducialController fiducialController, Vector3 oldPositionInScreen)
    {
        TuioObject m_obj = tuioManager.GetMarker(markerID);
        Vector3 position = new Vector3(m_obj.getX() * Screen.width, isLoopBarMarker ? 0.5f * Screen.height : (1 - m_obj.getY()) * Screen.height, cameraOffset);

        //when the marker is snapped... 
        if (fiducialController.IsSnapped())
        {
            //...and the new position is NOT far away enough from the old position, then set position to oldPosition 
            if (Vector2.Distance(position, oldPositionInScreen) < movementThreshold)
                position = oldPositionInScreen;
            //...and the new position is far away enoug from the old position, set snapped to false
            else
                fiducialController.SetIsSnapped(false);
        }
        //otherwise, if marker is NOT snapped...
        else if(!fiducialController.IsSnapped())
        {
            //...and motion speed is zero, snap him to nearest grid position and set snapped to true
            if (m_obj.getMotionSpeed() == 0)
            {
                #region X-Axis
                position.x = this.CalculateXPosition(position, isLoopBarMarker, GetMarkerWithMultiplier(markerID));
                #endregion

                #region Y-Axis
                //suggests the y Position because it's a joker marker
                if (isJoker)
                {
                    position.y = fiducialController.gameObject.GetComponent<JokerMarker>().CalculateYPosition(position, fiducialController);
                }
                //doesn't move object on y-axis, when it's a LoopBarMarker
                else if (!isLoopBarMarker)
                {
                    float snappingDistance = cellHeight / 2;

                    //if marker is below grid area
                    if (position.y < heightOffset + snappingDistance)
                        position.y = 0;
                    //if marker is above grid area
                    else if (position.y > gridHeight + heightOffset - snappingDistance)
                        position.y = gridHeight + heightOffset - cellHeight;
                    //if marker is on grid area
                    else
                    {
                        float yPos = position.y - heightOffset - snappingDistance;
                        float markerYOffset = yPos % cellHeight;
                        if (markerYOffset < cellHeight / 2)
                            position.y = yPos - markerYOffset;
                        else
                            position.y = yPos - markerYOffset + cellHeight;
                    }
                    position.y += (heightOffset + snappingDistance);
                }
                #endregion

                fiducialController.SetIsSnapped(true);
            }
            //if the marker is moving, the position will be set in the return statement
            //else{}
        }
        return this.m_MainCamera.ScreenToWorldPoint(position);
    }

    //In screen space
    public float CalculateXPosition(Vector3 position, bool isLoopBarMarker, float markerWidthMultiplier)
    {
        float snappingDistance = cellWidth / 2 + cellWidth * markerWidthMultiplier;//different marker sizes have effects on snapping distances
        if (isLoopBarMarker)
            snappingDistance = cellWidth / 2;

        //if marker is below grid area
        if (position.x < widthOffset + snappingDistance)
            position.x = 0;
        //if marker is above grid area
        else if (position.x > gridWidth + widthOffset - snappingDistance)
            position.x = gridWidth + widthOffset - 2 * snappingDistance;
        //if marker is on grid area
        else
        {
            float xPos = position.x - widthOffset - snappingDistance;
            float markerXOffset = xPos % cellWidth;
            if (markerXOffset < cellWidth / 2)
                position.x = xPos - markerXOffset;
            else
                position.x = xPos - markerXOffset + cellWidth;

        }
        position.x += (widthOffset + snappingDistance);
        return position.x;
    }

    //used for correct snapping on the x axis and sprite scale
    //TOOD Link with settings class
    public static float GetMarkerWithMultiplier(int markerID)
    {
        return markerID < 13 ? 0.5f : (markerID < 23 ? 1 : (markerID < 30 ? 1.5f : 2));
    }

    public float GetCellWidthInWorldLength()
    {
        return cellSizeWorld.x;
    }

    public float GetCellHeightInWorldLength()
    {
        return cellSizeWorld.y;
    }

    #region For JokerSuggestion
    public float GetHeightOffset()
    {
        return heightOffset;
    }

    public float GetCellHeightInPx()
    {
        return cellHeight;
    }
    #endregion

    #region For OuterLinesForOrientation
    public float GetXPosForBeat(int beat)
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(beat * cellWidth + widthOffset + cellWidth / 2, 0, 0)).x;
    }

    public float GetYPosForTune(int tune)
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(0, tune * cellHeight + heightOffset + cellHeight / 2, 0)).y;
    }

    public int GetNumberOfBeats()
    {
        return bars;
    }

    public int GetNumberOfTunes()
    {
        return tunes;
    }
    #endregion

}
