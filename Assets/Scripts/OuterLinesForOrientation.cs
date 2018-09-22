using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OuterLinesForOrientation : MonoBehaviour
{

    private TokenPosition m_tokenPosition;
    private Settings m_settings;
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
        m_settings = Settings.Instance;
        m_camera = GameObject.FindGameObjectWithTag(m_settings.mainCameraTag).GetComponent<Camera>();

        //Instantiates variables for spawning top and bottom prefabs
        float prefabYBounds = prefab.GetComponent<SpriteRenderer>().bounds.size.y / 2;
        float topYPos = -5 + prefabYBounds; //TODO remove TopOffset
        float bottomYPos = 5 - prefabYBounds;// TODO add BottomOffset
        float xPos;

        //spawns number of beats Outer_Line_For_Orientation prefabs in loop for Top and Bottom
        for (int i = 0; i < m_settings.beats + 1; i++)
        {
            xPos = m_tokenPosition.GetXPosForBeat(i);
            GameObject currentGO = Instantiate(prefab, new Vector3(xPos, topYPos, 0), Quaternion.identity);
            currentGO.transform.parent = this.transform;
            currentGO = Instantiate(prefab, new Vector3(xPos, bottomYPos, 0), Quaternion.identity);
            currentGO.transform.parent = this.transform;
        }

        //Instantiates variables for spawning and updating left and right lines on loopBars
        startLoopBar = GameObject.Find(m_settings.startBarLoop).transform;
        endLoopBar = GameObject.Find(m_settings.endtBarLoop).transform;
        int numberOfTunes = m_settings.tunes;
        leftLines = new Transform[numberOfTunes + 1];
        rightLines = new Transform[numberOfTunes + 1];
        float yPos;

        //spawns number of tunes Outer_Line_For_Orientation prefabs in loop for left and right
        for (int i = 0; i < m_settings.tunes; i++)
        {
            yPos = m_tokenPosition.GetYPosForTune(i);
            leftLines[i] = Instantiate(prefab, new Vector3(startLoopBar.transform.position.x - prefabYBounds, yPos, 0), Quaternion.Euler(new Vector3(0, 0, 90))).transform; //prefabYBounds can be used, because the sprite got turned by 90 degrees
            leftLines[i].transform.parent = startLoopBar.transform;
            rightLines[i] = Instantiate(prefab, new Vector3(endLoopBar.transform.position.x + prefabYBounds, yPos, 0), Quaternion.Euler(new Vector3(0, 0, 90))).transform;
            rightLines[i].transform.parent = endLoopBar.transform;
        }
    }
}
