using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniducialLibrary;

public class LinesForOrientation : MonoBehaviour
{
    public GameObject belongingMarker;
    private Color inActiveColor;

    private Settings m_settings;

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
    private Transform lineTop;
    private Transform lineBottom;
    private Transform lineLeft;
    private Transform lineRight;

    private float spriteWidthMultiplier;
    private float scaleFactorY;
    private float scaleFactorTopBottomX;
    private float scaleFactorLefRightX;

    private FiducialController m_fiducial;

    void Start()
    {
        m_fiducial = belongingMarker.GetComponent<FiducialController>();
        m_settings = Settings.Instance;

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

        inActiveColor = m_settings.linesForOrientationInactiveColor;
        this.SetColorOfLines(inActiveColor);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_sRend.isVisible)
        {
            //activate spriterenderer in children if deactivated
            if (!childrenSpriteRenderer[0].isVisible)
                EnableChildrenSpriteRenderer(true);

            //get current position and set position of children accordingly
            currentPos = belongingMarker.transform.position;
            lineTop.position = new Vector3(currentPos.x, 5 - (childrenSpriteRenderer[0].bounds.size.y) / 2, lineTop.position.z);
            lineBottom.position = new Vector3(currentPos.x, -5 + (childrenSpriteRenderer[1].bounds.size.y) / 2, lineBottom.position.z);
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

                //sets color of this linesForOrientation and TODO: other linestForOrientation markers of same beat 
                this.SetColorOfLines(bm_spriteRenderer.color);
            }
            else
            {
                //make lines thin again
                lineTop.localScale = new Vector3(scaleFactorTopBottomX, scaleFactorY, 1);
                lineBottom.localScale = new Vector3(scaleFactorTopBottomX, scaleFactorY, 1);
                lineLeft.localScale = new Vector3(scaleFactorY, scaleFactorLefRightX, 1);
                lineRight.localScale = new Vector3(scaleFactorY, scaleFactorLefRightX, 1);

                this.SetColorOfLines(inActiveColor);
            }
        }
        //if the marker is not visible, also deactivate the linesForOrientation
        else if (childrenSpriteRenderer[0].isVisible)
        {
            //deactivate spriteRenderer in children
            EnableChildrenSpriteRenderer(false);
        }
    }

    private void SetColorOfLines(Color color)
    {
        left_spriteRenderer.color = color;
        right_spriteRenderer.color = color;
        top_spriteRenderer.color = color;
        bottom_spriteRenderer.color = color;
    }

    private void EnableChildrenSpriteRenderer(bool v)
    {
        for (int i = 0; i < childrenSpriteRenderer.Length; i++)
        {
            childrenSpriteRenderer[i].enabled = v;
        }
    }

}
