using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MK;

public class GlowScript : MonoBehaviour
{

    private SpriteRenderer rend;
    private Transform currentLocationBar;

    private Vector3 clb_pos;
    private Color r_color;
    private ColorAccToPosition colorAccToPosition;

    private Vector3 spriteSize;
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
        spriteSize = rend.bounds.size;

        Settings m_settings = Settings.Instance;
        red = m_settings.red;
        redMaterial.color = red;
        blue = m_settings.blue;
        blueMaterial.color = blue;
        green = m_settings.green;
        greenMaterial.color = green;
        currentLocationBar = GameObject.Find(m_settings.locationBarName).transform;
    }
    void LateUpdate()
    {
        if (rend.isVisible)
        {
            //gets current position of the current_location_bar
            clb_pos = currentLocationBar.position;
            r_color = rend.color;

            //if current_location_bar is over this sprite and the marker is not moving, make it glow
            if (clb_pos.x >= (this.transform.position.x - spriteSize.x / 2) && clb_pos.x <= (this.transform.position.x + spriteSize.x / 2)
                && fiducial.MovementDirection == new Vector2(0.0f, 0.0f))
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
