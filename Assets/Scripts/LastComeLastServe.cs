using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TUIO;
using UniducialLibrary;
using System;

/**
 * This class is responsible for the active and inactive marker algorithm.
 * It implements the TuioListener Interface, so that every marker movement etc. is instantly recognized.
 * **/
public class LastComeLastServe : MonoBehaviour, TuioListener
{
    private Color red;
    private Color blue;
    private Color green;
    private Color grey;

    private float movementThreshold;

    private List<GameObject> redMarkers;
    private List<GameObject> greenMarkers;
    private List<GameObject> blueMarkers;
    private List<GameObject> allColoredMarkers;

    private Dictionary<int, GameObject> markers;
    private GameObject[] activeMarkersOnGrid;

    private List<TuioObject> addedTuioObjects;
    private List<TuioObject> removedTuioObjects;
    private List<TuioObject> updatedTuioObjects;

    private TuioManager m_tuiomanager;
    private Settings m_settings;
    private TokenPosition m_tokenPosition;

    // Use this for initialization
    void Start()
    {
        m_tokenPosition = TokenPosition.Instance;
        m_settings = Settings.Instance;

        movementThreshold = m_settings.movementThreshold;

        redMarkers = new List<GameObject>();
        greenMarkers = new List<GameObject>();
        blueMarkers = new List<GameObject>();
        allColoredMarkers = new List<GameObject>();

        red = m_settings.red;
        blue = m_settings.blue;
        green = m_settings.green;
        grey = m_settings.grey;

        addedTuioObjects = new List<TuioObject>();
        removedTuioObjects = new List<TuioObject>();
        updatedTuioObjects = new List<TuioObject>();

        activeMarkersOnGrid = new GameObject[m_settings.beats];

        //add this class to the callback list of the client
        m_tuiomanager = TuioManager.Instance;
        TuioClient m_tuioClient = m_tuiomanager.GetTuioClient();
        m_tuioClient.addTuioListener(this);

        //get all markers in the game
        GameObject[] markersArray = GameObject.FindGameObjectsWithTag(m_settings.markerTag);
        //fills dictionary with markers for later easier access (MarkerID as key)
        markers = new Dictionary<int, GameObject>();
        foreach (GameObject marker in markersArray)
        {
            markers.Add(marker.GetComponent<FiducialController>().MarkerID, marker);
        }
    }

    public void LateUpdate()
    {
        //if new marker(s) has/ have been set, act accordingly
        if (addedTuioObjects.Count != 0)
        {
            foreach (TuioObject tuioObject in addedTuioObjects)
                this.AddMarker(tuioObject);
            addedTuioObjects = new List<TuioObject>();
        }

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

        this.RemoveActiveMarkersIfTheySwitchedPosition();
        this.RunThroughMarkerListAndUpdateActiveMarkers(redMarkers, red);
        this.RunThroughMarkerListAndUpdateActiveMarkers(blueMarkers, blue);
        this.RunThroughMarkerListAndUpdateActiveMarkers(greenMarkers, green);
    }

    //removes markers saved in activeMarkersOnGrid array if they are not on the position they got saved in the array
    private void RemoveActiveMarkersIfTheySwitchedPosition()
    {
        for (int i = 0; i < activeMarkersOnGrid.Length; i++)
        {
            if (activeMarkersOnGrid[i] != null)
            {
                //TODO change to get key from marker dictionary
                int width = (int)(Settings.GetMarkerWidhMultiplier(activeMarkersOnGrid[i].GetComponent<FiducialController>().MarkerID) * 2);

                if (i != m_tokenPosition.GetTactPosition(activeMarkersOnGrid[i].transform.position) - 1)//1/4
                    activeMarkersOnGrid[i] = null;

                if (width > 1 && activeMarkersOnGrid[i + 1] != null && activeMarkersOnGrid[i + 1] != activeMarkersOnGrid[i])//1/2
                    activeMarkersOnGrid[i + 1] = null;

                if (width > 2 && activeMarkersOnGrid[i + 2] != null && activeMarkersOnGrid[i + 2] != activeMarkersOnGrid[i])//3/4
                    activeMarkersOnGrid[i + 2] = null;

                if (width > 3 && activeMarkersOnGrid[i + 3] != null && activeMarkersOnGrid[i + 3] != activeMarkersOnGrid[i])//4/4
                    activeMarkersOnGrid[i + 3] = null;

                i += width - 1; //otherwise it would check e.g. the second beat of a 3/4 marker and the if queries above would be wrong
            }
        }
    }

