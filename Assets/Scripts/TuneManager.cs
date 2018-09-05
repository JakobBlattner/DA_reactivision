using System;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TUIO;
using UniducialLibrary;
using System.IO;

public class TuneManager : MonoBehaviour
{
    public LoopController startController;
    public LoopController endController;
    public LocationBar locationBar;

    public Vector3 locationBarOffset = new Vector3(0.1f, 0.1f, 0); //TODO Set as time threshold 
    public int lastSentNote = 0;

    private static SerialPort serialPort;
    private static String[] serialPortNames = { "COM5", "/dev/cu.usbmodem1411", "/dev/cu.usbmodem1421" };
    private static int serialBaudrate = 9600;

    private String receivedMsg = "";
    private int msgIndex = 0;
    private int damping = 0;

    private Settings m_settings;
    public LastComeLastServe lastComeLastServe;

    private int tunesPerString;
    private bool enableChords;

    public GameObject[] activeMarkers;
    private GameObject noteToSend;
    private GameObject redNoteToSend;
    private GameObject greenNoteToSend;
    private GameObject blueNoteToSend;

    // Use this for initialization
    void Start()
    {
        m_settings = Settings.Instance;
        locationBar = Component.FindObjectOfType<LocationBar>();
        lastComeLastServe = Component.FindObjectOfType<LastComeLastServe>();

        tunesPerString = m_settings.tunesPerString;
        enableChords = lastComeLastServe.enableChords;

        // Get loop controllers
        var x = Component.FindObjectsOfType<LoopController>();
        startController = x[0].startMarker ? x[0] : x[1];
        endController = x[0].startMarker ? x[1] : x[0];

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

            if (enableChords)
            {
                noteToSend = lastComeLastServe.GetActiveMarkers(Color.white)[tactPosWithOffset];
                this.SendNote(noteToSend, null, null);
            }
            else
            {
                redNoteToSend = lastComeLastServe.GetActiveMarkers(m_settings.red)[tactPosWithOffset];
                greenNoteToSend = lastComeLastServe.GetActiveMarkers(m_settings.green)[tactPosWithOffset];
                blueNoteToSend = lastComeLastServe.GetActiveMarkers(m_settings.blue)[tactPosWithOffset];

                this.SendNote(redNoteToSend, blueNoteToSend, greenNoteToSend);
            }            
        }
        //var locationBarTactPosition = GetTactPosition(this.locationBar.transform.position);
        //var noteMarker = this.activeMarkers[locationBarTactPosition];
    }

    //TODO check mit Raphael
    private void SendNote(GameObject firstNote, GameObject secondNote, GameObject thirdNote)
    {/*
        if (noteToSend != null) // TODO: add edge case handling if duration > 1 and tactPostWithOffset == 0
        {
            // TODO: Serial Communication with Arduino
            lastSentNote += noteToSend.duration - 1;
            int noteToSend = this.locationBar.GetNote(noteToSend.transform.position);
            Debug.Log("Send note " + noteToSend + " (MarkerID = " + noteToSend.fiducialController.MarkerID + ")");
            int s = 0;
            int f = 0;
            if (noteToSend < tunesPerString)
            { // TODO: create a function for this
                s = 2;
                f = noteToSend;
            }
            else if (tunesPerString <= noteToSend && noteToSend < tunesPerString * 2)
            {
                s = 1;
                f = noteToSend - tunesPerString;
            }
            else if (tunesPerString * 2 <= noteToSend && noteToSend < tunesPerString*3 )//*TODO: check; hier stand anstatt "tunesPerString*3" 30
            {
                s = 0;
                f = noteToSend - tunesPerString * 2;
            }
            if (serialPort.IsOpen)
            {
                serialPort.WriteLine(msgIndex + "," + s + "," + f + "," + noteToSend.duration + "," + damping);
                Debug.Log("[LOG: wrote cmd: ]" + msgIndex + "," + s + "," + f + "," + noteToSend.duration + "," + damping);
            }
            Debug.Log("Marker " + noteToSend.fiducialController.MarkerID + " with Note " + noteToSend + " has been played for " + noteToSend.duration);
            /*do // wait until received msg starts with last sent id
            {
                receivedMsg = serialPort.ReadExisting();
                Debug.Log("[LOG: received: " + receivedMsg + "]");
            } while (!receivedMsg.StartsWith("" + msgIndex));
            Debug.Log(receivedMsg);*/
            //msgIndex++;
        //}
    }
}
