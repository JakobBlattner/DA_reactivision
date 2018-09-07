using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BpmManager : MonoBehaviour {

    private Settings m_settings;
    private int bpm;

	// Use this for initialization
	void Start () {
        m_settings = Settings.Instance;
        bpm = m_settings.bpm;
	}
	
	// Update is called once per frame
	void Update () {
	    
	}
}
