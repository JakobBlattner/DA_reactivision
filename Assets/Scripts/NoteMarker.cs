using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteMarker : MonoBehaviour {
    public Vector2 lastPosition;
    private Vector2 threshold = new Vector2(10.5f, 10.5f);

    Manager manager;
    public int duration = 0;

    public static int startMarkerId = 0;
    public static int endMarkerId = 31;
    public static int dvc = 4; // duration variation count

    public FiducialController fiducialController;
    private float lastTimeMoved;
    private float lastTimeAlive;
    // TODO: May depend on BPM
    private readonly float lastTimeMovedThreshold = 5.2534f;

    // Use this for initialization
	void Start () {
        // Init
        lastPosition = new Vector2(this.transform.position.x, this.transform.position.y);
        manager = FindObjectOfType<Manager>();

        // Determine the duration
        fiducialController = this.GetComponent<FiducialController>();
        var maxNoteCount = endMarkerId - startMarkerId + 1;
        var markerID = fiducialController.MarkerID;
        duration = (markerID - startMarkerId) / (maxNoteCount / dvc) + 1; // 1 = 1/4, 2 = 2/4, 3 = 3/4, 4 = 4/4
	}
	
	// Update is called once per frame
	void Update () {
        if (!UniducialLibrary.TuioManager.Instance.IsMarkerAlive(fiducialController.MarkerID)) {
            if (lastTimeAlive > 0f && Time.time > (lastTimeAlive + lastTimeMovedThreshold)) {
                manager.NoteMarkerRemoved(this);
                lastTimeAlive = -1f;
            }
            return;
        }

        // I am alive!
        lastTimeAlive = Time.time;
        var currentPosition = new Vector2(this.transform.position.x, this.transform.position.y);
        var delta = currentPosition - lastPosition;


        if (Mathf.Abs(delta.x) < threshold.x && Mathf.Abs(delta.y) < threshold.y) {
            // No can do babydooll
            if (lastTimeMoved > 0f && Time.time > (lastTimeMoved + lastTimeMovedThreshold)) {
                manager.NoteMarkerPositined(this);
                lastTimeMoved = -1f;
            }
            return;
        }

        // I was moved
        lastPosition = currentPosition;
        manager.NoteMarkerMoved(this, delta);
        lastTimeMoved = Time.time;
	}
}
