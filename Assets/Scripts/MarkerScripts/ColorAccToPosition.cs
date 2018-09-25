using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorAccToPosition : MonoBehaviour
{

    private Color blue;
    private Color red;
    private Color green;
    private Color grey;
    private Color currentColor;
    private LastComeLastServe m_lastcomelastserve;
    private FiducialController m_fiducial;
    private SpriteRenderer rend;
    private TokenPosition m_tokenPosition;
    private Settings m_settings;
    private int tunesPerString;
    private float note;

    void Start()
    {
        m_settings = Settings.Instance;
        blue = m_settings.blue;
        red = m_settings.red;
        green = m_settings.green;
        grey = m_settings.grey;

        rend = GetComponent<SpriteRenderer>();
        m_tokenPosition = TokenPosition.Instance;
        tunesPerString = Settings.Instance.tunesPerString;
        currentColor = rend.color;

        m_lastcomelastserve = GameObject.FindObjectOfType<LastComeLastServe>();
        m_fiducial = GetComponent<FiducialController>();
    }

    public void SetCurrentColor(Color color)
    {
        rend.color = color;
        currentColor = color;
    }

    void LateUpdate()
    {
        if (rend.color != grey)
            this.CheckColor();

        if (m_fiducial.IsSnapped() && !m_lastcomelastserve.IsMarkerInActiveMarkers(this.gameObject))
        {
            rend.color = grey;
        }
    }

    public void CheckColor()
    {
        note = m_tokenPosition.GetNote(this.transform.position);

        if (note < tunesPerString)
            rend.color = blue;
        else if (note < tunesPerString * 2)
            rend.color = green;
        else
            rend.color = red;
    }
}
