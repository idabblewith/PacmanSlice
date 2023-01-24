using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    The Base State Other Ghost States will inherit from.
    Protected - only classes that inherit this can access the statemachine
    Constructor - pass in statemachine to assign statemachine to the protected var statemachine
*/

public abstract class GhostState : FS
{
    protected GhostFSM stateMachine;

    public GhostState(GhostFSM stateMachine) {
        this.stateMachine = stateMachine;
    }
}