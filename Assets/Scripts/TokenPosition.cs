using System.Collections;
using System.Collections.Generic;
using TUIO;
using UniducialLibrary;
using UnityEngine;

public class TokenPosition : MonoBehaviour
{

    private int bars = 4; // Anzahl der Takte
    private int tunes = 23;
    private int minimalUnit = 4; //minimale Einheit --> Unterteilung des Grids
    private int heightOffset = 100; //in pixels
    private int widthOffset = 20; //in pixels

    private Camera m_MainCamera;
    private float gridHeight;
    private float gridWidth;
    private float cellWidth;
    private float cellHeight;

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

        //calculate snapping grid
        gridHeight = m_MainCamera.pixelHeight - heightOffset * 2;
        gridWidth = m_MainCamera.pixelWidth - widthOffset * 2;
        cellHeight = gridHeight / tunes;
        cellWidth = gridWidth / (minimalUnit * bars);
    }

    public Vector3 CalculateGridPosition(int markerID, float cameraOffset)
    {
        TuioObject m_obj = tuioManager.GetMarker(markerID);
        Vector3 position = new Vector3(m_obj.getX() * Screen.width, (1 - m_obj.getY()) * Screen.height, cameraOffset);

        //if marker is not moving snap to grid position
        if (m_obj.getMotionSpeed() == 0)
        {
            #region X-Axis
            this.CalculateXPosition(position);
            #endregion

            #region Y-Axis
            //if marker is below grid area
            if (position.y < heightOffset)
                position.y = 0;
            //if marker is above grid area
            else if (position.y > gridHeight + heightOffset)
                position.y = gridHeight - cellHeight;
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
            position.x = gridWidth - cellWidth;
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

}
