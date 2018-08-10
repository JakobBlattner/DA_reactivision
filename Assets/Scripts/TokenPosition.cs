using System.Collections;
using System.Collections.Generic;
using TUIO;
using UniducialLibrary;
using UnityEngine;

public class TokenPosition 
{
    private int bars = 16; // Anzahl der Takte
    private int tunes = 23;
    private int heightOffset = 20; //in pixels
    private int widthOffset = 20; //in pixels

    private Camera m_MainCamera;
    private float gridHeight;
    private float gridWidth;
    private float cellWidth;
    public Vector2 cellSizeWorld;
    private float cellHeight;




    Vector2 minWorldCoords;
    Vector2 maxWorldCoords;
    Vector2 worldDiff;








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

        // 
        minWorldCoords = this.m_MainCamera.ScreenToWorldPoint(new Vector2(0 + (widthOffset * 2), 0 + heightOffset));
        maxWorldCoords = this.m_MainCamera.ScreenToWorldPoint(new Vector2(m_MainCamera.pixelWidth - (widthOffset * 2), m_MainCamera.pixelHeight - heightOffset));
        worldDiff = maxWorldCoords - minWorldCoords;

        //calculate snapping grid
        gridHeight = m_MainCamera.pixelHeight - heightOffset * 2;
        gridWidth = m_MainCamera.pixelWidth - widthOffset * 2;
        cellHeight = gridHeight / tunes;
        cellWidth = gridWidth / bars;


        cellSizeWorld = Vector2.zero;
        //cellSizeWorld.x = cellWidth / (m_MainCamera.pixelWidth / (bars + 2));
        cellSizeWorld.x = worldDiff.x / bars;
        cellSizeWorld.y = worldDiff.y / tunes;
    }

    public int GetNote(Vector2 pos) {
        var relativeYpos = (pos.y - minWorldCoords.y) / (cellSizeWorld.y * tunes);
        return (int)Mathf.Floor(relativeYpos * tunes);
    }
    public int GetTactPosition(Vector2 pos) {
        var relativeXpos = (pos.x - minWorldCoords.x) / (cellSizeWorld.x * bars);
        return (int)Mathf.Floor(relativeXpos * bars);
    }

    public Vector3 CalculateGridPosition(int markerID, float cameraOffset, bool isLoopBarMarker)
    {
        TuioObject m_obj = tuioManager.GetMarker(markerID);
        Vector3 position = new Vector3(m_obj.getX() * Screen.width, isLoopBarMarker ? 0.5f*Screen.height : (1 - m_obj.getY()) * Screen.height, cameraOffset);

        //if marker is not moving snap to grid position
        if (m_obj.getMotionSpeed() == 0)
        {
            #region X-Axis
            position.x = this.CalculateXPosition(position);
            #endregion

            #region Y-Axis
            //doesn't move object on y-axis, when it's a LoopBarMarker
            if (!isLoopBarMarker)
            {
                //if marker is below grid area
                if (position.y < heightOffset)
                    position.y = 0;
                //if marker is above grid area
                else if (position.y > gridHeight + heightOffset)
                    position.y = gridHeight + heightOffset - cellHeight;
                //if marker is on grid area
                else
                {
                    float yPos = position.y - heightOffset;
                    float markerYOffset = yPos % cellHeight;
                    if (markerYOffset < cellHeight / 2)
                        position.y = yPos - markerYOffset;
                    else
                        position.y = yPos - markerYOffset + cellHeight;
                }
                position.y += heightOffset;
            }
            #endregion 
        }

        return this.m_MainCamera.ScreenToWorldPoint(position);
    }

    //In screen space
    public float CalculateXPosition(Vector3 position)
    {
        //if marker is below grid area
        if (position.x < widthOffset)
            position.x = 0;
        //if marker is above grid area
        else if (position.x > gridWidth + widthOffset)
            position.x = gridWidth + widthOffset- cellWidth;
        //if marker is on grid area
        else
        {
            float xPos = position.x - widthOffset;
            float markerXOffset = xPos % cellWidth;
            if (markerXOffset < cellWidth / 2)
                position.x = xPos - markerXOffset;
            else
                position.x = xPos - markerXOffset + cellWidth;

        }
        position.x += widthOffset;
        return position.x;
    }

    public float GetCellWidthInWorldLength()
    {
        return cellSizeWorld.x;
    }

}
