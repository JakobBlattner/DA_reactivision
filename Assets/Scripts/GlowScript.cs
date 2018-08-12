using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MK;

public class GlowScript : MonoBehaviour
{

    private SpriteRenderer rend;
    private LineRenderer lineRenderer;
    private Vector3 lr_pos;
    private char color;
    private Color r_color;
    private Vector3 spriteSize;
    private FiducialController fiducial;

    public Material blueMat;
    public Material greenMat;
    public Material redMat;
    public Material defaultMat;

    void Start()
    {
        //gets renderer
        rend = GetComponent<SpriteRenderer>();

        //checks which color the marker has
        if (rend.color.r > rend.color.b && rend.color.r > rend.color.g)
            color = 'r';
        else if (rend.color.g > rend.color.b && rend.color.g > rend.color.r)
            color = 'g';
        else
            color = 'b';

        GameObject locationBar = GameObject.Find("Current_Location_Bar");
        lineRenderer = locationBar.GetComponent<LineRenderer>();

        fiducial = GetComponent<FiducialController>();
        spriteSize = rend.bounds.size;

        r_color = rend.material.color;
        rend.material = defaultMat;
        r_color = rend.color;
    }
    void Update()
    {
        //gets current position of the current_location_bar
        lr_pos = lineRenderer.GetPosition(0);

        //if locationBar is over this sprite and the marker is not moving, make it glow
        if (lr_pos.x >= (this.transform.position.x - spriteSize.x / 2) && lr_pos.x <= (this.transform.position.x + spriteSize.x / 2)
            && fiducial.MovementDirection == new Vector2(0.0f, 0.0f))
        {
            if (color.Equals('r'))
                rend.material = redMat;
            else if (color.Equals('g'))
                rend.material = greenMat;
            else
                rend.material = blueMat;
        }
        //sets material to defaultMaterial and color to the initial color
        else if (rend.material != defaultMat)
        {
            rend.material = defaultMat;
            rend.color = r_color;
        }

    }
}
