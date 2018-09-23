using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniducialLibrary;
using System;

public class LinesForOrientation : MonoBehaviour
{
    public GameObject belongingMarker;
    private Color inActiveColor;

    private Settings m_settings;
    private TokenPosition m_tokenPosition;
    private LastComeLastServe m_lastComeLastServe;
    private SpriteRenderer m_sRend;
    private SpriteRenderer[] childrenSpriteRenderer;

    private Vector3 currentPos;
    private Transform startLoopBar;
    private Transform endLoopBar;
    private SpriteRenderer bm_spriteRenderer;
    private SpriteRenderer left_spriteRenderer;
    private SpriteRenderer right_spriteRenderer;
    private SpriteRenderer top_spriteRenderer;
    private SpriteRenderer bottom_spriteRenderer;
    private float bottomOffset;
    private Transform lineTop;
    private Transform lineBottom;
    private Transform lineLeft;
    private Transform lineRight;

    private float spriteWidthMultiplier;
    private float scaleFactorY;
    private float scaleFactorTopBottomX;
    private float scaleFactorLefRightX;

    private FiducialController m_fiducial;
    private float maxSpeedToShowOtherLinesForOrientation;
    private bool activateColoredLinesForOrientation;
    private int oldBeat;
    private List<LinesForOrientation> otherMarkersOnSameBeat;

