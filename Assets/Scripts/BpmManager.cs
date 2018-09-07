using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using UnityEngine;

public class BpmManager : MonoBehaviour {

    private Settings m_settings;
    private int bpm;

    // Serial communication
    private static SerialPort serialPort;
    private static String[] serialPortNames;
    private int serialBaudrate;
    private String receivedMsg = "";


	// Use this for initialization
	void Start () {
        m_settings = Settings.Instance;
        bpm = m_settings.bpm;
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
            Debug.LogWarning("No serial connection established, using fixed BPM value: " + bpm);

        }
	}
	
	// Update is called once per frame
	void Update () {
        if (serialPort.IsOpen)
        {
            bpm = Int32.Parse(serialPort.ReadExisting().Trim());
            Debug.Log("[LOG: received BPM: ]" + bpm);
        }
	}
}
