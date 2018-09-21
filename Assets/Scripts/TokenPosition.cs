using System;
using System.Collections;
using System.Collections.Generic;
using TUIO;
using UniducialLibrary;
using UnityEngine;

public class TokenPosition
{
    private int beats;
    private int tunes;
    private int heightOffsetInPx;
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
        heightOffsetInPx = m_settings.heightOffSetInPx;
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

    public Vector3 CalculateGridPosition(int markerID, float cameraOffset, bool isLoopBarMarker, bool isJoker, FiducialController fiducialController, Vector3 oldPositionInScreen)
    {
        //does not change position if the marker is currently being played
        if (!isLoopBarMarker && m_lastComeLastServe.IsBeingPlayed(markerID))
            return fiducialController.gameObject.transform.position;

        TuioObject m_obj = m_tuioManager.GetMarker(markerID);
        //-80 and +40 and the next valid codeline solved a problem with the build reactivision-like table we used
        Vector3 position = new Vector3(m_obj.getX() * (Screen.width), isLoopBarMarker ? 0.5f * Screen.height : (1 - m_obj.getY()) * Screen.height, cameraOffset);
        //next line: see last comment
        //when the marker is snapped... 
        if (fiducialController.IsSnapped())
        {
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
                position.x = this.CalculateXPosition(position, isLoopBarMarker, m_settings.GetMarkerWidthMultiplier(markerID));
                #endregion

                #region Y-Axis
                //suggests the y Position because it's a joker marker
                if (isJoker)
                    position.y = fiducialController.gameObject.GetComponent<JokerMarker>().CalculateYPosition(position, fiducialController);
                //doesn't move object on y-axis, when it's a LoopBarMarker
                else if (!isLoopBarMarker)
                {
                    float snappingDistance = -cellHeightInPx / 2;

                    //if marker is below grid area
                    if (position.y < heightOffsetInPx + snappingDistance)
                        position.y = 0;
                    //if marker is above grid area
                    else if (position.y > gridHeightInPx + heightOffsetInPx - snappingDistance)
                        position.y = gridHeightInPx + heightOffsetInPx - cellHeightInPx;
                    //if marker is on grid area
                    else
                    {
                        float yPos = position.y - heightOffsetInPx - snappingDistance;
                        float markerYOffset = yPos % cellHeightInPx;
                        if (markerYOffset < cellHeightInPx / 2)
                            position.y = yPos - markerYOffset;
                        else
                            position.y = yPos - markerYOffset + cellHeightInPx;
                    }
                    position.y += (heightOffsetInPx + snappingDistance);
                }
                #endregion

                fiducialController.SetIsSnapped(true);
                fiducialController.SetLastTimeSnapped(Time.time);
            }
            //if the marker is moving, the position will be set in the return statement
            //else{}
        }
        return this.m_MainCamera.ScreenToWorldPoint(position);
    }

    //In screen space
    public float CalculateXPosition(Vector3 position, bool isLoopBarMarker, float markerWidthMultiplier)
    {
        float snappingDistance = cellWidthInPx * markerWidthMultiplier;//different marker sizes have effects on snapping distances

        if (isLoopBarMarker) snappingDistance = 0;

        //if marker is left of grid area
        if (position.x < widthOffsetInPx + snappingDistance)
        {
            Debug.Log("position.x < widthOffsetInPx + snappingDistance: " + position.x + " < " + widthOffsetInPx + " + " + snappingDistance);
            Debug.Log("Setting position.x to 0.");
            position.x = 0;
        }
        //if marker is above grid area
        else if (position.x > gridWidthInPx + widthOffsetInPx - snappingDistance)
        {
            Debug.Log("position.x > gridWidthInPx + widthOffsetInPx - snappingDistance: " + position.x + " > " + gridWidthInPx + " + " + widthOffsetInPx + " - " + snappingDistance);
            position.x = gridWidthInPx + widthOffsetInPx - 2 * snappingDistance;
        }
        //if marker is on grid area
        else
        {
            Debug.Log("Marker is on grid area");
            float xPos = position.x - widthOffsetInPx - snappingDistance;
            float markerXOffset = xPos % cellWidthInPx;
            if (markerXOffset < cellWidthInPx / 2)
                position.x = xPos - markerXOffset;
            else
                position.x = xPos - markerXOffset + cellWidthInPx;

        }
        position.x += (widthOffsetInPx + snappingDistance);
        return position.x;
    }

    public bool MovedFurtherThanThreshold(Vector3 pos1, Vector3 pos2, bool isJoker)
    {
        return isJoker ? Math.Abs(pos1.x - pos2.x) > movementThreshold.x : (Math.Abs(pos1.x - pos2.x) > movementThreshold.x || Math.Abs(pos1.y - pos2.y) > movementThreshold.y);
    }

    #region For OuterLinesForOrientation.cs and LoopController.cs Start()
    public float GetXPosForBeat(int beat)
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(beat * cellWidthInPx + widthOffsetInPx, 0, 0)).x;
    }

    public float GetYPosForTune(int tune)
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(0, tune * cellHeightInPx + heightOffsetInPx + cellHeightInPx / 2, 0)).y;
    }
    #endregion

}
