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
    public LoopController startController;
    public LoopController endController;
    public Transform m_locationBar;

    public Vector3 locationBarOffset;
    public int lastSentNote = 0;

    private static SerialPort serialPort;
    private static String[] serialPortNames;
    private int serialBaudrate;
    private String receivedMsg = "";
    private int msgIndex = 0;
    private int damping = 0;
    private string messageToSend;

    private Settings m_settings;
    public LastComeLastServe m_lastComeLastServe;
    private TokenPosition m_tokenposition;
    private int tunesPerString;
    private bool enableChords;

    public GameObject[] activeMarkers;
    private GameObject[] activeRedNotes;
    private GameObject[] activeGreenNotes;
    private GameObject[] activeBlueNotes;
    private int[] lastSentIDs = { -1, -1, -1 };
    private int oldTactPos;


    // Use this for initialization
    void Start()
    {
        m_settings = Settings.Instance;
        m_locationBar = Component.FindObjectOfType<LocationBar>().transform;
        m_lastComeLastServe = Component.FindObjectOfType<LastComeLastServe>();
        m_tokenposition = TokenPosition.Instance;

        serialBaudrate = m_settings.serialBaudrate;
        serialPortNames = m_settings.serialPortNames;

        tunesPerString = m_settings.tunesPerString;
        enableChords = m_lastComeLastServe.enableChords;
        oldTactPos = -1;

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
                serialPort.WriteTimeout = 100; // ms
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

    void FixedUpdate()
    {
        locationBarOffset = m_settings.locationBarOffset;
        int tactPosWithOffset = m_tokenposition.GetTactPosition(this.m_locationBar.position - locationBarOffset);

        if (tactPosWithOffset >= m_tokenposition.GetTactPosition(GameObject.Find(m_settings.startBarLoop).transform.position) && tactPosWithOffset != oldTactPos)
        {
            lastSentNote = tactPosWithOffset;
            int nextBeat = tactPosWithOffset < (m_settings.beats - 1) ? tactPosWithOffset + 1 : 0;

            //reads active markers dependend on enabled chords or not
            if (!enableChords)
            {
                activeMarkers = m_lastComeLastServe.GetActiveMarkers(Color.white);
                GameObject[] arrayToSend = new GameObject[3];

                //calculates string on guitar which will be played and sets the tune to be played on the position of the array assigned to the string
                arrayToSend[m_tokenposition.GetNote(activeMarkers[tactPosWithOffset].transform.position) / tunesPerString] = activeMarkers[tactPosWithOffset];
                if (tactPosWithOffset > 0)
                {
                    lastSentIDs[0] = activeMarkers[tactPosWithOffset - 1] != null ? activeMarkers[tactPosWithOffset - 1].GetComponent<FiducialController>().MarkerID : -1;
                }
                else
                {
                    lastSentIDs[0] = -1;
                }
                this.SendNote(arrayToSend, new bool[] { activeMarkers[nextBeat] == null }, lastSentIDs, tactPosWithOffset);
            }
            else
            {
                activeRedNotes = m_lastComeLastServe.GetActiveMarkers(m_settings.red);
                activeGreenNotes = m_lastComeLastServe.GetActiveMarkers(m_settings.green);
                activeBlueNotes = m_lastComeLastServe.GetActiveMarkers(m_settings.blue);

                if (tactPosWithOffset > m_tokenposition.GetTactPosition(GameObject.Find(m_settings.startBarLoop).transform.position)) // TODO don'T use > 0 use > min tact
                {
                    lastSentIDs[0] = activeRedNotes[tactPosWithOffset - 1] != null ? activeRedNotes[tactPosWithOffset - 1].GetComponent<FiducialController>().MarkerID : -1;
                    lastSentIDs[1] = activeGreenNotes[tactPosWithOffset - 1] != null ? activeGreenNotes[tactPosWithOffset - 1].GetComponent<FiducialController>().MarkerID : -1;
                    lastSentIDs[2] = activeBlueNotes[tactPosWithOffset - 1] != null ? activeBlueNotes[tactPosWithOffset - 1].GetComponent<FiducialController>().MarkerID : -1;
                }
                else
                {
                    lastSentIDs[0] = -1;
                    lastSentIDs[1] = -1;
                    lastSentIDs[2] = -1;
                }
                this.SendNote(new GameObject[] { activeRedNotes[tactPosWithOffset], activeGreenNotes[tactPosWithOffset], activeBlueNotes[tactPosWithOffset] }, new bool[] { activeRedNotes[nextBeat] == null, activeGreenNotes[nextBeat] == null, activeBlueNotes[nextBeat] == null }, lastSentIDs, tactPosWithOffset);
            }
            oldTactPos = tactPosWithOffset;
        }
    }

    private void SendNote(GameObject[] notesToSend, bool[] isNextBeatEmpty, int[] lastSentID, int tactPos)
    {
        messageToSend = msgIndex + ",100";
        int startBarTactPosition = m_tokenposition.GetTactPosition(GameObject.Find(m_settings.startBarLoop).transform.position);
        int endBarTactPosition = m_tokenposition.GetTactPosition(GameObject.Find(m_settings.endtBarLoop).transform.position);

        for (int i = 0; i < notesToSend.Length; i++)
        {
            if (notesToSend[i] != null)
            {
                int id = notesToSend[i].GetComponent<FiducialController>().MarkerID;
                int duration = (int)(m_settings.GetMarkerWidthMultiplier(id) * 2);
                int tuneHeight = m_tokenposition.GetNote(notesToSend[i].transform.position) % tunesPerString;
                //if (id == lastSentID[i])  //don't send new fret
                //{
                //    tuneHeight = -1;
                //}
                //if the next beat is empty --> damping = 1, else damping = 0 
                int damping = isNextBeatEmpty[i] ? 1 : 0;

                // adjusting parameters
                tuneHeight = id == lastSentID[i] ? -1 : tuneHeight;  //don't send new fret if it's the same note
                duration -= id == lastSentID[i] ? tactPos + 1 - startBarTactPosition : 0;  // subtract duration for what's before the start loop bar
                damping = endBarTactPosition == tactPos ? 1 : damping;  //always damp if it's the last note
                messageToSend += "," + tuneHeight + "," + duration + "," + damping;
                Debug.Log("Playing Marker " + id + " on string " + (i + 1) + " with fret " + (tuneHeight + 1) + " on position " + (lastSentNote + 1) + ((damping == 1) ? " with damping." : " without damping."));
            }
            else
                messageToSend += "," + -1 + "," + 0 + "," + 0;
        }

        lastSentNote++;
        //Debug.Log("Send note " + noteToSend + " (MarkerID = " + noteToSend.fiducialController.MarkerID + ")");

        if (serialPort.IsOpen)
        {
            Debug.Log("[LOG: going to write cmd: ]" + messageToSend);
            try
            {
                serialPort.WriteLine(messageToSend);
                Debug.Log("[LOG: wrote cmd: ]" + messageToSend);
            }
            catch (IOException)
            {
                Debug.Log("[LOG: Failed to write cmd: ]" + messageToSend);
            }
            while (serialPort.BytesToRead > 0)
            {
                Debug.Log("Arduino says: " + serialPort.ReadExisting());
            }


        }

        msgIndex++;
    }
}
