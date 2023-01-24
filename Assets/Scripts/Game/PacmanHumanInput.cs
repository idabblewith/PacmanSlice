using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
    Handles Input with New Input System
*/

public class PacmanHumanInput : MonoBehaviour, Controls.IPacmanActions, Controls.IUIPanelsActions
{

    /*
        Set Implementation of Controls.IAction
        Import System and InputSystem touse events for each action in inputmap.
        Create the actions (events) for each method
        Invoke the events in each respective method (below)
    */

    public Vector2 MovementValue { get; private set; }
    public event Action MinimapEvent;
    public event Action PauseEvent;
    public event Action AcceptEvent;
    public event Action CancelEvent;
    public event Action LoseLifEvent;
    public event Action GetPelletsEvent;
    public event Action<Vector2> ClickEvent;

    /*
        Create a reference from this script to the Pacman Controls Input Map by
        implementing Interface.
        Implement methods of interface
        Create a controls variable which is instantiated as new controls (constructed)
        Enabling the script ot understand each method on the input map with setcallbacks
        - enabling the controls functionality.
    */

    public Controls controls;

    void Start()
    {
        controls = new Controls();
        controls.Pacman.SetCallbacks(this);
        controls.UIPanels.SetCallbacks(this);
        controls.UIPanels.Disable();
        controls.Pacman.Enable();
        // Update method removed as unnecessary
    }

    //Ensures that there are no problems when script is no longer active
    void OnDestroy()
    {
        controls.Pacman.Disable();
        controls.UIPanels.Disable();
    }


    public void ToggleActionMap(InputActionMap actionMap)
    {
        bool currentStateOfActionMap = actionMap.enabled;
        if (currentStateOfActionMap == false) {
            actionMap.Enable(); 
        } else {
            actionMap.Disable();
        }
    }


    #region PacmanInputActions
    public void OnNavigate(InputAction.CallbackContext context)
    {
        MovementValue = context.ReadValue<Vector2>();
    }

    public void OnMap(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        MinimapEvent?.Invoke();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        PauseEvent?.Invoke();
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        CancelEvent?.Invoke();
    }

    public void OnLoseLife(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        LoseLifEvent?.Invoke();
    }

    public void OnGetAllPellets(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        GetPelletsEvent?.Invoke();
    }

    public void OnAccept(InputAction.CallbackContext context)
    {
        if (!context.performed) { return; }
        AcceptEvent?.Invoke();
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
        {
            ClickEvent?.Invoke(Mouse.current.position.ReadValue());
        }
    }

    #endregion

}
