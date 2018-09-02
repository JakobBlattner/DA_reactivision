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
    private TokenPosition tokenPosition;
    private float cellHeight;
    private float heightOffSet;

    // Use this for initialization
    void Start()
    {
        oldPosition = Vector3.back;
        childrenSpriteRenderer = GetComponentsInChildren<SpriteRenderer>();
        m_sRend = GetComponent<SpriteRenderer>();

        tokenPosition = TokenPosition.Instance;
        numberOfTunes = tokenPosition.GetNumberOfTunes();
        cellHeight = tokenPosition.GetCellHeightInPx();
        heightOffSet = tokenPosition.GetHeightOffset();

        //TODO get from settings class
        pentatonicTunes = new int[9];
        pentatonicTunes[0] = 1;
        pentatonicTunes[1] = 3;
        pentatonicTunes[2] = 6;
        pentatonicTunes[3] = 8;
        pentatonicTunes[4] = 10;
        pentatonicTunes[5] = 13;
        pentatonicTunes[6] = 15;
        pentatonicTunes[7] = 18;
        pentatonicTunes[8] = 20;
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
                Debug.Log("User changed position of Joker with ID " + fiducialController.MarkerID);
                realYPosition = pos.y;
                pos.y = heightOffSet + pentatonicTunes[(int)Random.Range(0, pentatonicTunes.Length)] * cellHeight - cellHeight / 2;
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
