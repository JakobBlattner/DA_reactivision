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

    private Dictionary<int, GameObject> markers;
    private GameObject[] activeMarkersOnGrid;
    private GameObject[] activeREDMarkersOnGrid;
    private GameObject[] activeGREENMarkersOnGrid;
    private GameObject[] activeBLUEMarkersOnGrid;

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
    }

    //removes markers saved in activeMarkersOnGrid array if they are not on the position they got saved in the array
    private void RemoveActiveMarkersIfTheySwitchedPosition(List<GameObject> markerList, GameObject[] activeMarkersArray)
    {
        for (int i = 0; i < activeMarkersArray.Length; i++)
        {
            if (activeMarkersArray[i] != null)
            {
                //TODO change to get key from marker dictionary
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
            //gets beat on which the the marker lies on
            int beat = m_tokenPosition.GetTactPosition(marker.transform.position);
            FiducialController m_fiducial = marker.GetComponent<FiducialController>();
            //Debug.Log(m_fiducial.MarkerID + " beat: " + beat);

            //checks if this marker isn't already the one in the activeMarker list
            if (activeMarkersArray[beat] != marker && m_fiducial.IsSnapped())
            {
                int width = (int)(m_settings.GetMarkerWidthMultiplier(m_fiducial.MarkerID) * 2);

                //if beat position is empty or if position is not empty, current marker hast snapped before marker on beat 
                if (activeMarkersArray[beat] == null || (activeMarkersArray[beat].GetComponent<FiducialController>().GetLastTimeSnapped() < m_fiducial.GetLastTimeSnapped()))
                {
                    //if the marker is wider/longer than one beat
                    if (width > 1) //mind 1/2
                    {
                        //checks if second beat of the marker is not ok, if so: break
                        if (beat - 1 > m_settings.beats || (activeMarkersArray[beat - 1] != null && (activeMarkersArray[beat - 1].GetComponent<FiducialController>().GetLastTimeSnapped() > m_fiducial.GetLastTimeSnapped())))//1/2
                            break;
                        else
                        {
                            if (width > 2)// mind 3/4
                            {
                                //checks if third beat of the marker is not ok, if so: break
                                if (beat + 1 < 0 || (activeMarkersArray[beat + 1] != null && (activeMarkersArray[beat + 1].GetComponent<FiducialController>().GetLastTimeSnapped() > m_fiducial.GetLastTimeSnapped())))//3/4
                                    break;
                                else
                                {
                                    if (width > 3)// 4/4
                                    {
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
                    Debug.Log("Marker " + m_fiducial.MarkerID + " got activated on position " + (width > 3 ? beat - 1 : (width > 1 ? beat : beat + 1)) + " for " + width + " beats.");
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
        //prevents loopBar Markers to be added to lists
        if (tobj.getSymbolID() != m_settings.startLoopBarMarkerID && tobj.getSymbolID() != m_settings.endLoopBarMarkerID)
        {
            GameObject currentMarker = markers[tobj.getSymbolID()];
            Debug.Log("Marker " + tobj.getSymbolID() + " has been added to the grid.");

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
        if (tobj.getSymbolID() != m_settings.startLoopBarMarkerID && tobj.getSymbolID() != m_settings.endLoopBarMarkerID)
        {
            GameObject currentMarker = markers[tobj.getSymbolID()];
            GameObject[] currentList = activeMarkersOnGrid;

            Debug.Log("Marker " + tobj.getSymbolID() + " has been removed from the grid.");

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
        }
    }

    //keeps color allocation of markers up to date
    private void UpdateMarker(TuioObject tobj)
    {
        //prevents loopBar Markers to be added to lists
        if (tobj.getSymbolID() != m_settings.startLoopBarMarkerID && tobj.getSymbolID() != m_settings.endLoopBarMarkerID)
        {
            GameObject currentMarker = markers[tobj.getSymbolID()];

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
                    Debug.Log("Marker " + tobj.getSymbolID() + " switched colors from red to blue.");
                }
                else if (greenMarkers.Contains(currentMarker))
                {
                    greenMarkers.Remove(currentMarker);
                    Debug.Log("Marker " + tobj.getSymbolID() + " switched colors from red to green.");
                }
            }
            else if (m_color == green && !greenMarkers.Contains(currentMarker))
            {
                greenMarkers.Add(currentMarker);

                if (blueMarkers.Contains(currentMarker))
                {
                    blueMarkers.Remove(currentMarker);
                    Debug.Log("Marker " + tobj.getSymbolID() + " switched colors green to blue.");
                }
                else if (redMarkers.Contains(currentMarker))
                {
                    redMarkers.Remove(currentMarker);
                    Debug.Log("Marker " + tobj.getSymbolID() + " switched colors green to red.");
                }
            }
            else if (m_color == blue && !blueMarkers.Contains(currentMarker))
            {
                blueMarkers.Add(currentMarker);

                if (greenMarkers.Contains(currentMarker))
                {
                    greenMarkers.Remove(currentMarker);
                    Debug.Log("Marker " + tobj.getSymbolID() + " switched colors blue to green.");
                }
                else if (redMarkers.Contains(currentMarker))
                {
                    redMarkers.Remove(currentMarker);
                    Debug.Log("Marker " + tobj.getSymbolID() + " switched colors blue to red.");
                }
            }
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
        removedTuioObjects.Add(tobj);
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
