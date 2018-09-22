using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColouredAreasSizeSetter : MonoBehaviour
{

    void Awake()
    {
        Settings m_settings = Settings.Instance;
        float totalHeightOfTunesOnString = /*m_settings.tunesPerString **/m_settings.cellSizeWorld.y;

        transform.localScale = new Vector2(transform.localScale.x, totalHeightOfTunesOnString);
        //transform.localPosition = new Vector2(transform.localPosition.x,
            //Camera.main.ScreenToWorldPoint(new Vector3(0, (m_settings.heightOffSetInPx_bottom + m_settings.gridHeightInPx), 0)).y);
    }
}
