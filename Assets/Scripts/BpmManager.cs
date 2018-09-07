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
    private Boolean serialConnected = false;

	// Use this for initialization
	void Start () {
        m_settings = Settings.Instance;
        this.bpm = m_settings.bpm;

        for (int i = 0; i < m_settings.serialPortNamesBpm.Length && !serialConnected; i++)
        {
            try
            {
                Debug.Log("Trying to open serial connection on port: " + m_settings.serialPortNamesBpm[i]);
                serialPort = new SerialPort(m_settings.serialPortNamesBpm[i], m_settings.serialBaudrate);
                serialPort.Open();
                Debug.Log("Opened serial connection on port: " + m_settings.serialPortNamesBpm[i]);
                serialConnected = true;
            }
            catch (IOException)
            {
                Debug.Log("Failed to open serial connection on port: " + m_settings.serialPortNamesBpm[i]);
            }
        }
        if (!serialConnected)
        {
            Debug.LogWarning("No serial connection established, using fixed BPM value: " + bpm);

        }
	}
	
	// Update is called once per frame
	void Update () {
        if (serialConnected && serialPort.IsOpen && serialPort.BytesToRead > 0)
        {
            //bpm = Int32.Parse(serialPort.ReadExisting().Trim());

            try{
                receivedMsg = serialPort.ReadExisting().Trim();
                if (receivedMsg.Contains(";"))
                {
                    int bpmVal = Int32.Parse(receivedMsg.Split(';')[0]);
                    if(bpmVal >= 60){
                        bpm = bpmVal;
                        Debug.Log("[LOG: received BPM: ]" + bpm);        
                    }
                }
            }
            catch (OverflowException)
            {
                ; // ignore this too
            }
            catch (FormatException) 
            {
                ; // ignore this
            }

        }
	}

    public int getBpm() {
        return bpm;
    }
}
