using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JokerMarker : MonoBehaviour
{

    private int[] pentatonicTunes;
    private int numberOfTunes;
    //private Dictionary<int, Vector3> jokerMarker;
    private Vector3 oldPosition;
    private float realYPosition;
    private SpriteRenderer[] childrenSpriteRenderer;
    private SpriteRenderer m_sRend;
    private Settings m_settings;
    private float cellHeightInPx;
    private float heightOffSet;

    // Use this for initialization
    void Start()
    {
        oldPosition = Vector3.back;
        childrenSpriteRenderer = GetComponentsInChildren<SpriteRenderer>();
        m_sRend = GetComponent<SpriteRenderer>();

        m_settings = Settings.Instance;
        numberOfTunes = m_settings.tunes;
        cellHeightInPx = m_settings.cellHeightInPx;
        heightOffSet = m_settings.heightOffSetInPx;

        pentatonicTunes = m_settings.pentatonicTunes;
    }

    public float CalculateYPosition(Vector3 pos, FiducialController fiducialController)
    {
        //only does something, if the marker lays still
        if (fiducialController.MovementDirection == Vector2.zero)
        {
            //removes marker from list, if the token has been moved on the x axis
            if (oldPosition.x != pos.x)
            {
                oldPosition = Vector3.back;
            }
            //if marker is not in the recognised jokerMarker dictionary - set y Position
            if (oldPosition == Vector3.back)
            {
                //TODO: check if needs to be rewritten to: switch to nearest pentatonic position
                Debug.Log("Joker Marker " + fiducialController.MarkerID + "has been set.");
                realYPosition = pos.y;
                pos.y = heightOffSet + pentatonicTunes[(int)Random.Range(0, pentatonicTunes.Length)] * cellHeightInPx - cellHeightInPx / 2;
                oldPosition = pos;

                return pos.y;
            }
            else
            {
                return oldPosition.y;
            }
        }
        //marker is moving
        return pos.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (childrenSpriteRenderer.Length > 1)
        {
            if (m_sRend.isVisible && !childrenSpriteRenderer[1].isVisible)
                EnableChildrenSpriteRenderer(true);
            else if (!m_sRend.isVisible)
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

    public float GetRealYPosition()
    {
        return realYPosition;
    }
}
