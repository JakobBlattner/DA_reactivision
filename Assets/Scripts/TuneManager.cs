using System;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TUIO;
using UniducialLibrary;
using System.IO;
using System.Linq;

public class TuneManager : MonoBehaviour
{
    public NoteMarker[] noteMarkers;
    public NoteMarker[] activeMarkers;
    public NoteMarker[] inactiveMarkers;

    public LoopController startController;
    public LoopController endController;
    public LocationBar locationBar;


    public Vector3 locationBarOffset = new Vector3(0.1f, 0.1f, 0);
    public int lastSentNote = 0;

    private static SerialPort serialPort;
    private static String[] serialPortNames = {"COM1", "COM2", "COM3", "COM4", "COM5", "/dev/cu.usbmodem1411", "/dev/cu.usbmodem1421" };
    private String receivedMsg = "";
    private int msgIndex = 0;
    private int damping = 0;
    private static int serialBaudrate = 9600;
    private Settings m_settings;

    /*TODO:
     Rewrite so that TuneManager is only responsible for communication with the arduino:
     - TuneManager get's actice marker from LastComeLastServe.cs
     */

    // Use this for initialization
    void Start()
    {
        m_settings = Settings.Instance;

        noteMarkers = new NoteMarker[m_settings.beats];
        activeMarkers = new NoteMarker[m_settings.beats];
        inactiveMarkers = new NoteMarker[100];

        // Get loop controllers
        var x = Component.FindObjectsOfType<LoopController>();
        startController = x[0].startMarker ? x[0] : x[1];
        endController = x[0].startMarker ? x[1] : x[0];

        locationBar = Component.FindObjectOfType<LocationBar>();


        // Get note markers
        var markerObjets = GameObject.FindGameObjectsWithTag(m_settings.markerTag);
        noteMarkers = new NoteMarker[markerObjets.Length];
        for (int i = 0; i < noteMarkers.Length; ++i)
        {
            noteMarkers[i] = markerObjets[i].GetComponent<NoteMarker>();
        }

        Boolean serialConnected = false;
        for (int i = 0; i < serialPortNames.Length && !serialConnected; i++)
        {
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
        if (!serialConnected)
        {
            Debug.LogWarning("No serial connection established, no music will be played");
        }
    }

    // Update is called once per frame
    void Update()
    {

        var tactPosWithOffset = this.locationBar.GetTactPosition(this.locationBar.transform.position - locationBarOffset);

        if (lastSentNote < tactPosWithOffset || tactPosWithOffset == 0)
        {
            lastSentNote = tactPosWithOffset;
            //Debug.Log(tactPosWithOffset);
            NoteMarker noteMarker = activeMarkers[lastSentNote];
            if (noteMarker != null) // TODO: add edge case handling if duration > 1 and tactPostWithOffset == 0
            {
                // TODO: Serial Communication with Arduino
                lastSentNote += noteMarker.duration - 1;
                int noteToSend = this.locationBar.GetNote(noteMarker.transform.position);
                Debug.Log("Send note " + noteToSend + " (MarkerID = " + noteMarker.fiducialController.MarkerID + ")");
                int s = 0;
                int f = 0;
                if (noteToSend < 8)
                { // TODO: create a function for this
                    s = 2;
                    f = noteToSend;
                }
                else if (8 <= noteToSend && noteToSend < 16)
                {
                    s = 1;
                    f = noteToSend - 8;
                }
                else if (16 <= noteToSend && noteToSend < 30)
                {
                    s = 0;
                    f = noteToSend - 16;
                }
                if (serialPort.IsOpen)
                {
                    // FOR TESTING ONLY - TODO: get this from lastcomelastserve in future
                    int noteMarkerFret0 = 1;
                    int noteMarkerFret1 = 2;
                    int noteMarkerFret2 = 3;

                    int noteMarkerDuration0 = 1;
                    int noteMarkerDuration1 = 1;
                    int noteMarkerDuration2 = 2;

                    int noteMarkerDamping0 = 0;
                    int noteMarkerDamping1 = 1;
                    int noteMarkerDamping2 = 1;
                    int[] cmdArray = {msgIndex, this.locationBar.bpm, noteMarkerFret0, noteMarkerDuration0, noteMarkerDamping0, noteMarkerFret1, noteMarkerDuration1, noteMarkerDamping1, noteMarkerFret2, noteMarkerDuration2, noteMarkerDamping2};
                    // FOR TESTING ONLY
                    /*
                     * Message format is message index, bpm, 1st NOTE, 2nd NOTE, 3rd NOTE
                     * NOTE format is fret(<0 if pause), duration(1|2|3|4), damping(0|1)
                     */
                    serialPort.WriteLine(string.Join(",", Array.ConvertAll(cmdArray, x => x.ToString())));
                    Debug.Log("[LOG: wrote cmd: ]" + string.Join(",", Array.ConvertAll(cmdArray, x => x.ToString())));
                }
                Debug.Log("Marker " + noteMarker.fiducialController.MarkerID + " with Note " + noteToSend + " has been played for " + noteMarker.duration); //most likely remove this, because timing issues
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

    #region NoteMarker Interaction Messages

    public void NoteMarkerMoved(NoteMarker marker, Vector2 delta)
    {
        // Remove from active markers
        for (int i = 0; i < this.activeMarkers.Length; ++i)
        {
            if (this.activeMarkers[i] == marker)
            {
                this.activeMarkers[i] = null;
                break;
            }
        }

        //Debug.Log("Marker " + marker.fiducialController.MarkerID + " moved by " + delta);
    }

    public void NoteMarkerRemoved(NoteMarker marker)
    {// Remove from active markers
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

    public void NoteMarkerPositioned(NoteMarker marker)
    {
        // Add to active markers
        var tactPos = this.locationBar.GetTactPosition(marker.lastPosition);

        if (tactPos < 1 || tactPos > 16)
        {
            return;
        }

        if (this.activeMarkers[tactPos] != null)
        {
            return;
        }

        this.activeMarkers[tactPos] = marker;
        Debug.Log("Marker " + marker.fiducialController.MarkerID + " positioned at " + tactPos + ")");
    }
    #endregion
}
