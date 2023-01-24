using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    The Base State Other Pacman States will inherit from.
    Protected - only classes that inherit this can access the statemachine
    Constructor - pass in statemachine to assign statemachine to the protected var statemachine
*/

public abstract class PacmanState : FS
{
    protected PacmanFSM stateMachine;

    public PacmanState(PacmanFSM stateMachine)
    {
        this.stateMachine = stateMachine;
    }
}
