using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JokerSuggestions : MonoBehaviour
{

    private Transform[] jokerMarkers;

    // Use this for initialization
    void Start()
    {
        jokerMarkers = new Transform[6];
        int counter = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).tag.Equals("Marker"))
            {
                jokerMarkers[counter] = transform.GetChild(i);
                counter++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
