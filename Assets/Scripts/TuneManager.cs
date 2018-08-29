using System;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TUIO;
using UniducialLibrary;
using System.IO;

public class TuneManager : MonoBehaviour {
    public NoteMarker[] noteMarkers = new NoteMarker[16];
    public NoteMarker[] activeMarkers = new NoteMarker[16];
    public NoteMarker[] inactiveMarkers = new NoteMarker[100];

    public LoopController startController;
    public LoopController endController;
    public LocationBar locationBar;


    public float timeSendOffset = 0f;
    public int lastSentNote = 0;

    private static SerialPort serialPort;
    private static String[] serialPortNames = {"/dev/cu.usbmodem1411", "/dev/cu.usbmodem1421"};
    private String receivedMsg = "";
    private int msgIndex = 0;
    private int damping = 0;
    private static int serialBaudrate = 9600;


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

        Boolean serialConnected = false;
        for (int i = 0; i < serialPortNames.Length && !serialConnected; i++){
            try
            {
                Debug.Log("Trying to open serial connection on port: " + serialPortNames[i]);
                serialPort = new SerialPort(serialPortNames[i], serialBaudrate);
                serialPort.Open();
                Debug.Log("Opened serial connection on port: " + serialPortNames[i]);
                serialConnected = true;
            }
            catch (IOException)
            {
                Debug.Log("Failed to open serial connection on port: " + serialPortNames[i]);
            }
        }
        if(!serialConnected) {
            Debug.LogWarning("No serial connection established, no music will be played");
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
                int noteToSend = this.locationBar.GetNote(noteMarker.transform.position);
                Debug.Log("Send note " + noteToSend + " (MarkerID = " + noteMarker.fiducialController.MarkerID + ")");
                int s = 0;
                int f = 0;
                if(noteToSend < 8) { // TODO: create a function for this
                    s = 2;
                    f = noteToSend;
                }
                else if (8 <= noteToSend && noteToSend < 16)
                {
                    s = 1;
                    f = noteToSend - 8;
                } else if (16 <= noteToSend && noteToSend < 30)
                {
                    s = 0;
                    f = noteToSend - 16;
                }
                if (serialPort.IsOpen)
                {
                    serialPort.WriteLine(msgIndex + "," + s + "," + f + "," + noteMarker.duration + "," + damping);
                    Debug.Log("[LOG: wrote cmd: ]" + msgIndex + "," + s + "," + f + "," + noteMarker.duration + "," + damping);
                }
                Debug.Log("Marker " + noteMarker.fiducialController.MarkerID + " with Note " + noteToSend + " has been played for " + noteMarker.duration);
                /*do // wait until received msg starts with last sent id
                {
                    receivedMsg = serialPort.ReadExisting();
                    Debug.Log("[LOG: received: " + receivedMsg + "]");
                } while (!receivedMsg.StartsWith("" + msgIndex));
                Debug.Log(receivedMsg);*/
                msgIndex++;
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

        Debug.Log("Marker " + marker.fiducialController.MarkerID + " moved by " + delta);
    }

    public void NoteMarkerRemoved(NoteMarker marker) {// Remove from active markers
        var tactPos = this.locationBar.GetTactPosition(marker.lastPosition);

        for (int i = 0; i < this.activeMarkers.Length; ++i)
        {
            if (this.activeMarkers[i] == marker)
            {
                this.activeMarkers[i] = null;
                break;
            }
        }

        Debug.Log("Marker " + marker.fiducialController.MarkerID + " removed from position " + tactPos);
    }

    public void NoteMarkerPositioned(NoteMarker marker) {
        // Add to active markers
        var tactPos = this.locationBar.GetTactPosition(marker.lastPosition);

        if (tactPos < 0 || tactPos > 15) {
            return;
        }

        if (this.activeMarkers[tactPos] != null) {
            return;
        }

        this.activeMarkers[tactPos] = marker;
        Debug.Log("Marker " + marker.fiducialController.MarkerID + " positioned at " + tactPos + ")");
    }
}