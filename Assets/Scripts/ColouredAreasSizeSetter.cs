using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColouredAreasSizeSetter : MonoBehaviour
{

    void Awake()
    {
        Settings m_settings = Settings.Instance;
        float totalHeightOfTunesOnString = m_settings.worldDiff.y / 3 / 2;
        float startPointWithTopOffsetInWorld = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight - m_settings.heightOffSetInPx_top, 0)).y;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform childTransform = transform.GetChild(i).transform;
            childTransform.localScale = new Vector2(childTransform.transform.localScale.x, totalHeightOfTunesOnString);
            childTransform.position = new Vector2(childTransform.position.x, startPointWithTopOffsetInWorld - (totalHeightOfTunesOnString) - i * (totalHeightOfTunesOnString * 2));
        }
    }
}
