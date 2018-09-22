using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JokerMarker : MonoBehaviour
{

    private int[] pentatonicTunes;
    private TokenPosition m_tokenPosition;
    private int numberOfTunes;
    //private Dictionary<int, Vector3> jokerMarker;
    private Vector3 oldPosition;
    private float realOldYPosition;
    private SpriteRenderer[] childrenSpriteRenderer;
    private SpriteRenderer m_sRend;
    private Settings m_settings;
    private float cellHeightInPx;
    private float heightOffSet_bottom;
    private LastComeLastServe m_lastComeLastServe;

    // Use this for initialization
    void Start()
    {
        oldPosition = Vector3.back;
        childrenSpriteRenderer = GetComponentsInChildren<SpriteRenderer>();
        m_sRend = GetComponent<SpriteRenderer>();

        m_settings = Settings.Instance;
        numberOfTunes = m_settings.tunes;
        cellHeightInPx = m_settings.cellHeightInPx;
        heightOffSet_bottom = m_settings.heightOffSetInPx_bottom;

        m_lastComeLastServe = GameObject.FindObjectOfType<LastComeLastServe>();
        pentatonicTunes = m_settings.pentatonicTunes;
        m_tokenPosition = TokenPosition.Instance;

        float markerWidthMultiplier = m_settings.GetMarkerWidthMultiplier(GetComponent<FiducialController>().MarkerID);
        Transform jokerIcon = transform.GetChild(1);
        jokerIcon.localScale = new Vector3(jokerIcon.localScale.x / (markerWidthMultiplier*2), jokerIcon.localScale.y, jokerIcon.localScale.z);
    }

    public float CalculateYPosition(Vector3 pos, FiducialController fiducialController, int currentBeat)
    {
        //only does something, if the marker lays still
        if (fiducialController.MovementDirection == Vector2.zero)
        {
            //if marker is not in the recognised jokerMarker dictionary - set y Position
            if (oldPosition.x != pos.x)
            {
                Debug.Log("Joker Marker " + fiducialController.MarkerID + " has been set.");
                realOldYPosition = pos.y;

                //checks which pentatonic tunes are not occupied
                List<List<GameObject>> allActiveMarkers = m_lastComeLastServe.GetAllActiveMarkers();
                List<int> freePentatonicTuneHeights = new List<int>();

                int i = 0;
                bool isInRangeOfString = false;
                //Gets current tune of marker and thereby knows on which string it must calc free tune
                if (m_lastComeLastServe.enableChords)
                {
                    i = m_tokenPosition.GetNote(Camera.main.ScreenToWorldPoint(pos));
                    i = i < m_settings.tunesPerString ? 0 : (i < (m_settings.tunesPerString * 2) ? 1 : 2);
                }

                for (int j = 0; j < pentatonicTunes.Length; j++)
                {
                    //if chords are not enabled, jump between strings
                    if (!m_lastComeLastServe.enableChords && (i + 1) * m_settings.tunesPerString < pentatonicTunes[j])
                        i++;
                    //else if chords are enabled, check if current pentatonic tune is in range of current string
                    else if (m_lastComeLastServe.enableChords && i * m_settings.tunesPerString < pentatonicTunes[j] && pentatonicTunes[j] < (i + 1) * m_settings.tunesPerString)
                        isInRangeOfString = true;
                    else
                        isInRangeOfString = false;

                    if ((m_lastComeLastServe.enableChords ? isInRangeOfString : 1 == 1) && (allActiveMarkers[i][currentBeat] == null || m_tokenPosition.GetNote(allActiveMarkers[i][currentBeat].transform.position) + 1 != pentatonicTunes[j]))
                        freePentatonicTuneHeights.Add(pentatonicTunes[j]);
                }
                Debug.Log(freePentatonicTuneHeights.Count);
                //Gets random pentatonic tune and calculates y position based on said tune
                pos.y = heightOffSet_bottom + freePentatonicTuneHeights[(int)Random.Range(0, freePentatonicTuneHeights.Count)] * cellHeightInPx - cellHeightInPx / 2;
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

    public float GetRealOldYPosition()
    {
        return realOldYPosition;
    }
}
