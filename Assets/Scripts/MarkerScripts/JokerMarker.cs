using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JokerMarker : MonoBehaviour {

    private int[] pentatonicTunes;
    private int numberOfTunes;
    //private Dictionary<int, Vector3> jokerMarker;
    private Vector3 oldPosition;
    private SpriteRenderer[] childrenSpriteRenderer;
    private SpriteRenderer m_sRend;
    private TokenPosition tokenPosition;
    private float cellHeight;
    private float heightOffSet;

    // Use this for initialization
    void Start()
    {
        //jokerMarker = new Dictionary<int, Vector3>();
        oldPosition = Vector3.back;
        childrenSpriteRenderer = GetComponentsInChildren<SpriteRenderer>();
        m_sRend = GetComponent<SpriteRenderer>();

        tokenPosition = TokenPosition.Instance;
        numberOfTunes = tokenPosition.GetNumberOfTunes();
        cellHeight = tokenPosition.GetCellHeight();
        heightOffSet = tokenPosition.GetHeightOffset();

        //TODO evaluate pentatonic tunes with numberOfTunes and pentatonicTunes array
        pentatonicTunes = new int[3];
        pentatonicTunes[0] = 3;
        pentatonicTunes[1] = 10;
        pentatonicTunes[2] = 18;
    }

    public float CalculateYPosition(Vector3 pos, FiducialController fiducialController)
    {
        //only does something, if the marker lays still
        if (fiducialController.MovementDirection == new Vector2(0.0f, 0.0f))
        {
            //removes marker from list, if the token has been moved on the x axis
            if (oldPosition.x != pos.x)
            {
                oldPosition = Vector3.back;
            }
            //if marker is not in the recognised jokerMarker dictionary - set y Position
            if (oldPosition == Vector3.back)
            {
                Debug.Log("User changed position of Joker with ID " + fiducialController.MarkerID);
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
    void Update () {
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
}
