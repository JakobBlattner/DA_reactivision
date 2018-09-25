using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//visualize bar between start and end Loop Marker
public class LocationBar : MonoBehaviour
{

    private int bpm;
    public Vector3 startBarPosition;
    public Vector3 endBarPosition;

    private TokenPosition m_tokenPostion;
    private Settings m_settings;
    private BpmManager bpmManager;
    private Rigidbody2D m_rigidbody2D;

    private float cellWidth;

    void Start()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_settings = Settings.Instance;
        m_tokenPostion = TokenPosition.Instance;

        cellWidth = m_settings.cellSizeWorld.x;
        bpmManager = Component.FindObjectOfType<BpmManager>();
        GameObject[] loopMarkers = GameObject.FindGameObjectsWithTag(m_settings.loopMarkerTag);

        foreach (GameObject loopMarker in loopMarkers)
        {

            if (loopMarker.GetComponent<LoopController>().startMarker)
                startBarPosition = loopMarker.transform.position;
            else
                endBarPosition = loopMarker.transform.position;
        }

        this.transform.position = new Vector3(startBarPosition.x, transform.position.y, transform.position.z);
        m_rigidbody2D.velocity = new Vector2((cellWidth * bpm) / 60, 0);
    }

    void FixedUpdate()
    {
        bpm = bpmManager.getBpm();
        m_rigidbody2D.velocity = new Vector2((cellWidth * bpm) / 60, 0);

        //sets position to startBar position if it's x position is below the startbar
        if (this.transform.position.x < startBarPosition.x)
            this.transform.position = new Vector3(startBarPosition.x, transform.position.y, transform.position.z);

        //sets position to startBar position if it's x position is above the endbar
        if (this.transform.position.x > endBarPosition.x)
            this.transform.position = new Vector3(startBarPosition.x, transform.position.y, transform.position.z);
    }

    //Sets new position of StartBar in screen space
    public void SetStartBarPosition(Vector3 newPosition)
    {
        Debug.Log("The start-loopbar has been set from beat " + (m_tokenPostion.GetTactPositionForLoopBarMarker(startBarPosition) + 1) + " to " + (int)(m_tokenPostion.GetTactPositionForLoopBarMarker(newPosition) + 1) + ".");
        this.startBarPosition = newPosition;
    }

    //Sets new position of EndBar in screen space
    public void SetEndBarPosition(Vector3 newPosition)
    {
        Debug.Log("The end-loopbar has been set from beat " + (m_tokenPostion.GetTactPositionForLoopBarMarker(endBarPosition) + 1) + " to " + (m_tokenPostion.GetTactPositionForLoopBarMarker(newPosition) + 1) + ".");
        this.endBarPosition = newPosition;
    }
}