    void Start()
    {
        m_fiducial = belongingMarker.GetComponent<FiducialController>();
        m_settings = Settings.Instance;
        m_tokenPosition = TokenPosition.Instance;
        m_lastComeLastServe = GameObject.FindObjectOfType<LastComeLastServe>();

        m_sRend = belongingMarker.GetComponent<SpriteRenderer>();
        childrenSpriteRenderer = GetComponentsInChildren<SpriteRenderer>();

        spriteWidthMultiplier = m_settings.GetMarkerWidthMultiplier(m_fiducial.MarkerID);
        scaleFactorTopBottomX = belongingMarker.transform.localScale.x * m_settings.thickenFactorTopBottomX; ;
        scaleFactorLefRightX = m_settings.scaleFactorLefRightX;
        scaleFactorY = m_settings.scaleFactorY;

        if (lineTop == null)
            lineTop = transform.Find("Line_Top");
        if (lineBottom == null)
            lineBottom = transform.Find("Line_Bottom");
        if (lineLeft == null)
            lineLeft = transform.Find("Line_Left");
        if (lineRight == null)
            lineRight = transform.Find("Line_Right");

        startLoopBar = GameObject.Find(m_settings.startBarLoop).transform;
        endLoopBar = GameObject.Find(m_settings.endtBarLoop).transform;

        bm_spriteRenderer = belongingMarker.GetComponent<SpriteRenderer>();
        left_spriteRenderer = lineLeft.GetComponent<SpriteRenderer>();
        right_spriteRenderer = lineRight.GetComponent<SpriteRenderer>();
        top_spriteRenderer = lineTop.GetComponent<SpriteRenderer>();
        bottom_spriteRenderer = lineBottom.GetComponent<SpriteRenderer>();

        bottomOffset = Camera.main.ScreenToWorldPoint(new Vector3(0, Camera.main.pixelHeight / 2 + m_settings.heightOffSetInPx_bottom, 0)).y;

        maxSpeedToShowOtherLinesForOrientation = m_settings.maxSpeedToShowOtherLinesForOrientation;

        inActiveColor = m_settings.linesForOrientationInactiveColor;
        this.SetColorOfLines(inActiveColor, 1, true);
        otherMarkersOnSameBeat = new List<LinesForOrientation>();
        oldBeat = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_sRend.isVisible)
        {
            //activate spriterenderer in children if deactivated
            if (!childrenSpriteRenderer[0].isVisible)
                EnableChildrenSpriteRenderer(true);

            //get current position and set position of children accordingly
            currentPos = belongingMarker.transform.position;
            lineTop.position = new Vector3(currentPos.x, 5 - (childrenSpriteRenderer[0].bounds.size.y) / 2, lineTop.position.z);
            lineBottom.position = new Vector3(currentPos.x, -5 + (childrenSpriteRenderer[1].bounds.size.y) / 2 + bottomOffset, lineBottom.position.z);
            lineLeft.position = new Vector3(startLoopBar.position.x + (childrenSpriteRenderer[2].bounds.size.x) / 2, currentPos.y, lineLeft.position.z);
            lineRight.position = new Vector3(endLoopBar.position.x - (childrenSpriteRenderer[3].bounds.size.x) / 2, currentPos.y, lineRight.position.z);

            //if(is moving)
            if (!m_fiducial.IsSnapped())
            {
                //make lines thicker
                lineTop.localScale = new Vector3(scaleFactorTopBottomX, scaleFactorY * 2, 1);
                lineBottom.localScale = new Vector3(scaleFactorTopBottomX, scaleFactorY * 2, 1);
                lineLeft.localScale = new Vector3(scaleFactorY, scaleFactorLefRightX * 2, 1);
                lineRight.localScale = new Vector3(scaleFactorY, scaleFactorLefRightX * 2, 1);

                //sets color of this linesForOrientation and...
                this.SetColorOfLines(bm_spriteRenderer.color, 0.5f, true);

                //...other linestForOrientation markers of same beat if speed is slower than the seen value
                if (m_fiducial.Speed < maxSpeedToShowOtherLinesForOrientation)
                    this.ActivateLinesOfOtherMarkersOnSameBeat(true);
                else
                    this.ActivateLinesOfOtherMarkersOnSameBeat(false);
            }
            else if (m_fiducial.IsSnapped() && activateColoredLinesForOrientation)
                this.SetColorOfLines(bm_spriteRenderer.color, 0.5f, false);//colors left and right LinesForOrientation
            else
            {
                //make lines thin again
                lineTop.localScale = new Vector3(scaleFactorTopBottomX, scaleFactorY, 1);
                lineBottom.localScale = new Vector3(scaleFactorTopBottomX, scaleFactorY, 1);
                lineLeft.localScale = new Vector3(scaleFactorY, scaleFactorLefRightX, 1);
                lineRight.localScale = new Vector3(scaleFactorY, scaleFactorLefRightX, 1);

                this.ActivateLinesOfOtherMarkersOnSameBeat(false);
                this.SetColorOfLines(inActiveColor, 1, true);
            }
        }
        //if the marker is not visible, also deactivate the linesForOrientation
        else if (childrenSpriteRenderer[0].isVisible)
        {
            //deactivate spriteRenderer in children
            EnableChildrenSpriteRenderer(false);
        }
    }

    private void ActivateLinesOfOtherMarkersOnSameBeat(bool v)
    {
        int currentBeat = m_tokenPosition.GetTactPosition(m_fiducial.transform.position);
        //deactiveates colors from other markers on oldBeat
        if (oldBeat != currentBeat)
        {
            foreach (LinesForOrientation linesFromOtherMarker in otherMarkersOnSameBeat)
                linesFromOtherMarker.ActivateColoredLinesForOrientation(false);
            otherMarkersOnSameBeat = new List<LinesForOrientation>();
            oldBeat = currentBeat;
        }

        List<GameObject[]> allActiveMarkers = m_lastComeLastServe.GetAllActiveMarkers();

        foreach (GameObject[] guitarString in allActiveMarkers)
        {
            if (guitarString[currentBeat] != null && guitarString[currentBeat].name != m_fiducial.gameObject.name)
            {
                LinesForOrientation linesFromOtherMarker = GameObject.Find(guitarString[currentBeat].name + "_lines").GetComponent<LinesForOrientation>();

                //adds current lines from other marker to list (for color deactivation purpose)
                if (!otherMarkersOnSameBeat.Contains(linesFromOtherMarker))
                    otherMarkersOnSameBeat.Add(linesFromOtherMarker);
                //sets color to other marker on same beat to it's own color
                linesFromOtherMarker.ActivateColoredLinesForOrientation(v);
            }
        }
    }

    private void SetColorOfLines(Color color, float intensity, bool allLines)
    {
        color = new Color(color.r, color.g, color.b, intensity);

        left_spriteRenderer.color = color;
        right_spriteRenderer.color = color;

        //only sets all colours to passed color if allLines is true
        if (allLines)
        {
            top_spriteRenderer.color = color;
            bottom_spriteRenderer.color = color;
        }

        if (color != inActiveColor)
        {
            left_spriteRenderer.sortingOrder = 1;
            right_spriteRenderer.sortingOrder = 1;
            top_spriteRenderer.sortingOrder = 1;
            bottom_spriteRenderer.sortingOrder = 1;
        }
        else
        {
            left_spriteRenderer.sortingOrder = 0;
            right_spriteRenderer.sortingOrder = 0;
            top_spriteRenderer.sortingOrder = 0;
            bottom_spriteRenderer.sortingOrder = 0;
        }
    }

    public void ActivateColoredLinesForOrientation(bool v)
    {
        activateColoredLinesForOrientation = v;
    }

    private void EnableChildrenSpriteRenderer(bool v)
    {
        for (int i = 0; i < childrenSpriteRenderer.Length; i++)
        {
            childrenSpriteRenderer[i].enabled = v;
        }
    }

}
