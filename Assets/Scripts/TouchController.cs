using System;
using System.Collections.Generic;
using UnityEngine;


public class TouchController : MonoBehaviour
{
    public int CursorID= 0;

    //translation
    public bool IsPositionMapped = false;
    public bool InvertX = false;
    public bool InvertY = false;

    //Loop Touch
    public bool movingLoopMarker = false;

    //rotation
    public bool AutoHideGO = false;
    private bool m_ControlsGUIElement = false;


    public float CameraOffset = 10;
    private UniducialLibrary.TuioManager m_TuioManager;
    private Camera m_MainCamera;

    //members
    private Vector2 m_ScreenPosition;
    private Vector3 m_WorldPosition;
    private Vector2 m_Direction;
    private float m_Speed;
    private float m_Acceleration;
    private bool m_IsVisible;

    void Awake()
    {
        this.m_TuioManager = UniducialLibrary.TuioManager.Instance;

        //uncomment next line to set port explicitly (default is 3333)
        //tuioManager.TuioPort = 7777;

        this.m_TuioManager.Connect();


        this.m_ScreenPosition = Vector2.zero;
        this.m_WorldPosition = Vector3.zero;
        this.m_Direction = Vector2.zero;
        this.m_Speed = 0f;
        this.m_Acceleration = 0f;
        this.m_IsVisible = true;
    }

    void Start()
    {
        //get reference to main camera
        this.m_MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        //check if the main camera exists
        if (this.m_MainCamera == null)
        {
            Debug.LogError("There is no main camera defined in your scene.");
        }
    }

    void Update()
    {
        if (this.m_TuioManager.IsConnected
            && this.m_TuioManager.IsCursorAlive(this.CursorID))
        {
            TUIO.TuioCursor cursor= this.m_TuioManager.GetCursor(this.CursorID);

            //update parameters
            this.m_ScreenPosition.x = cursor.getX();
            this.m_ScreenPosition.y = cursor.getY();
            this.m_Speed = cursor.getMotionSpeed();
            this.m_Acceleration = cursor.getMotionAccel();
            this.m_Direction.x = cursor.getXSpeed();
            this.m_Direction.y = cursor.getYSpeed();
            this.m_IsVisible = true;

            //set game object to visible, if it was hidden before
            ShowGameObject();

            //update transform component
            UpdateTransform();
        }
        else
        {
            //automatically hide game object when marker is not visible
            if (this.AutoHideGO)
            {
                HideGameObject();
            }

            this.m_IsVisible = false;
        }
    }


    void OnApplicationQuit()
    {
        if (this.m_TuioManager.IsConnected)
        {
            this.m_TuioManager.Disconnect();
        }
    }

    private void UpdateTransform()
    {
        //position mapping
        if (this.IsPositionMapped)
        {
            //calculate world position with respect to camera view direction
            float xPos = this.m_ScreenPosition.x;
            float yPos = this.m_ScreenPosition.y;
            if (this.InvertX) xPos = 1 - xPos;
            if (this.InvertY) yPos = 1 - yPos;

            if (this.m_ControlsGUIElement)
            {
                transform.position = new Vector3(xPos, 1 - yPos, 0);
            }
            else
            {
                Vector3 position = new Vector3(xPos * Screen.width,
                    (1 - yPos) * Screen.height, this.CameraOffset);
                this.m_WorldPosition = this.m_MainCamera.ScreenToWorldPoint(position);
                //worldPosition += cameraOffset * mainCamera.transform.forward;
                transform.position = this.m_WorldPosition;
            }
        }
    }

    private void ShowGameObject()
    {
        if (this.m_ControlsGUIElement)
        {
            //show GUI components
            if (gameObject.GetComponent<GUIText>() != null && !gameObject.GetComponent<GUIText>().enabled)
            {
                gameObject.GetComponent<GUIText>().enabled = true;
            }
            if (gameObject.GetComponent<GUITexture>() != null && !gameObject.GetComponent<GUITexture>().enabled)
            {
                gameObject.GetComponent<GUITexture>().enabled = true;
            }
        }
        else
        {
            if (gameObject.GetComponent<Renderer>() != null && !gameObject.GetComponent<Renderer>().enabled)
            {
                gameObject.GetComponent<Renderer>().enabled = true;
            }
        }
    }

    private void HideGameObject()
    {
        if (this.m_ControlsGUIElement)
        {
            //hide GUI components
            if (gameObject.GetComponent<GUIText>() != null && gameObject.GetComponent<GUIText>().enabled)
            {
                gameObject.GetComponent<GUIText>().enabled = false;
            }
            if (gameObject.GetComponent<GUITexture>() != null && gameObject.GetComponent<GUITexture>().enabled)
            {
                gameObject.GetComponent<GUITexture>().enabled = false;
            }
        }
        else
        {
            //set 3d game object to visible, if it was hidden before
            if (gameObject.GetComponent<Renderer>() != null && gameObject.GetComponent<Renderer>().enabled)
            {
                gameObject.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    #region Getter

    public bool isAttachedToGUIComponent()
    {
        return (gameObject.GetComponent<GUIText>() != null || gameObject.GetComponent<GUITexture>() != null);
    }
    public Vector2 ScreenPosition
    {
        get { return this.m_ScreenPosition; }
    }
    public Vector3 WorldPosition
    {
        get { return this.m_WorldPosition; }
    }
    public Vector2 MovementDirection
    {
        get { return this.m_Direction; }
    }
    public float Speed
    {
        get { return this.m_Speed; }
    }
    public float Acceleration
    {
        get { return this.m_Acceleration; }
    }
    public bool IsVisible
    {
        get { return this.m_IsVisible; }
    }
    #endregion
}

