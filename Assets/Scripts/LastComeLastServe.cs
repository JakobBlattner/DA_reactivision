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

    private Dictionary<int, GameObject> activeMarkers;
    private GameObject[] markers;

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

        //add this class to the callback list of the client
        m_tuiomanager = TuioManager.Instance;
        TuioClient m_tuioClient = m_tuiomanager.GetTuioClient();
        m_tuioClient.addTuioListener(this);

        //get all markers in the game
        markers = GameObject.FindGameObjectsWithTag("Marker");
    }

    public void FixedUpdate()
    {
        //if new markers has been set, act accordingly
        if (addedTuioObjects.Count != 0)
        {
            foreach (TuioObject tuioObject in addedTuioObjects)
                this.addMarker(tuioObject);
            addedTuioObjects = new List<TuioObject>();
        }
        //if markers have been removed, act accordingly
        if (removedTuioObjects.Count != 0)
        {
            foreach (TuioObject tuioObject in removedTuioObjects)
                this.removeMarker(tuioObject);
            removedTuioObjects = new List<TuioObject>();
        }
        if (updatedTuioObjects.Count != 0)
        {
            foreach (TuioObject tuioObject in updatedTuioObjects)
                this.updateMarker(tuioObject);
            updatedTuioObjects = new List<TuioObject>();
        }
    }

    private void addMarker(TuioObject tobj)
    {

        //TODO work from here on
        foreach (GameObject marker in markers)
        {
            //if marker hast the same ID as the tuioObj
            if (marker.GetComponent<FiducialController>().MarkerID == tobj.getSymbolID())
            {
                //gets current color and adds it to the according List
                //then sets marker, which has been the latest marker before the new marker on the string, to grey
                ColorAccToPosition m_colorAccToPosition = marker.GetComponent<ColorAccToPosition>();
                Color m_color = m_colorAccToPosition.GetCurrentColor();

                if (m_color.r > m_color.b && m_color.r > m_color.g)
                {
                    redMarkers.Add(marker);
                    this.SetOldMarkerToGray(redMarkers, marker);
                }
                else if (m_color.g > m_color.b && m_color.g > m_color.r)
                {
                    greenMarkers.Add(marker);
                    this.SetOldMarkerToGray(greenMarkers, marker);
                }
                else
                {
                    blueMarkers.Add(marker);
                    this.SetOldMarkerToGray(blueMarkers, marker);
                }

                m_colorAccToPosition.SetAsLastTuneOnStringAndBeat(true);
                break;
            }
        }
    }

    private void removeMarker(TuioObject tobj)
    {
        //when marker gets removed from table
    }

    private void updateMarker(TuioObject tobj)
    {
        //when changing color and/or beat
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
    public void addTuioObject(TuioObject tobj)
    {
        addedTuioObjects.Add(tobj);
    }


    public void updateTuioObject(TuioObject tobj)
    {
        //TODO implement behaviour when position is changing
        throw new NotImplementedException();
    }

    //callback method, removes TuioObject from the list of active tuioObjects
    public void removeTuioObject(TuioObject tobj)
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
