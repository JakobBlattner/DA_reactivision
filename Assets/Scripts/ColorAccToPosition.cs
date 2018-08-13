using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorAccToPosition : MonoBehaviour {

    public Color blue;
    public Color red;
    public Color green;
    public Color currentColor;

    private bool lastSetTuneOnStringAndBeat;

    private SpriteRenderer rend;
    private TokenPosition tokenPosition;
    private float note;

	void Start() {
        rend = GetComponent<SpriteRenderer>();
        tokenPosition = TokenPosition.Instance;
        lastSetTuneOnStringAndBeat = true;
    }
	
	void FixedUpdate () {
        if (rend.isVisible && lastSetTuneOnStringAndBeat)
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

    public void SetCurrentColor(Color color)
    {
        currentColor = color;
    }

    public void SetAsLastTuneOnStringAndBeat(bool v)
    {
        lastSetTuneOnStringAndBeat = v;
    }
}
