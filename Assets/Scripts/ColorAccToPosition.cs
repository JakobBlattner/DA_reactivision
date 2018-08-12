using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorAccToPosition : MonoBehaviour {

    public Color blue;
    public Color red;
    public Color green;
    private Color currentColor;

    private SpriteRenderer rend;
    private TokenPosition tokenPosition;
    private float note;
    //private Vector2 currentPos;

	void Start() {
        rend = GetComponent<SpriteRenderer>();
        tokenPosition = TokenPosition.Instance;
	}
	
	void FixedUpdate () {
        if (rend.isVisible)
        {
            note = tokenPosition.GetNote(this.transform.position);

            if (note < 8)
                rend.color = blue;
            else if (note < 16)
                rend.color = green;
            else
                rend.color = red;

            currentColor = rend.color;
        }
    }

    public Color GetCurrentColor()
    {
        return currentColor;
    }
}
