using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//visualize bar between start and end Loop Marker
public class LocationBar : MonoBehaviour {

    //TODO Get BPM from rotary encoder --> Arduino
    public int bpm = 100;
    private Vector3 startBarPosition;
    private Vector3 endBarPosition;

    private TokenPosition m_tokenPostion;
    private LineRenderer m_lineRenderer;

    private float cellWidth;
    private int msPerCell;
    private float distanceToCover;
    private float totalDistance;
    private float startTime;

    //Finds start bar and end bar of loop area
    void Start() {
        GameObject[] loopMarkers = GameObject.FindGameObjectsWithTag("LoopGO");
        foreach (GameObject loopMarker in loopMarkers) {

            if (loopMarker.GetComponent<LoopController>().startMarker)
                startBarPosition = loopMarker.transform.position;//Camera.main.WorldToScreenPoint(loopMarker.transform.position);
            else
                endBarPosition = loopMarker.transform.position;//Camera.main.WorldToScreenPoint(loopMarker.transform.position);
        }

        m_tokenPostion = TokenPosition.Instance;
        m_lineRenderer = this.GetComponent<LineRenderer>();

        //sets first two points of line renderer
        this.ResetLoop();

        //Calculates speed
        msPerCell = 60000 / bpm; //in ms
        cellWidth = m_tokenPostion.GetCellWidthInWorldLength();

        totalDistance = Vector3.Distance(startBarPosition, endBarPosition);
        startTime = Time.time;
    }

    void Update () {
        //calculates the speed according to the cells between start and end bar and bpm
        float cellsBetweenBars = totalDistance / cellWidth;
        float timePerTotalDistance = (cellsBetweenBars* msPerCell)/1000;
        float speed = (totalDistance / timePerTotalDistance);

        float lengthTraveled = ((Time.time - startTime)* speed) / totalDistance;

        Vector3 lerpVec = Vector3.Lerp(startBarPosition, endBarPosition, lengthTraveled);

        //sets first and second position of LineRenderer
        m_lineRenderer.SetPosition(0, new Vector3(lerpVec.x, -5, 10));
        m_lineRenderer.SetPosition(1, new Vector3(lerpVec.x, 5, 10));

        //resets loop if endBar has been reached by current_location_bar
        if (lerpVec.x >= endBarPosition.x)
        {
            this.ResetLoop();
            startTime = Time.time;
        }
    }

    private void ResetLoop()
    {
        m_lineRenderer.SetPosition(0, new Vector3(startBarPosition.x, -5, 10));
        m_lineRenderer.SetPosition(1, new Vector3(startBarPosition.x, 5 /*equals max y position in px*/, 10));
    }

    //Sets new position of StartBar in screen space
    public void SetStartBarPosition(Vector3 newPosition, bool resetLoop)
    {
        this.startBarPosition = newPosition;
        totalDistance = Vector3.Distance(startBarPosition, endBarPosition);

        if (resetLoop)
            this.ResetLoop();
    }

    //Sets new position of EndBar in screen space
    public void SetEndBarPosition(Vector3 newPosition)
    {
        this.endBarPosition = newPosition;
        totalDistance = Vector3.Distance(startBarPosition, endBarPosition);
    }
}
