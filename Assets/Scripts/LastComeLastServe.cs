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
    public bool enableChords;

    private Color red;
    private Color blue;
    private Color green;
    private Color grey;

    private List<GameObject> redMarkers;
    private List<GameObject> greenMarkers;
    private List<GameObject> blueMarkers;
    private List<int> markersOnGrid; //being used for already on table lying marker recognition and adding on startup

    internal Dictionary<int, GameObject> markers;
    private GameObject[] activeMarkersOnGrid;
    private GameObject[] activeREDMarkersOnGrid;
    private GameObject[] activeGREENMarkersOnGrid;
    private GameObject[] activeBLUEMarkersOnGrid;

    private List<TuioObject> addedTuioObjects;
    private List<TuioObject> removedTuioObjects;
    private List<TuioObject> updatedTuioObjects;

    //lists which will only be accessed by the implemented TuioListener Interface methods
    private List<TuioObject> addedTuioObjectsI;
    private List<TuioObject> removedTuioObjectsI;
    private List<TuioObject> updatedTuioObjectsI;

    private Dictionary<int, int> currentTunes;

    private TuioManager m_tuiomanager;
    private Settings m_settings;
    private TokenPosition m_tokenPosition;

    void Start()
    {
        m_tokenPosition = TokenPosition.Instance;
        m_settings = Settings.Instance;

        markersOnGrid = new List<int>();

        redMarkers = new List<GameObject>();
        greenMarkers = new List<GameObject>();
        blueMarkers = new List<GameObject>();

        red = m_settings.red;
        blue = m_settings.blue;
        green = m_settings.green;
        grey = m_settings.grey;

        addedTuioObjects = new List<TuioObject>();
        removedTuioObjects = new List<TuioObject>();
        updatedTuioObjects = new List<TuioObject>();

        addedTuioObjectsI = new List<TuioObject>();
        removedTuioObjectsI = new List<TuioObject>();
        updatedTuioObjectsI = new List<TuioObject>();

        activeMarkersOnGrid = new GameObject[m_settings.beats];
        activeREDMarkersOnGrid = new GameObject[m_settings.beats];
        activeGREENMarkersOnGrid = new GameObject[m_settings.beats];
        activeBLUEMarkersOnGrid = new GameObject[m_settings.beats];

        //add this class to the callback list of the client
        m_tuiomanager = TuioManager.Instance;
        TuioClient m_tuioClient = m_tuiomanager.GetTuioClient();
        m_tuioClient.addTuioListener(this);

        //get all markers in the game
        GameObject[] markersArray = GameObject.FindGameObjectsWithTag(m_settings.markerTag);
        //fills dictionary with markers for later easier access (MarkerID as key)
        markers = new Dictionary<int, GameObject>();
        currentTunes = new Dictionary<int, int>();
        foreach (GameObject marker in markersArray)
        {
            int id = marker.GetComponent<FiducialController>().MarkerID;
            markers.Add(id, marker);
            currentTunes.Add(id, 0);
        }
    }

    public void LateUpdate()
    {
        //if new marker(s) has/ have been added, updated or removed, execute respective method and remove from list which is only for interfaces
        if (addedTuioObjects.Count != 0)
        {
            foreach (TuioObject tuioObject in addedTuioObjects)
            {
                this.AddMarker(tuioObject);
                addedTuioObjectsI.Remove(tuioObject);
            }
        }

        if (removedTuioObjects.Count != 0)
        {
            foreach (TuioObject tuioObject in removedTuioObjects)
            {
                this.RemoveMarker(tuioObject);
                removedTuioObjectsI.Remove(tuioObject);
            }
        }
        if (updatedTuioObjects.Count != 0)
        {
            foreach (TuioObject tuioObject in updatedTuioObjects)
            {
                this.UpdateMarker(tuioObject);
                updatedTuioObjectsI.Remove(tuioObject);
            }
        }

        if (!enableChords)
        {
            this.RemoveActiveMarkersIfTheySwitchedPosition(null, activeMarkersOnGrid);

            this.RunThroughMarkerListAndUpdateActiveMarkers(redMarkers, activeMarkersOnGrid, red);
            this.RunThroughMarkerListAndUpdateActiveMarkers(blueMarkers, activeMarkersOnGrid, blue);
            this.RunThroughMarkerListAndUpdateActiveMarkers(greenMarkers, activeMarkersOnGrid, green);
        }
        else
        {
            this.RemoveActiveMarkersIfTheySwitchedPosition(redMarkers, activeREDMarkersOnGrid);
            this.RemoveActiveMarkersIfTheySwitchedPosition(greenMarkers, activeGREENMarkersOnGrid);
            this.RemoveActiveMarkersIfTheySwitchedPosition(blueMarkers, activeBLUEMarkersOnGrid);

            this.RunThroughMarkerListAndUpdateActiveMarkers(redMarkers, activeREDMarkersOnGrid, red);
            this.RunThroughMarkerListAndUpdateActiveMarkers(greenMarkers, activeGREENMarkersOnGrid, green);
            this.RunThroughMarkerListAndUpdateActiveMarkers(blueMarkers, activeBLUEMarkersOnGrid, blue);
        }

        addedTuioObjects = new List<TuioObject>(addedTuioObjectsI);
        updatedTuioObjects = new List<TuioObject>(updatedTuioObjectsI);
        removedTuioObjects = new List<TuioObject>(removedTuioObjectsI);
    }

    //removes markers saved in activeMarkersOnGrid array if they are not on the position they got saved in the array
    private void RemoveActiveMarkersIfTheySwitchedPosition(List<GameObject> markerList, GameObject[] activeMarkersArray)
    {
        for (int i = 0; i < activeMarkersArray.Length; i++)
        {
            if (activeMarkersArray[i] != null)
            {
                int width = (int)(m_settings.GetMarkerWidthMultiplier(activeMarkersArray[i].GetComponent<FiducialController>().MarkerID) * 2);

                if ((i != m_tokenPosition.GetTactPosition(activeMarkersArray[i].transform.position) - Mathf.Floor(width / 2)))//1/4
                    activeMarkersArray[i] = null;

                //if chords are enabled and the color of the current marker is not the color of the current string being checked
                if (enableChords && activeMarkersArray[i] != null && !markerList.Contains(activeMarkersArray[i]))
                    activeMarkersArray[i] = null;

                if (width > 1 && activeMarkersArray[i + 1] != null && activeMarkersArray[i + 1] != activeMarkersArray[i])//1/2
                    activeMarkersArray[i + 1] = null;

                if (width > 2 && activeMarkersArray[i + 2] != null && activeMarkersArray[i + 2] != activeMarkersArray[i])//3/4
                    activeMarkersArray[i + 2] = null;

                if (width > 3 && activeMarkersArray[i + 3] != null && activeMarkersArray[i + 3] != activeMarkersArray[i])//4/4
                    activeMarkersArray[i + 3] = null;

                i += width - 1; //otherwise it would check e.g. the second beat of a 3/4 marker and the if queries (and their indices) above would be wrong
            }
        }
    }

    //Gets beat from each marker of the passed list and checks if this marker is the latest marker on said beat
    private void RunThroughMarkerListAndUpdateActiveMarkers(List<GameObject> markerList, GameObject[] activeMarkersArray, Color color)
    {
        foreach (GameObject marker in markerList)
        {
            FiducialController m_fiducial = marker.GetComponent<FiducialController>();

            if (m_fiducial.IsSnapped())
            {
                //gets beat on which the the marker lies on
                int beat = m_tokenPosition.GetTactPosition(marker.transform.position);
                //gets tune on which the marker lies on and checks if the tune has changed. If so --> log message
                int tune = m_tokenPosition.GetNote(marker.transform.position);
                if (( tune + 1) != currentTunes[m_fiducial.MarkerID])
                {
                    //only sends Log message if it's not the first tune change
                    if (currentTunes[m_fiducial.MarkerID] != 0)
                        Debug.Log("Marker " + m_fiducial.MarkerID + " changed tune from " + (currentTunes[m_fiducial.MarkerID] + 1) + " to " + (tune + 1) + ".");
                    currentTunes[m_fiducial.MarkerID] = tune + 1;
                }

                //checks if this marker isn't already the one in the activeMarker list
                if (activeMarkersArray[beat] != marker)
                {
                    int width = (int)(m_settings.GetMarkerWidthMultiplier(m_fiducial.MarkerID) * 2);

                    //if beat position is empty or if position is not empty, current marker hast snapped before marker on beat 
                    if (activeMarkersArray[beat] == null || (activeMarkersArray[beat].GetComponent<FiducialController>().GetLastTimeSnapped() < m_fiducial.GetLastTimeSnapped()))
                    {
                        //if the marker is wider/longer than one beat
                        if (width > 1) //mind 1/2
                        {
                            beat = beat == 0 ? 1 : beat;  // TODO: beat can't be at 0 for width > 1
                            //checks if second beat of the marker is not ok, if so: break
                            if (beat - 1 > m_settings.beats || (activeMarkersArray[beat - 1] != null && (activeMarkersArray[beat - 1].GetComponent<FiducialController>().GetLastTimeSnapped() > m_fiducial.GetLastTimeSnapped())))//1/2
                                break;
                            else
                            {
                                if (width > 2)// mind 3/4
                                {
                                    beat = beat == 0 ? 2 : beat;  // TODO: beat can't be at 0 for width > 1
                                    //checks if third beat of the marker is not ok, if so: break
                                    if (beat + 1 < 0 || (activeMarkersArray[beat + 1] != null && (activeMarkersArray[beat + 1].GetComponent<FiducialController>().GetLastTimeSnapped() > m_fiducial.GetLastTimeSnapped())))//3/4
                                        break;
                                    else
                                    {
                                        if (width > 3)// 4/4
                                        {
                                            beat = beat == 0 ? 3 : beat;  // TODO: beat can't be at 0 for width > 3
                                            //checks if fourth beat of the marker is not ok, if so: break
                                            if (beat - 2 > m_settings.beats || (activeMarkersArray[beat - 2] != null && (activeMarkersArray[beat - 2].GetComponent<FiducialController>().GetLastTimeSnapped() > m_fiducial.GetLastTimeSnapped())))//4/4
                                                break;
                                            this.ActivateMarkerOnBeatWithColor(marker, activeMarkersArray, beat - 2, color); //4/4
                                        }
                                        this.ActivateMarkerOnBeatWithColor(marker, activeMarkersArray, beat + 1, color); //3/4
                                    }
                                }
                                this.ActivateMarkerOnBeatWithColor(marker, activeMarkersArray, beat - 1, color); //1/2
                            }
                        }
                        this.ActivateMarkerOnBeatWithColor(marker, activeMarkersArray, beat, color); //1/4
                        Debug.Log("Marker " + m_fiducial.MarkerID + " got activated on position " + (width > 3 ? beat - 1 : (width > 1 ? beat : beat + 1)) + " on tune " + (m_tokenPosition.GetNote(marker.transform.position) + 1) + " for " + width + " beat/s.");
                    }
                }
            }
        }
    }

    private void ActivateMarkerOnBeatWithColor(GameObject marker, GameObject[] activeMarkersArray, int beat, Color color)
    {
        if (activeMarkersArray[beat] != null)
        {
            activeMarkersArray[beat].GetComponent<ColorAccToPosition>().SetCurrentColor(grey);
            this.RemoveMarkerFromActiveMarkersOnGrid(activeMarkersArray[beat], activeMarkersArray);
        }
        activeMarkersArray[beat] = marker;
        marker.GetComponent<ColorAccToPosition>().SetCurrentColor(color); //if marker was deactivated/ grey
    }

    private void RemoveMarkerFromActiveMarkersOnGrid(GameObject marker, GameObject[] activeMarkersArray)
    {
        for (int i = 0; i < activeMarkersArray.Length; i++)
        {
            if (activeMarkersArray[i] == marker)
                activeMarkersArray[i] = null;
        }
    }

    private void AddMarker(TuioObject tobj)
    {
        // TODO: This is not called if reacitvision is already running, and marker is on table
        int symbolID = tobj.getSymbolID();
        //if a marker ID is being recognized which is not being used be the program, cancel call and...
        //...prevents loopBar Markers to be added to lists
        if (markers.ContainsKey(symbolID) &&
            symbolID != m_settings.startLoopBarMarkerID && symbolID != m_settings.endLoopBarMarkerID)
        {
            GameObject currentMarker = markers[symbolID];
            currentMarker.GetComponent<FiducialController>().SetLastTimeAdded(Time.time);
            Debug.Log("Marker " + symbolID + " has been added to the grid.");

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
        int symbolID = tobj.getSymbolID();

        if (markers.ContainsKey(symbolID) && symbolID != m_settings.startLoopBarMarkerID && symbolID != m_settings.endLoopBarMarkerID)
        {

            float? lastTimeAdded = markers[symbolID].GetComponent<FiducialController>().GetLastTimeAdded();
            //checks the last time the Marker has been added to the table and doesn't remove it if it's too low (marker has been removed too fast --> false positive from fiducial library)
            if (lastTimeAdded == null || lastTimeAdded > m_settings.lastTimeAddedThreshold)
            {

                GameObject currentMarker = markers[symbolID];
                GameObject[] currentList = activeMarkersOnGrid;

                Debug.Log("Marker " + symbolID + " has been removed from the grid.");

                //remove from colored lists
                if (redMarkers.Contains(currentMarker))
                {
                    redMarkers.Remove(currentMarker);
                    if (enableChords)
                        currentList = activeREDMarkersOnGrid;
                }
                else if (blueMarkers.Contains(currentMarker))
                {
                    blueMarkers.Remove(currentMarker);
                    if (enableChords)
                        currentList = activeBLUEMarkersOnGrid;
                }
                else if (greenMarkers.Contains(currentMarker))
                {
                    greenMarkers.Remove(currentMarker);
                    if (enableChords)
                        currentList = activeGREENMarkersOnGrid;
                }

                //remove from activeMarkersOnGrid array
                RemoveMarkerFromActiveMarkersOnGrid(currentMarker, currentList);
                //set isSnapped to false
                currentMarker.GetComponent<FiducialController>().SetIsSnapped(false);
            }
        }
    }

    //keeps color allocation of markers up to date
    private void UpdateMarker(TuioObject tobj)
    {
        int symbolID = tobj.getSymbolID();

        //checks if fiducial library recognized marker which is not being used in this program and...
        //... prevents loopBar Markers to be added to lists
        if (markers.ContainsKey(symbolID) &&
            symbolID != m_settings.startLoopBarMarkerID && symbolID != m_settings.endLoopBarMarkerID)
        {
            GameObject currentMarker = markers[symbolID];

            //checks if marker if snapped, if so --> no further changements made (color change etc.)
            if (!currentMarker.GetComponent<FiducialController>().IsSnapped())
            {
                //gets current color, adds marker to the according List and removes from other lists (if containing)
                ColorAccToPosition m_colorAccToPosition = currentMarker.GetComponent<ColorAccToPosition>();
                //updates sprite color - going too fast for loop in ColoAccToPosition script
                m_colorAccToPosition.CheckColor();
                Color m_color = currentMarker.GetComponent<SpriteRenderer>().color;

                if (m_color == red && !redMarkers.Contains(currentMarker))
                {
                    redMarkers.Add(currentMarker);

                    if (blueMarkers.Contains(currentMarker))
                    {
                        blueMarkers.Remove(currentMarker);
                        Debug.Log("Marker " + symbolID + " switched colors from blue to red.");
                    }
                    else if (greenMarkers.Contains(currentMarker))
                    {
                        greenMarkers.Remove(currentMarker);
                        Debug.Log("Marker " + symbolID + " switched colors from green to red.");
                    }
                }
                else if (m_color == green && !greenMarkers.Contains(currentMarker))
                {
                    greenMarkers.Add(currentMarker);

                    if (blueMarkers.Contains(currentMarker))
                    {
                        blueMarkers.Remove(currentMarker);
                        Debug.Log("Marker " + symbolID + " switched colors blue to green.");
                    }
                    else if (redMarkers.Contains(currentMarker))
                    {
                        redMarkers.Remove(currentMarker);
                        Debug.Log("Marker " + symbolID + " switched colors red to green.");
                    }
                }
                else if (m_color == blue && !blueMarkers.Contains(currentMarker))
                {
                    blueMarkers.Add(currentMarker);

                    if (greenMarkers.Contains(currentMarker))
                    {
                        greenMarkers.Remove(currentMarker);
                        Debug.Log("Marker " + symbolID + " switched colors green to blue.");
                    }
                    else if (redMarkers.Contains(currentMarker))
                    {
                        redMarkers.Remove(currentMarker);
                        Debug.Log("Marker " + symbolID + " switched colors red to blue.");
                    }
                }
            }
        }
    }

    //callback method, adds TuioObject to the list of active tuioObjects
    public void AddTuioObject(TuioObject tobj)
    {
        addedTuioObjectsI.Add(tobj);

        if (!markersOnGrid.Contains(tobj.getSymbolID()))
            markersOnGrid.Add(tobj.getSymbolID());
    }

    //callback method, adds TuioObject to the list of updated tuioObjects
    public void UpdateTuioObject(TuioObject tobj)
    {
        if (!markersOnGrid.Contains(tobj.getSymbolID()))
        {
            markersOnGrid.Add(tobj.getSymbolID());
            addedTuioObjectsI.Add(tobj);
        }

        updatedTuioObjectsI.Add(tobj);
    }

    //callback method, removes TuioObject from the list of active tuioObjects
    public void RemoveTuioObject(TuioObject tobj)
    {
        removedTuioObjectsI.Add(tobj);

        if (markersOnGrid.Contains(tobj.getSymbolID()))
            markersOnGrid.Remove(tobj.getSymbolID());
    }

    internal bool IsBeingPlayed(int id)
    {
        return IsBeingPlayed(markers[id]);
    }

    //checks if marker is currently being played
    internal bool IsBeingPlayed(GameObject marker)
    {
        return IsMarkerInActiveMarkers(marker) && IsCurrentLocationBarOverTune(marker);
    }

    internal bool IsOtherMarkerBeingPlayedAtThisBeat(int beat)
    {
        if (!enableChords)
            return activeMarkersOnGrid[beat] != null && IsCurrentLocationBarOverTune(activeMarkersOnGrid[beat]);
        else
        {
            if (activeREDMarkersOnGrid[beat] != null && IsCurrentLocationBarOverTune(activeREDMarkersOnGrid[beat]))
                return true;
            else if (activeGREENMarkersOnGrid[beat] != null && IsCurrentLocationBarOverTune(activeGREENMarkersOnGrid[beat]))
                return true;
            else if (activeBLUEMarkersOnGrid[beat] != null && IsCurrentLocationBarOverTune(activeBLUEMarkersOnGrid[beat]))
                return true;
        }
        return false;
    }

    private bool IsCurrentLocationBarOverTune(GameObject marker)
    {
        GameObject currentLocationBar = GameObject.Find(m_settings.locationBarName);
        Vector3 spriteSize = marker.GetComponent<SpriteRenderer>().bounds.size;

        return currentLocationBar.transform.position.x >= (marker.transform.position.x - spriteSize.x / 2) && currentLocationBar.transform.position.x <= (marker.transform.position.x + spriteSize.x / 2);
    }

    private bool IsMarkerInActiveMarkers(GameObject marker)
    {
        if (enableChords)
        {
            for (int i = 0; i < activeREDMarkersOnGrid.Length; i++)
            {
                if (activeREDMarkersOnGrid[i] == marker || activeBLUEMarkersOnGrid[i] == marker || activeGREENMarkersOnGrid[i] == marker)
                    return true;
            }
            return false;
        }
        else
        {
            for (int i = 0; i < activeMarkersOnGrid.Length; i++)
            {
                if (activeMarkersOnGrid[i] == marker)
                    return true;
            }
            return false;
        }
    }


    internal GameObject[] GetActiveMarkers(Color color)
    {
        if (!enableChords)
            return activeMarkersOnGrid;
        else if (color == m_settings.red)
            return activeREDMarkersOnGrid;
        else if (color == m_settings.green)
            return activeGREENMarkersOnGrid;
        else
            return activeBLUEMarkersOnGrid;
    }

    internal List<GameObject[]> GetAllActiveMarkers()
    {
        List<GameObject[]> allActiveMarkersList = new List<GameObject[]>();

        if (!enableChords)
        {
            allActiveMarkersList.Add(activeMarkersOnGrid);
            return allActiveMarkersList;
        }
        else
        {
            allActiveMarkersList.Add(activeBLUEMarkersOnGrid);
            allActiveMarkersList.Add(activeGREENMarkersOnGrid);
            allActiveMarkersList.Add(activeREDMarkersOnGrid);

            return allActiveMarkersList;
        }

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
