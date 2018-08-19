﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OuterLinesForOrientation : MonoBehaviour
{

    private TokenPosition m_tokenPosition;
    public GameObject prefab;
    private Transform startLoopBar;
    private Transform endLoopBar;
    private Transform[] leftLines;
    private Transform[] rightLines;
    private Camera m_camera;

    // Use this for initialization
    void Start()
    {
        if (prefab == null)
            Debug.Log("No prefab in Script OuterLinesForOrientation set.");

        m_tokenPosition = TokenPosition.Instance;
        m_camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        //Instantiates variables for spawning top and bottom prefabs
        float prefabYBounds = prefab.GetComponent<SpriteRenderer>().bounds.size.y/2;
        float topYPos = -5 + prefabYBounds;
        float bottomYPos = 5 - prefabYBounds;
        float xPos;

        //spawns number of beats Outer_Line_For_Orientation prefabs in loop for Top and Bottom
        for (int i = 0; i < m_tokenPosition.GetNumberOfBeats(); i++)
        {
            xPos = m_tokenPosition.GetXPosForBeat(i);
            GameObject currentGO = Instantiate(prefab, new Vector3(xPos, topYPos, 0), Quaternion.identity);
            currentGO.transform.parent = this.transform;
            currentGO = Instantiate(prefab, new Vector3(xPos, bottomYPos, 0), Quaternion.identity);
            currentGO.transform.parent = this.transform;
        }

        //Instantiates variables for spawning and updating left and right lines on loopBars
        startLoopBar = GameObject.Find("Loop_Bar_0").transform;
        endLoopBar = GameObject.Find("Loop_Bar_1").transform;
        int numberOfTunes = m_tokenPosition.GetNumberOfTunes();
        leftLines = new Transform[numberOfTunes + 1];
        rightLines = new Transform[numberOfTunes + 1];
        float yPos;

        //spawns number of tunes Outer_Line_For_Orientation prefabs in loop for left and right
        for (int i = 0; i < m_tokenPosition.GetNumberOfTunes(); i++)
        {
            yPos = m_tokenPosition.GetYPosForTune(i);
            leftLines[i] = Instantiate(prefab, new Vector3(startLoopBar.transform.position.x - prefabYBounds, yPos, 0), Quaternion.Euler(new Vector3(0, 0, 90))).transform; //prefabYBounds can be used, because the sprite got turned by 90 degrees
            leftLines[i].transform.parent = startLoopBar.transform;
            rightLines[i] = Instantiate(prefab, new Vector3(endLoopBar.transform.position.x + prefabYBounds, yPos, 0), Quaternion.Euler(new Vector3(0, 0, 90))).transform;
            rightLines[i].transform.parent = endLoopBar.transform;
        }
    }
}