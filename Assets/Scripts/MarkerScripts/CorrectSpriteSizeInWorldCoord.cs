using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorrectSpriteSizeInWorldCoord : MonoBehaviour
{
    void Awake()
    {
        Settings m_settings = Settings.Instance;
        float lengthAndHeight = m_settings.cellSizeWorld.x;
        transform.localScale = new Vector2(lengthAndHeight * (transform.parent.name.Equals("TestMarkers") ? 0.5f : m_settings.GetMarkerWidthMultiplier(GetComponent<FiducialController>().MarkerID)), lengthAndHeight/2);
    }
}
