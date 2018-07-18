using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour {
    public NoteMarker[] noteMarkers = new NoteMarker[16];
    public NoteMarker[] activeMarkers = new NoteMarker[16];
    public NoteMarker[] inactiveMarkers = new NoteMarker[100];

    public LoopController startController;
    public LoopController endController;
    public LocationBar locationBar;


    public float timeSendOffset = 0f;
    public int lastSentNote = 0;

	// Use this for initialization
	void Start () {
        // Get loop controllers
        var x = Component.FindObjectsOfType<LoopController>();
        startController = x[0].startMarker ? x[0] : x[1];
        endController = x[0].startMarker ? x[1] : x[0];

        locationBar = Component.FindObjectOfType<LocationBar>();

		
        // Get note markers
        var markerObjets = GameObject.FindGameObjectsWithTag("Marker");
        noteMarkers = new NoteMarker[markerObjets.Length];
        for (int i = 0; i < noteMarkers.Length; ++i) {
            noteMarkers[i] = markerObjets[i].GetComponent<NoteMarker>();
        }
	}
	
	// Update is called once per frame
    void Update () {
        var tactPosWithOffset = this.locationBar.GetTactPosition(this.locationBar.timeMarker + timeSendOffset);

        if (lastSentNote < tactPosWithOffset || tactPosWithOffset == 0)
        {
            lastSentNote = tactPosWithOffset;
            //Debug.Log(tactPosWithOffset);
            NoteMarker noteMarker = activeMarkers[lastSentNote];
            if(noteMarker != null) // TODO: add edge case handling if duration > 1 and tactPostWithOffset == 0
            {
                // TODO: Serial Communication with Arduino
                lastSentNote += noteMarker.duration - 1;
                Debug.Log("Send note " + this.locationBar.GetNote(noteMarker.transform.position) + " (MarkerID = " + noteMarker.fiducialController.MarkerID + ")");
            }
        }

        
        //var locationBarTactPosition = GetTactPosition(this.locationBar.transform.position);
        //var noteMarker = this.activeMarkers[locationBarTactPosition];
	}


    public void NoteMarkerMoved(NoteMarker marker, Vector2 delta) {
        // Remove from active markers
        for (int i = 0; i < this.activeMarkers.Length; ++i) {
            if (this.activeMarkers[i] == marker) {
                this.activeMarkers[i] = null;
                break;
            }
        }

        Debug.Log("Marker " + marker.fiducialController.MarkerID + " moved");
    }

    public void NoteMarkerRemoved(NoteMarker marker) {// Remove from active markers
        for (int i = 0; i < this.activeMarkers.Length; ++i)
        {
            if (this.activeMarkers[i] == marker)
            {
                this.activeMarkers[i] = null;
                break;
            }
        }

        Debug.Log("Marker " + marker.fiducialController.MarkerID + " removed");
    }

    public void NoteMarkerPositined(NoteMarker marker) {
        // Add to active markers
        var tactPos = this.locationBar.GetTactPosition(marker.lastPosition);

        if (tactPos < 0 || tactPos > 15) {
            return;
        }

        if (this.activeMarkers[tactPos] != null) {
            return;
        }

        this.activeMarkers[tactPos] = marker;
        Debug.Log("Marker " + marker.fiducialController.MarkerID + " positined (TactPos " + tactPos + ")");
    }
}
