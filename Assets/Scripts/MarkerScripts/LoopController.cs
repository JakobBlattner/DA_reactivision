using System;
using System.Collections;
using System.Collections.Generic;
using TUIO;
using UniducialLibrary;
using UnityEngine;

public class LoopController : MonoBehaviour
{
    private Settings m_settings;
    private TuioManager m_tuioManager;
    private TuioObject m_obj;
    private TokenPosition m_tokenPosition;
    private LocationBar m_locationBar;
    private FiducialController m_fiducialController;

    public bool startMarker = false;
    private GameObject otherLoopMarker;
    private Vector3 newPos;

    void Start()
    {
        m_settings = Settings.Instance;
        GameObject[] loopMarkers = GameObject.FindGameObjectsWithTag(m_settings.loopMarkerTag);
        int counter = 0;

        foreach (GameObject loopMarker in loopMarkers)
        {
            if (loopMarker.GetComponent<LoopController>().startMarker)
                counter++;

            //save other LoopMarker
            if (loopMarker.GetComponent<LoopController>().startMarker != this.startMarker)
                otherLoopMarker = loopMarker;

            if (counter > 1)
            {
                Debug.LogError("More than one Loop Start Gameobject defined. Must be exactely one.");
                break;
            }
        }
        if (counter == 0)
            Debug.LogError("No Loop Start GameObject defined. Must be exactely one.");

        m_tuioManager = TuioManager.Instance;
        m_tokenPosition = TokenPosition.Instance;
        m_locationBar = FindObjectsOfType<LocationBar>()[0];
        m_fiducialController = this.GetComponent<FiducialController>();
        transform.position = new Vector3(startMarker ? m_tokenPosition.GetXPosForBeat(0) : m_tokenPosition.GetXPosForBeat(16), transform.position.y, transform.position.z);
        newPos = transform.position;
    }

    void Update()
    {
        if (m_obj != null)
        {
            if (m_obj.getMotionSpeed() == 0 &&
                this.transform.position.x != newPos.x)
            {
                newPos = Camera.main.WorldToScreenPoint(this.transform.position);
                newPos = Camera.main.ScreenToWorldPoint(new Vector3(m_tokenPosition.CalculateXPosition(newPos, true, 1), newPos.y, newPos.z));

                this.transform.position = newPos;

                //tells locationBar new position
                if (startMarker)
                    m_locationBar.SetStartBarPosition(newPos);
                else
                    m_locationBar.SetEndBarPosition(newPos);
            }
        }
        else
            m_obj = m_tuioManager.GetMarker(m_fiducialController.MarkerID);
    }
}
