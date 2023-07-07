using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    #region movement
    public Vector2 input
    {
        get
        {
            Vector2 i = Vector2.zero;
            i.x = Input.GetAxis("Horizontal");
            i.y = Input.GetAxis("Vertical");
            i *= (i.x != 0.0f && i.y != 0.0f) ? .7071f : 1.0f;
            return i;
        }
    }

    public Vector2 raw
    {
        get
        {
            Vector2 i = Vector2.zero;
            i.x = Input.GetAxisRaw("Horizontal");
            i.y = Input.GetAxisRaw("Vertical");
            i *= (i.x != 0.0f && i.y != 0.0f) ? .7071f : 1.0f;
            return i;
        }
    }

    public bool walk
    {
        get { return Input.GetKey(KeyCode.LeftShift); }
    }

    public bool crouch
    {
        get { return Input.GetKey(KeyCode.LeftControl); }
    }

    public bool jump
    {
        get { return Input.GetKeyDown(KeyCode.Space); }
    }

    #endregion

    public Vector2 mouse
    {
        get
        {
            Vector2 i = Vector2.zero;
            i.x = Input.GetAxis("Mouse X");
            i.y = Input.GetAxis("Mouse Y");
            return i;
        }
    }

    #region interact

    public KeyCode interactKey
    { 
        get { return KeyCode.E; }
    }

    public bool interact
    {
        get { return Input.GetKeyDown(interactKey); }
    }

    #endregion

    #region combat

    public bool blockpressed
    {
        get { return Input.GetMouseButtonDown(1); }
    }

    public bool blockreleased
    {
        get { return Input.GetMouseButtonUp(0); }
    }

    public bool blocking
    {
        get { return Input.GetMouseButton(1); }
    }

    public bool attackpressed
    {
        get { return Input.GetMouseButtonDown(0); }
    }

    public bool attackreleased
    {
        get { return Input.GetMouseButtonUp(0); }
    }

    public bool attacking
    {
        get { return Input.GetMouseButton(0); }
    }

    #endregion

    public Vector2 down
    {
        get { return _down; }
    }

    private Vector2 previous;
    private Vector2 _down;

    void Update()
    {
        _down = Vector2.zero;
        if (raw.x != previous.x)
        {
            previous.x = raw.x;
            if (previous.x != 0)
                _down.x = previous.x;
        }
        if (raw.y != previous.y)
        {
            previous.y = raw.y;
            if (previous.y != 0)
                _down.y = previous.y;
        }
    }

    //for testing only: FPS to TPS camera and back
    public bool cameraSwitch
    {
        get { return Input.GetKeyDown(KeyCode.C); }
    }

    #region ability

    public bool Ability1Selected
    {
        get { return Input.GetKeyDown(KeyCode.Alpha1); }
    }
    public bool Ability2Selected
    {
        get { return Input.GetKeyDown(KeyCode.Alpha2); }
    }
    public bool Ability3Selected
    {
        get { return Input.GetKeyDown(KeyCode.Alpha3); }
    }
    public bool Ability4Selected
    {
        get { return Input.GetKeyDown(KeyCode.Alpha4); }
    }
    public bool Ability5Selected
    {
        get { return Input.GetKeyDown(KeyCode.Alpha5); }
    }

    #endregion

}
