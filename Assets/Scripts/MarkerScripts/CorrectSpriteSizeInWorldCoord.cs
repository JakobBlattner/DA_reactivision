using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorrectSpriteSizeInWorldCoord : MonoBehaviour
{
    void Awake()
    {
        Settings m_settings = Settings.Instance;
        float width = m_settings.GetMarkerWidthMultiplier(GetComponent<FiducialController>().MarkerID) * 2;
        float scaleFactor = m_settings.cellSizeWorld.x * (transform.parent.name.Equals(m_settings.testMarkerParentName) ? 0.5f : width / 2);
        transform.localScale = new Vector2(scaleFactor, scaleFactor / width);
    }
}
