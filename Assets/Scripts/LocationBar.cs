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

        if(this.transform.position.x < startBarPosition.x)
            this.transform.position = new Vector3(endBarPosition.x, transform.position.y, transform.position.z);

        if (this.transform.position.x > endBarPosition.x)
            this.transform.position = new Vector3(startBarPosition.x, transform.position.y, transform.position.z);
    }

    //Sets new position of StartBar in screen space
    public void SetStartBarPosition(Vector3 newPosition)
    {
        this.startBarPosition = newPosition;
    }

    //Sets new position of EndBar in screen space
    public void SetEndBarPosition(Vector3 newPosition)
    {
        this.endBarPosition = newPosition;
    }
}
