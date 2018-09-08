using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSetter : MonoBehaviour
{

    public bool red;
    public bool green;
    public bool blue;

    void Start()
    {
        Settings m_settings = Settings.Instance;

        if (GetComponent<SpriteRenderer>())
        {
            SpriteRenderer s_rend = GetComponent<SpriteRenderer>();
            if (red)
                s_rend.color = m_settings.red;
            else if (blue)
                s_rend.color = m_settings.blue;
            else if (green)
                s_rend.color = m_settings.green;

            if (transform.parent.parent.tag.Equals(m_settings.loopMarkerTag))
                s_rend.color = new Color(s_rend.color.r, s_rend.color.g, s_rend.color.b, 0.1f);

            if (!red && !green && !blue)
                Debug.Log(this.name + " has no color set in ColorSetter component.");
        }
        else
            Debug.Log("ColorSetter has been added to a GameObject " + this.name + "without a SpriteRenderer component.");
    }
}
