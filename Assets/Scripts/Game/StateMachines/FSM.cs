using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    
*/

public abstract class FSM : MonoBehaviour
{
    public FS currentState { get; protected set; }

    private void Update()
    {
        currentState?.OnEveryUpdate(Time.deltaTime);
    }

    public void SwapState(FS nextState)
    {
        currentState?.OnExit();
        currentState = nextState;
        currentState?.OnEnter();
    }
}
