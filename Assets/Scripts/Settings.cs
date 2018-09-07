using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TUIO;
using UniducialLibrary;


/**
 This class consists of all global variables and their values
 **/
public class Settings : MonoBehaviour
{

    private TuioManager tuioManager;
    private static Settings m_settings;

    //communication values
    public readonly int serialBaudrate = 9600;
    public readonly string[] serialPortNames = { "COM1", "COM2", "COM3", "COM4", "/dev/cu.usbmodem1421" };
    public readonly string[] serialPortNamesBpm = { "COM6", "/dev/cu.usbmodem1411" };
    public readonly Vector2 locationBarOffset;

    //color values
    public readonly Color red = new Color(220, 0, 0, 255);
    public readonly Color green = new Color(0, 200, 0, 255);
    public readonly Color blue = new Color(0, 0, 255, 255);
    public readonly Color grey = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    //tag names
    public readonly string markerTag = "Marker";
    public readonly string jokerParentTag = "JokerParent";
    public readonly string mainCameraTag = "MainCamera";
    public readonly string loopMarkerTag = "LoopGO";
    public readonly int startLoopBarMarkerID;
    public readonly int endLoopBarMarkerID;

    //music stuff
    public readonly int bpm = 100;
    public readonly int tunes = 24;
    public readonly int beats = 16;
    public readonly int tunesPerString = 8;
    public readonly int heightOffSetInPx = Camera.main.pixelHeight / 64;
    public readonly int widthOffSetInPx = Camera.main.pixelWidth / 64;

    //for JokerMarker
    public readonly int[] pentatonicTunes;

    //grid in px
    public readonly float gridHeightInPx;
    public readonly float gridWidthInPx;
    public readonly float cellHeightInPx;
    public readonly float cellWidthInPx;

    //grid in world coords
    public readonly Vector3 minWorldCoords;
    public readonly Vector3 maxWorldCoords;
    public readonly Vector3 worldDiff;
    public readonly Vector3 cellSizeWorld;

    //for jitter threshold
    public readonly Vector2 movementThreshold;

    //for OuterLinesForOrientation
    public readonly float thickenFactorTopBottomX = 2;
    public readonly float scaleFactorLefRightX = 0.0625f;
    public readonly float scaleFactorY = 0.125f;

    //scaling Factors for OutlinesForOrientation
    private static readonly int lastIndexOfOneFourthMarker = 13;
    private static readonly int lastIndexOfOneHalfMarker = 23;
    private static readonly int lastIndexOfThreeFourthMarker = 30;

    public static Settings Instance
    {
        get
        {
            if (m_settings == null)
                new Settings();

            return m_settings;
        }
    }

    public Settings()
    {
        if (m_settings != null)
        {
            Debug.LogError("Trying to create two instances of singleton.");
            return;
        }

        m_settings = this;

        Camera m_MainCamera = Camera.main;

        gridHeightInPx = m_MainCamera.pixelHeight - heightOffSetInPx * 2;
        gridWidthInPx = m_MainCamera.pixelWidth - widthOffSetInPx * 2;
        cellHeightInPx = gridHeightInPx / tunes;
        cellWidthInPx = gridWidthInPx / beats;

        minWorldCoords = m_MainCamera.ScreenToWorldPoint(new Vector2(0 + (widthOffSetInPx), 0 + heightOffSetInPx));
        maxWorldCoords = m_MainCamera.ScreenToWorldPoint(new Vector2(m_MainCamera.pixelWidth - (widthOffSetInPx), m_MainCamera.pixelHeight - heightOffSetInPx));
        worldDiff = maxWorldCoords - minWorldCoords;

        cellSizeWorld = Vector2.zero;
        cellSizeWorld.x = worldDiff.x / beats;
        cellSizeWorld.y = worldDiff.y / tunes;

        movementThreshold = Camera.main.WorldToScreenPoint(new Vector2(cellSizeWorld.x / 2, cellSizeWorld.y / 2)) - Camera.main.WorldToScreenPoint(Vector2.zero);
        locationBarOffset = cellSizeWorld * 0.01f;

        pentatonicTunes = new int[9];
        pentatonicTunes[0] = 1;
        pentatonicTunes[1] = 3;
        pentatonicTunes[2] = 6;
        pentatonicTunes[3] = 8;
        pentatonicTunes[4] = 10;
        pentatonicTunes[5] = 13;
        pentatonicTunes[6] = 15;
        pentatonicTunes[7] = 18;
        pentatonicTunes[8] = 20;

        startLoopBarMarkerID = GameObject.FindGameObjectsWithTag(loopMarkerTag)[0].GetComponent<FiducialController>().MarkerID;
        endLoopBarMarkerID = GameObject.FindGameObjectsWithTag(loopMarkerTag)[1].GetComponent<FiducialController>().MarkerID;
    }

    public float GetMarkerWidthMultiplier(int markerID)
    {
        return markerID < lastIndexOfOneFourthMarker ? 0.5f : (markerID < lastIndexOfOneHalfMarker ? 1 : (markerID < lastIndexOfThreeFourthMarker ? 1.5f : 2));
    }
}
