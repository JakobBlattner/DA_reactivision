using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MK;

public class GlowScript : MonoBehaviour
{

    private SpriteRenderer rend;

    private Color r_color;
    private LastComeLastServe m_lastComeLastServe;

    private FiducialController fiducial;

    public Material blueMaterial;
    public Material greenMaterial;
    public Material redMaterial;
    public Material defaultMaterial;

    private Color red;
    private Color blue;
    private Color green;

    void Start()
    {
        fiducial = GetComponent<FiducialController>();
        rend = GetComponent<SpriteRenderer>();

        Settings m_settings = Settings.Instance;
        red = m_settings.red;
        redMaterial.color = red;
        blue = m_settings.blue;
        blueMaterial.color = blue;
        green = m_settings.green;
        greenMaterial.color = green;

        m_lastComeLastServe = Object.FindObjectOfType<LastComeLastServe>();
    }
    void LateUpdate()
    {
        if (rend.isVisible)
        {
            //gets current position of the current_location_bar
            r_color = rend.color;

            //if this marker is in list of active markers and current_location_bar is over this sprite and the marker is not moving, make it glow
            if (m_lastComeLastServe.IsBeingPlayed(this.gameObject) && fiducial.IsSnapped())
            {
                //chooses correct glow material to be set
                if (r_color == red)
                    rend.material = redMaterial;
                else if (r_color == green)
                    rend.material = greenMaterial;
                else if (r_color == blue)
                    rend.material = blueMaterial;
            }
            //sets material to defaultMaterial if current_location_bar is not over this sprite
            else if (rend.material != defaultMaterial)
                rend.material = defaultMaterial;
        }

    }
}
