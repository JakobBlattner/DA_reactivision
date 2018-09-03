using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MK;

public class GlowScript : MonoBehaviour
{

    private SpriteRenderer rend;
    private LineRenderer lineRenderer;

    private Vector3 lr_pos;
    //private char color;
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
        GameObject locationBar = GameObject.Find("Current_Location_Bar");
        lineRenderer = locationBar.GetComponent<LineRenderer>();

        fiducial = GetComponent<FiducialController>();

        rend = GetComponent<SpriteRenderer>();
        //colorAccToPosition = GetComponent<ColorAccToPosition>();
        spriteSize = rend.bounds.size;

        Settings settings = Settings.Instance;
        red = settings.red;
        blue = settings.blue;
        green = settings.green;
    }
    void LateUpdate()
    {
        if (rend.isVisible)
        {
            //gets current position of the current_location_bar
            lr_pos = lineRenderer.GetPosition(0);
            r_color = rend.color;

            //if current_location_bar is over this sprite and the marker is not moving, make it glow
            if (lr_pos.x >= (this.transform.position.x - spriteSize.x / 2) && lr_pos.x <= (this.transform.position.x + spriteSize.x / 2)
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
