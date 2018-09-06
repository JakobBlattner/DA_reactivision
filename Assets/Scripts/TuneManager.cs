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
    public LocationBar m_locationBar;

    public Vector3 locationBarOffset = new Vector3(0.1f, 0.1f, 0); //TODO Set as time threshold 
    public int lastSentNote = 0;

    private static SerialPort serialPort;
    private static String[] serialPortNames = {"COM1", "COM2", "COM3", "COM4", "COM5", "/dev/cu.usbmodem1411", "/dev/cu.usbmodem1421" };
    private int serialBaudrate = 9600;
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
    private int oldTactPos;

    // Use this for initialization
    void Start()
    {
        m_settings = Settings.Instance;
        m_locationBar = Component.FindObjectOfType<LocationBar>();
        m_lastComeLastServe = Component.FindObjectOfType<LastComeLastServe>();
        m_tokenposition = TokenPosition.Instance;

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

    void Update()
    {
        //--> beat + 1 darf nicht mehr verändert werden-- > threshold
        // TODO: add edge case handling if duration > 1 and tactPostWithOffset == 0
        int tactPosWithOffset = this.m_locationBar.GetTactPosition(this.m_locationBar.transform.position - locationBarOffset);

        if (tactPosWithOffset != oldTactPos)
        {
            lastSentNote = tactPosWithOffset;

            int nextBeat = tactPosWithOffset == 16 ? 1 : tactPosWithOffset + 1;
            //reads active markers dependend on enabled chords or not
            if (!enableChords)
            {
                activeMarkers = m_lastComeLastServe.GetActiveMarkers(Color.white);
                GameObject[] arrayToSend = new GameObject[3];

                //calculates string on guitar which will be played and sets the tune to be played on the position of the array assigned to the string
                arrayToSend[m_tokenposition.GetNote(activeMarkers[tactPosWithOffset].transform.position) / tunesPerString] = activeMarkers[tactPosWithOffset];

                this.SendNote(arrayToSend, new bool[] { activeMarkers[nextBeat] == null });
            }
            else
            {
                activeRedNotes = m_lastComeLastServe.GetActiveMarkers(m_settings.red);
                activeGreenNotes = m_lastComeLastServe.GetActiveMarkers(m_settings.green);
                activeBlueNotes = m_lastComeLastServe.GetActiveMarkers(m_settings.blue);

                this.SendNote(new GameObject[] { activeRedNotes[tactPosWithOffset], activeGreenNotes[tactPosWithOffset], activeBlueNotes[tactPosWithOffset] }, new bool[] { activeRedNotes[nextBeat] == null, activeGreenNotes[nextBeat] == null, activeBlueNotes[nextBeat] == null });
            }
            oldTactPos = tactPosWithOffset;
        }
    }

    private void SendNote(GameObject[] notesToSend, bool[] isNextBeatEmpty)
    {
        messageToSend = msgIndex + "";

        for (int i = 0; i < notesToSend.Length; i++)
        {
            if (notesToSend[i] != null)
            {
                int id = notesToSend[i].GetComponent<FiducialController>().MarkerID;
                int duration = (int)(Settings.GetMarkerWidhMultiplier(id) * 2);
                int tuneHeight = m_tokenposition.GetNote(notesToSend[i].transform.position) % tunesPerString;
                //if the next beat is empty --> damping = 1, else damping = 0 
                int damping = isNextBeatEmpty[i] ? 1 : 0;

                messageToSend += "," + tuneHeight + "," + duration + "," + damping;
                Debug.Log("Marker " + id + " on string " + (i + 1)+  " with fret " + tuneHeight + " will be played for " + duration + ".");
            }
            else
                messageToSend += "," + -1 + "," + -1 + "," + -1;
        }

        lastSentNote++;
        //Debug.Log("Send note " + noteToSend + " (MarkerID = " + noteToSend.fiducialController.MarkerID + ")");

        if (serialPort.IsOpen)
        {
            serialPort.WriteLine(messageToSend);
            Debug.Log("[LOG: wrote cmd: ]" + messageToSend);
        }

        msgIndex++;
    }
}
