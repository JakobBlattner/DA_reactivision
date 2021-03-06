﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteMarker : MonoBehaviour
{
    public int duration = 0;

    public FiducialController fiducialController;
    private Settings m_settings;

    // TODO: May depend on BPM
    private readonly float lastTimeMovedThreshold = 2.2534f;

    // Use this for initialization
    void Start()
    {
        // Determine the duration
        fiducialController = this.GetComponent<FiducialController>();
        m_settings = Settings.Instance;
        duration = (int)(m_settings.GetMarkerWidthMultiplier(fiducialController.MarkerID) * 2);// 1 = 1/4, 2 = 2/4, 3 = 3/4, 4 = 4/4
    }

    void Update()
    {
        /*/Hier stellt sich die Frage, ob es nicht besser ist die Zeit, die der Marker gemoved wurde zu analysieren. Der Abstand ist zu viel vom Threshold abhängig --> schwer zum loggen
        //TODO: Add Codeabschnitt, in dem vermerkt wird, ob der Marker gesetzt wurde (has come alive)

        if (!UniducialLibrary.TuioManager.Instance.IsMarkerAlive(fiducialController.MarkerID))
        {
            if (lastTimeAlive > 0f && Time.time > (lastTimeAlive + lastTimeMovedThreshold))
            {
                //manager.NoteMarkerRemoved(this);
                lastTimeAlive = -1f;
            }
            return;
        }

        // I am alive!
        lastTimeAlive = Time.time;
        var currentPosition = new Vector2(this.transform.position.x, this.transform.position.y);
        var delta = currentPosition - lastPosition;

        if (Mathf.Abs(delta.x) < threshold.x && Mathf.Abs(delta.y) < threshold.y)
        {
            // No can do babydooll
            if (lastTimeMoved > 0f && Time.time > (lastTimeMoved + lastTimeMovedThreshold))
            {
                //manager.NoteMarkerPositioned(this);
                lastTimeMoved = -1f;
            }
            return;
        }

        // I was moved
        lastPosition = currentPosition;
        //manager.NoteMarkerMoved(this, delta);
        lastTimeMoved = Time.time;*/
    }
}
