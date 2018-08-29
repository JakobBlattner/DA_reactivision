using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TUIO;
using UniducialLibrary;
using System;

public class LastComeLastServe : MonoBehaviour
    //implement TUIOListener
    //set boolean after each event from TUIOListener
    //check in loop if boolean event has been triggered/ set and act accordingly
{

    public Color grayedOutMarkerColor;

    private List<GameObject> redMarkers;
    private List<GameObject> greenMarkers;
    private List<GameObject> blueMarkers;

    private TuioManager m_tuiomanager;

    private GameObject[] markers;

    // Use this for initialization
    void Start()
    {
        redMarkers = new List<GameObject>();
        greenMarkers = new List<GameObject>();
        blueMarkers = new List<GameObject>();

        m_tuiomanager = TuioManager.Instance;
    }

    public void FixedUpdate()
    {
        //+2 for the markers which set the LoopBars
        if(m_tuiomanager.GetObjectCount() != markers.Length+2)
        {
            GameObject [] allMarkers = GameObject.FindGameObjectsWithTag("Marker");
            //current position from TokenPosition.Instance.GetTactPosition(Vector2);
        }
    }

    public void addedMarker(TuioObject tobj)
    {
        //get GameObject with TUIO ID of the obj
       //GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");

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

                //next line not needed, is by default true
                //m_colorAccToPosition.SetAsLastTune(true);
            }
        }
    }

    public void removedMarker(TuioObject tobj)
    {
        //when marker gets removed from table
    }

    public void updateMarker(TuioObject tobj)
    {
        //when changing color and/or beat
    }

    private void SetOldMarkerToGray(List<GameObject> markers, GameObject marker)
    {
        if (markers.Count > 1)
        {
            ColorAccToPosition oldColorAccToPosition = markers[markers.Count - 2].GetComponent<ColorAccToPosition>();
            oldColorAccToPosition.SetAsLastTuneOnStringAndBeat(false);
            oldColorAccToPosition.SetCurrentColor(grayedOutMarkerColor);
        }
    }

}
