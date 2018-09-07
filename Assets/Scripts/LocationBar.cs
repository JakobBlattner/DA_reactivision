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
    private LineRenderer m_lineRenderer;

    //for in update loop calculation
    private float timeForTotalDistance;
    private float cellsBetweenBars;
    private float speed;
    private float lengthTravelledInPercent;

    private float cellWidth;
    private int msPerCell;
    public float totalDistance;
    private float startTime;
    private float currentTime;

    void Start()
    {
        m_settings = Settings.Instance;
        m_tokenPostion = TokenPosition.Instance;
        m_lineRenderer = this.GetComponent<LineRenderer>();

        GameObject[] loopMarkers = GameObject.FindGameObjectsWithTag(m_settings.loopMarkerTag);

        foreach (GameObject loopMarker in loopMarkers)
        {

            if (loopMarker.GetComponent<LoopController>().startMarker)
                startBarPosition = loopMarker.transform.position;
            else
                endBarPosition = loopMarker.transform.position;
        }

        //Gets non changing variables
        bpm = m_settings.bpm;
        msPerCell = 60000 / bpm; //in ms
        cellWidth = m_settings.cellSizeWorld.x;
        totalDistance = Vector3.Distance(startBarPosition, endBarPosition);

        //sets first two points of line renderer
        this.ResetLoop();
        startTime = Time.time;
    }

    void FixedUpdate()
    {
        //calculates the speed according to the cells between start and end bar and bpm
        this.UpdateValues();

        currentTime = Time.time;
        lengthTravelledInPercent = ((currentTime - startTime) * speed) / totalDistance;

        Vector3 lerpVec = Vector3.Lerp(startBarPosition, endBarPosition, lengthTravelledInPercent);

        //sets first and second position of LineRenderer
        m_lineRenderer.SetPosition(0, new Vector3(lerpVec.x, -5, 10));
        m_lineRenderer.SetPosition(1, new Vector3(lerpVec.x, 5, 10));

        //resets loop if endBar has been reached by current_location_bar
        if (lerpVec.x >= endBarPosition.x)
            this.ResetLoop();
    }



    private void ResetLoop()
    {
        m_lineRenderer.SetPosition(0, new Vector3(startBarPosition.x, -5, 10));
        m_lineRenderer.SetPosition(1, new Vector3(startBarPosition.x, 5 /*equals max y position in px*/, 10));

        startTime = Time.time;
    }

    //Sets new position of StartBar in screen space
    public void SetStartBarPosition(Vector3 newPosition)
    {

        //Calculates correct behaviour of current_location_bar when startBar changes
        if (m_lineRenderer == null)
        {
            this.m_lineRenderer = this.GetComponent<LineRenderer>();
        }
        Vector3 currentBarPos = m_lineRenderer.GetPosition(0);

        totalDistance = Vector3.Distance(newPosition, endBarPosition);
        this.UpdateValues();

        if (newPosition.x < currentBarPos.x)
        {
            lengthTravelledInPercent = 1 - Math.Abs(currentBarPos.x - endBarPosition.x) / totalDistance;
            startTime = currentTime - (lengthTravelledInPercent * timeForTotalDistance);
        }
        else
        {
            lengthTravelledInPercent = 0;
            startTime = Time.time;
        }

        this.startBarPosition = newPosition;
    }

    //Sets new position of EndBar in screen space
    public void SetEndBarPosition(Vector3 newPosition)
    {
        this.endBarPosition = newPosition;
        totalDistance = Vector3.Distance(startBarPosition, endBarPosition);
    }

    private void UpdateValues()
    {
        cellsBetweenBars = totalDistance / cellWidth;
        timeForTotalDistance = (cellsBetweenBars * msPerCell) / 1000;
        speed = (totalDistance / timeForTotalDistance);
    }



    public int GetTactPosition(Vector2 pos) {
        return TokenPosition.Instance.GetTactPosition(pos);
    }

    /*public int GetNote(Vector2 pos)
    {
        return TokenPosition.Instance.GetNote(pos);
    }*/
}
