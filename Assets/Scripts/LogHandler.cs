﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*This class automatically logs every Debug call in this program in a log-file*/
public class LogHandler : MonoBehaviour
{
    private StreamWriter _writer;
    void Awake()
    {
        DateTime now = DateTime.Now;
        string formatted = now.ToString(" dd'.'MM'.'yyyy' 'HH'_'mm'_'ss");
        _writer = File.AppendText("./logs/log" + formatted + ".txt");
        _writer.Write("\n\n=============== Game started ================\n\n");
        DontDestroyOnLoad(gameObject);
        Application.logMessageReceived+=HandleLog;
    }

    private void HandleLog(string condition, string stackTrace, LogType type)
    {
        var logEntry = string.Format("\n {0} {1} \n {2}\n {3}"
            , DateTime.Now, type, condition, stackTrace);
        _writer.Write(logEntry);
    }

    void OnDestroy()
    {
        _writer.Write("\n\n=============== Game ended ================\n\n");
        _writer.Close();
    }
}
