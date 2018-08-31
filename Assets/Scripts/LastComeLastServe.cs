using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TUIO;
using UniducialLibrary;
using System;

public class LastComeLastServe : MonoBehaviour, TuioListener
//implement TUIOListener
//set boolean after each event from TUIOListener
//check in loop if boolean event has been triggered/ set and act accordingly
{

    public Color grayedOutMarkerColor;

    private List<GameObject> redMarkers;
    private List<GameObject> greenMarkers;
    private List<GameObject> blueMarkers;

    private Dictionary<int, GameObject> markers;
    private GameObject[] activeMarkersOnGrid;

    private List<TuioObject> addedTuioObjects;
    private List<TuioObject> removedTuioObjects;
    private List<TuioObject> updatedTuioObjects;

    private TuioManager m_tuiomanager;

    // Use this for initialization
    void Start()
    {
        redMarkers = new List<GameObject>();
        greenMarkers = new List<GameObject>();
        blueMarkers = new List<GameObject>();

        addedTuioObjects = new List<TuioObject>();
        removedTuioObjects = new List<TuioObject>();
        updatedTuioObjects = new List<TuioObject>();

        activeMarkersOnGrid = new GameObject[16];

        //add this class to the callback list of the client
        m_tuiomanager = TuioManager.Instance;
        TuioClient m_tuioClient = m_tuiomanager.GetTuioClient();
        m_tuioClient.addTuioListener(this);

        //get all markers in the game
        GameObject[] markersArray = GameObject.FindGameObjectsWithTag("Marker");
        //fills dictionary with markers for later easier access (MarkerID as key)
        markers = new Dictionary<int, GameObject>();
        foreach (GameObject marker in markersArray)
        {
            markers.Add(marker.GetComponent<FiducialController>().MarkerID, marker);
        }
    }

    public void FixedUpdate()
    {
        //if new markers has been set, act accordingly
        if (addedTuioObjects.Count != 0)
        {
            foreach (TuioObject tuioObject in addedTuioObjects)
                this.AddMarker(tuioObject);
            addedTuioObjects = new List<TuioObject>();
        }
        //if markers have been removed, act accordingly
        if (removedTuioObjects.Count != 0)
        {
            foreach (TuioObject tuioObject in removedTuioObjects)
                this.RemoveMarker(tuioObject);
            removedTuioObjects = new List<TuioObject>();
        }
        if (updatedTuioObjects.Count != 0)
        {
            foreach (TuioObject tuioObject in updatedTuioObjects)
                this.UpdateMarker(tuioObject);
            updatedTuioObjects = new List<TuioObject>();
        }
    }

    private void AddMarker(TuioObject tobj)
    {
        GameObject currentMaker = markers[tobj.getSymbolID()];

        //gets current color and adds marker to the according List
        //then sets marker, which has been the latest marker before the new marker on the string, to grey
        ColorAccToPosition m_colorAccToPosition = currentMaker.GetComponent<ColorAccToPosition>();
        Color m_color = m_colorAccToPosition.GetCurrentColor();
        char mainColor = this.CheckMainColor(m_color);

        if (mainColor.Equals('r'))
        {
            redMarkers.Add(currentMaker);
            this.SetOldMarkerToGray(redMarkers, currentMaker);
        }
        else if (mainColor.Equals('g'))
        {
            greenMarkers.Add(currentMaker);
            this.SetOldMarkerToGray(greenMarkers, currentMaker);
        }
        else
        {
            blueMarkers.Add(currentMaker);
            this.SetOldMarkerToGray(blueMarkers, currentMaker);
        }

        m_colorAccToPosition.SetAsLastTuneOnStringAndBeat(true);
    }

    //when marker gets removed from table
    private void RemoveMarker(TuioObject tobj)
    {
        //check list according to color and check each lastTimeMoved from NoteMarker
        //check TuneManager for possible scenarios
    }

    //when changing color and/or beat
    private void UpdateMarker(TuioObject tobj)
    {

    }

    //returns a char synonym for the main color component of the color
    private char CheckMainColor(Color color)
    {
        if (color.r > color.b && color.r > color.g)
            return 'r';
        else if (color.g > color.b && color.g > color.r)
            return 'g';
        else
            return 'b';
    }

    private void SetOldMarkerToGray(List<GameObject> markers, GameObject marker)
    {

        //TODO: redo as foreach loop
        if (markers.Count > 1)
        {
            ColorAccToPosition oldColorAccToPosition = markers[markers.Count - 2].GetComponent<ColorAccToPosition>();
            oldColorAccToPosition.SetAsLastTuneOnStringAndBeat(false);
            oldColorAccToPosition.SetCurrentColor(grayedOutMarkerColor);
        }
    }

    //callback method, adds TuioObject to the list of active tuioObjects
    public void AddTuioObject(TuioObject tobj)
    {
        addedTuioObjects.Add(tobj);
    }

    //callback method, adds TuioObject to the list of updated tuioObjects
    public void UpdateTuioObject(TuioObject tobj)
    {
        updatedTuioObjects.Add(tobj);
    }

    //callback method, removes TuioObject from the list of active tuioObjects
    public void RemoveTuioObject(TuioObject tobj)
    {
        addedTuioObjects.Remove(tobj);
    }


    #region not needed TuioListener methods
    public void refresh(TuioTime ftime)
    {
    }

    public void addTuioCursor(TuioCursor tcur)
    {
    }

    public void updateTuioCursor(TuioCursor tcur)
    {
        throw new NotImplementedException();
    }

    public void removeTuioCursor(TuioCursor tcur)
    {
        throw new NotImplementedException();
    }
    #endregion
}
