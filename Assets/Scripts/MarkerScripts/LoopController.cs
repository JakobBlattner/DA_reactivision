using System;
using System.Collections;
using System.Collections.Generic;
using TUIO;
using UniducialLibrary;
using UnityEngine;

public class LoopController : MonoBehaviour
{
    public GameObject ghostPrefab;
    private GameObject ghost;

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

        if (ghostPrefab == null)
            Debug.LogError("No ghost prefab defined.");

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
            if (m_fiducialController.IsSnapped() &&
                this.transform.position.x != newPos.x)
            {
                newPos = this.transform.position;

                //tells locationBar new position
                if (startMarker)
                    m_locationBar.SetStartBarPosition(newPos);
                else
                    m_locationBar.SetEndBarPosition(newPos);
            }

            if (!m_fiducialController.IsSnapped() && ghost == null)
                ghost = GameObject.Instantiate(ghostPrefab, newPos, Quaternion.identity);
            else if (m_fiducialController.IsSnapped())
            {
                Destroy(ghost);
                ghost = null;
            }
        }
        else
            m_obj = m_tuioManager.GetMarker(m_fiducialController.MarkerID);
    }
}
