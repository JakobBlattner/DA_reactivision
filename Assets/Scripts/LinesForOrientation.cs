using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniducialLibrary;

public class LinesForOrientation : MonoBehaviour
{
    public GameObject belongingMarker;

    private SpriteRenderer m_sRend;
    private SpriteRenderer[] childrenSpriteRenderer;

    private Vector3 currentPos;
    private Transform startLoopBar;
    private Transform endLoopBar;

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

        m_sRend = belongingMarker.GetComponent<SpriteRenderer>();
        childrenSpriteRenderer = GetComponentsInChildren<SpriteRenderer>();

        spriteWidthMultiplier = TokenPosition.GetMarkerWithMultiplier(m_fiducial.MarkerID);
        scaleFactorTopBottomX = belongingMarker.transform.localScale.x * 2; ;
        scaleFactorLefRightX = childrenSpriteRenderer[2].transform.localScale.x / 2;
        scaleFactorY = childrenSpriteRenderer[0].transform.localScale.y;

        if (lineTop == null)
            lineTop = transform.Find("Line_Top");
        if (lineBottom == null)
            lineBottom = transform.Find("Line_Bottom");
        if (lineLeft == null)
            lineLeft = transform.Find("Line_Left");
        if (lineRight == null)
            lineRight = transform.Find("Line_Right");

        startLoopBar = GameObject.Find("Loop_Bar_0").transform;
        endLoopBar = GameObject.Find("Loop_Bar_1").transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_sRend.isVisible)
        {
            //activate spriterenderer in children if deactivated
            if (!childrenSpriteRenderer[0].isVisible)
                EnableChildrenSpriteRenderer(true);

            //get current position and set position oof children accordingly
            currentPos = belongingMarker.transform.position;
            lineTop.position = new Vector3(currentPos.x, 5 - (childrenSpriteRenderer[0].bounds.size.y) / 2, lineTop.position.z);
            lineBottom.position = new Vector3(currentPos.x, -5 + (childrenSpriteRenderer[1].bounds.size.y) / 2, lineBottom.position.z);
            lineLeft.position = new Vector3(startLoopBar.position.x + (childrenSpriteRenderer[2].bounds.size.x) / 2, currentPos.y, lineLeft.position.z);
            lineRight.position = new Vector3(endLoopBar.position.x - (childrenSpriteRenderer[3].bounds.size.x) / 2, currentPos.y, lineRight.position.z);

            //if(is moving)
            if (!m_fiducial.IsSnapped())//m_fiducial.MovementDirection != new Vector2(0.0f, 0.0f))
            {
                //make lines thicker
                lineTop.localScale = new Vector3(scaleFactorTopBottomX, scaleFactorY * 2, 1);
                lineBottom.localScale = new Vector3(scaleFactorTopBottomX, scaleFactorY * 2, 1);
                lineLeft.localScale = new Vector3(scaleFactorY, scaleFactorLefRightX * 2, 1);
                lineRight.localScale = new Vector3(scaleFactorY, scaleFactorLefRightX * 2, 1);
            }
            else
            {
                //make lines thin again
                lineTop.localScale = new Vector3(scaleFactorTopBottomX, scaleFactorY, 1);
                lineBottom.localScale = new Vector3(scaleFactorTopBottomX, scaleFactorY, 1);
                lineLeft.localScale = new Vector3(scaleFactorY, scaleFactorLefRightX, 1);
                lineRight.localScale = new Vector3(scaleFactorY, scaleFactorLefRightX, 1);
            }
        }
        //if the marker is not visible, also deactivate the linesFor Orientation
        else if (childrenSpriteRenderer[0].isVisible)
        {
            //deactivate spriteRenderer in children
            EnableChildrenSpriteRenderer(false);
        }
    }

    private void EnableChildrenSpriteRenderer(bool v)
    {
        for (int i = 0; i < childrenSpriteRenderer.Length; i++)
        {
            childrenSpriteRenderer[i].enabled = v;
        }
    }

}