    //Gets beat from each marker of the passed list and checks if this marker is the latest marker on said beat
    private void RunThroughMarkerListAndUpdateActiveMarkers(List<GameObject> markerList, Color color)
    {
        foreach (GameObject marker in markerList)
        {
            //gets beat on which the the marker lies on
            int beat = m_tokenPosition.GetTactPosition(marker.transform.position) - 1;

            //checks if this marker isn't already the one in the activeMarker list
            if (activeMarkersOnGrid[beat] != marker && marker.GetComponent<FiducialController>().IsSnapped())
            {
                int width = (int)(Settings.GetMarkerWidhMultiplier(marker.GetComponent<FiducialController>().MarkerID) * 2);

                //if beat position is empty or if position is not empty, current marker hast snapped before marker on beat 
                if (activeMarkersOnGrid[beat] == null || (activeMarkersOnGrid[beat].GetComponent<NoteMarker>().GetLastTimeSnapped() < marker.GetComponent<NoteMarker>().GetLastTimeSnapped()))
                {
                    //if the marker is wider/longer than one beat
                    if (width > 1) //mind 1/2
                    {
                        //checks if second beat of the marker is not ok, if so: break
                        if (beat + 1 > m_settings.beats || (activeMarkersOnGrid[beat + 1] != null && (activeMarkersOnGrid[beat + 1].GetComponent<NoteMarker>().GetLastTimeSnapped() > marker.GetComponent<NoteMarker>().GetLastTimeSnapped())))//1/2
                            break;
                        else
                        {
                            if (width > 2)// mind 3/4
                            {
                                //checks if third beat of the marker is not ok, if so: break
                                if (beat - 1 < 0 || (activeMarkersOnGrid[beat - 1] != null && (activeMarkersOnGrid[beat - 1].GetComponent<NoteMarker>().GetLastTimeSnapped() > marker.GetComponent<NoteMarker>().GetLastTimeSnapped())))//3/4
                                    break;
                                else
                                {
                                    if (width > 3)// 4/4
                                    {
                                        //checks if fourth beat of the marker is not ok, if so: break
                                        if (beat + 2 > m_settings.beats || (activeMarkersOnGrid[beat + 2] != null && (activeMarkersOnGrid[beat + 2].GetComponent<NoteMarker>().GetLastTimeSnapped() > marker.GetComponent<NoteMarker>().GetLastTimeSnapped())))//4/4
                                            break;
                                        this.ActivateMarkerOnBeatWithColor(marker, beat + 2, color); //4/4
                                    }
                                    this.ActivateMarkerOnBeatWithColor(marker, beat - 1, color); //3/4
                                }
                            }
                            this.ActivateMarkerOnBeatWithColor(marker, beat + 1, color); //1/2
                        }
                    }
                    this.ActivateMarkerOnBeatWithColor(marker, beat, color); //1/4
                }
            }
        }
    }

    private void ActivateMarkerOnBeatWithColor(GameObject marker, int beat, Color color)
    {
        if (activeMarkersOnGrid[beat] != null)
        {
            activeMarkersOnGrid[beat].GetComponent<ColorAccToPosition>().SetCurrentColor(grey);
            this.RemoveMarkerFromActiveMarkersOnGrid(activeMarkersOnGrid[beat]);
        }
        activeMarkersOnGrid[beat] = marker;
        marker.GetComponent<ColorAccToPosition>().SetCurrentColor(color); //if marker was deactivated/ grey
    }

    private void RemoveMarkerFromActiveMarkersOnGrid(GameObject marker)
    {
        for (int i = 0; i < activeMarkersOnGrid.Length; i++)
        {
            if (activeMarkersOnGrid[i] == marker)
                activeMarkersOnGrid[i] = null;
        }
    }

    private void AddMarker(TuioObject tobj)
    {
        //prevents loopBar Markers to be added to lists
        if (tobj.getSymbolID() != 34 && tobj.getSymbolID() != 35)
        {
            GameObject currentMarker = markers[tobj.getSymbolID()];

            //gets current color and adds marker to the according List
            ColorAccToPosition m_colorAccToPosition = currentMarker.GetComponent<ColorAccToPosition>();
            //updates sprite color - going too fast for loop in ColoAccToPosition script
            m_colorAccToPosition.CheckColor();
            Color m_color = currentMarker.GetComponent<SpriteRenderer>().color;

            if (m_color == red)
                redMarkers.Add(currentMarker);
            else if (m_color == green)
                greenMarkers.Add(currentMarker);
            else if (m_color == blue)
                blueMarkers.Add(currentMarker);
        }
    }

    //when marker gets removed from table, remove from every coloured list and activeMarkersOnGrid array
    private void RemoveMarker(TuioObject tobj)
    {
        GameObject currentMarker = markers[tobj.getSymbolID()];

        //remove from colored lists
        if (redMarkers.Contains(currentMarker))
        {
            redMarkers.Remove(currentMarker);
        }
        else if (blueMarkers.Contains(currentMarker))
        {
            blueMarkers.Remove(currentMarker);
        }
        else if (greenMarkers.Contains(currentMarker))
        {
            greenMarkers.Remove(currentMarker);
        }

        //remove from activeMarkersOnGrid array
        RemoveMarkerFromActiveMarkersOnGrid(currentMarker);
    }

    //when changing color and/or beat
    private void UpdateMarker(TuioObject tobj)
    {
        //use threshold for movement
        //TODO is this method needed?
        //for logging only? - if so, add threshold
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
        removedTuioObjects.Add(tobj);
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
