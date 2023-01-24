using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
    All Ghost States in one area.
*/


// Blinky & Clyde Default

public class GhostChaseState : GhostState
{
    float clydeTimer = -1f;
    public GhostChaseState(GhostFSM stateMachine) : base(stateMachine)
    {
    }


    public override void OnEnter()
    {
        stateMachine.GhostModel.SetActive(true);
        stateMachine.SetTrailOn(false);
        stateMachine.AssignMaterials();
        stateMachine.SetAnimState(GhostFSM.AnimStates.Chasing);
        stateMachine.isHunting = true;

        stateMachine.navMesh.isStopped = false;
        stateMachine.navMesh.speed = stateMachine.moveSpeed;

        if (!stateMachine.pacman.GameManager.shouldPlayChaseSounds)
        {
            stateMachine.pacman.GameManager.shouldPlayChaseSounds = true;
        }

        if (stateMachine.ghostName == GhostName.Clyde)
        {
            clydeTimer = Random.Range(0, 6);
        }
    }

    public override void OnEveryUpdate(float deltaTime)
    {
        stateMachine.TPLOGIC();
        if (stateMachine.ghostName == GhostName.Clyde && clydeTimer >= 0)
        {
            clydeTimer -= deltaTime;
            if (clydeTimer <= 0)
            {
                stateMachine.SwapState(new GhostFleeState(stateMachine));
            }
        }
    }

    public override void OnExit()
    {
        stateMachine.isHunting = false;
        stateMachine.pacman.GameManager.shouldPlayChaseSounds = false;
    }

}

// Inky and Pinky Default

public class GhostFlankState : GhostState
{

    public GhostFlankState(GhostFSM stateMachine) : base(stateMachine)
    {
    }

    public override void OnEnter()
    {
        stateMachine.GhostModel.SetActive(true);
        stateMachine.SetTrailOn(false);
        stateMachine.AssignMaterials();
        stateMachine.SetAnimState(GhostFSM.AnimStates.Chasing);
        stateMachine.isHunting = true;

        stateMachine.navMesh.isStopped = false;
        stateMachine.navMesh.speed = stateMachine.moveSpeed;

        if (!stateMachine.pacman.GameManager.shouldPlayChaseSounds)
        {
            stateMachine.pacman.GameManager.shouldPlayChaseSounds = true;
        }
    }

    public override void OnEveryUpdate(float deltaTime)
    {
        stateMachine.TPLOGIC();

        if (stateMachine.isFleeing)
        {
            stateMachine.SwapState(new GhostFleeState(stateMachine));
        }

    }

    public override void OnExit()
    {
        stateMachine.isHunting = false;
        stateMachine.pacman.GameManager.shouldPlayChaseSounds = false;
    }
}


// If Pacman eats power pellet

public class GhostFleeState : GhostState
{
    float clydeTimer = -1f;
    public GhostFleeState(GhostFSM stateMachine) : base(stateMachine)
    {
    }

    public override void OnEnter()
    {
        stateMachine.countdown = 8f;
        stateMachine.isFleeing = true;
        if (!stateMachine.pacman.GameManager.shouldPlayFleeSounds)
        {
            stateMachine.pacman.GameManager.shouldPlayFleeSounds = true;
        }

        
        foreach (GhostFSM ghost in stateMachine.pacman.GameManager.ghosts)
        {
            if (ghost.ghostName != GhostName.Clyde)
            {
                if (ghost.isFleeing != true)
                {
                    clydeTimer = 0;
                }
            }
        }
        if (clydeTimer == 0)
        {
            if (stateMachine.ghostName == GhostName.Clyde)
            {
                clydeTimer = Random.Range(0, 6);
            }
        }
    }

    public override void OnEveryUpdate(float deltaTime)
    {
        if (stateMachine.ghostName == GhostName.Clyde && clydeTimer >= 0)
        {
            clydeTimer -= deltaTime;
            if (clydeTimer <= 0)
            {
                stateMachine.SwapState(new GhostChaseState(stateMachine));
            }
            if (stateMachine.hasBeenEaten == true)
            {
                Debug.Log("Eaten");
                stateMachine.SwapState(new GhostDyingState(stateMachine));
            }
            Vector3 dir = (stateMachine.transform.position - stateMachine.pacman.transform.position).normalized * (stateMachine.navMesh.stoppingDistance * 2);
            NavMesh.SamplePosition(stateMachine.transform.position + dir, out NavMeshHit navMeshHit, (stateMachine.navMesh.stoppingDistance * 2), NavMesh.AllAreas);
            stateMachine.navMesh.SetDestination(navMeshHit.position);
        }
        else 
        {
            if (stateMachine.countdown >= 0)
            {
                stateMachine.countdown -= 1 * Time.deltaTime;
                if (stateMachine.hasBeenEaten == true)
                {
                    Debug.Log("Eaten");
                    stateMachine.SwapState(new GhostDyingState(stateMachine));
                }

                Vector3 dir = (stateMachine.transform.position - stateMachine.pacman.transform.position).normalized * (stateMachine.navMesh.stoppingDistance * 2);
                NavMesh.SamplePosition(stateMachine.transform.position + dir, out NavMeshHit navMeshHit, (stateMachine.navMesh.stoppingDistance * 2), NavMesh.AllAreas);
                stateMachine.navMesh.SetDestination(navMeshHit.position);
            }
            else if (stateMachine.countdown <= 0)
            {
                stateMachine.AssignDefaultState();
            }
        }
    }

    public override void OnExit()
    {
        stateMachine.isFleeing = false;
        stateMachine.countdown = -1;
        stateMachine.pacman.GameManager.shouldPlayFleeSounds = false;
    }
}


public class GhostDyingState : GhostState
{
    public GhostDyingState(GhostFSM stateMachine) : base(stateMachine){}

    float timer = 1.05f;
    public override void OnEnter()
    {
        stateMachine.SetAnimState(GhostFSM.AnimStates.Dying);
    }

    public override void OnEveryUpdate(float deltaTime)
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) 
        {
            stateMachine.SwapState(new GhostRespawnState(stateMachine));
        }

    }

    public override void OnExit()
    {

    }


}

// If Eaten

public class GhostRespawnState : GhostState
{
    public GhostRespawnState(GhostFSM stateMachine) : base(stateMachine)
    {
    }

    public override void OnEnter()
    {
        stateMachine.GhostModel.SetActive(false);
        stateMachine.SetTrailOn(true);
        stateMachine.SetAnimState(GhostFSM.AnimStates.Respawning);
        stateMachine.isRespawning = true;
        if (!stateMachine.pacman.GameManager.shouldPlayRespawnSounds)
        {
            stateMachine.pacman.GameManager.shouldPlayRespawnSounds = true;
        }
    }

    public override void OnEveryUpdate(float deltaTime)
    {
        float dist;
        Vector3 target = this.stateMachine.respawnLocation;
        stateMachine.navMesh.SetDestination(target);
        dist = Vector3.Distance(stateMachine.transform.position, target);
        if (dist <= 1f)
        {
            stateMachine.hasBeenEaten = false;
            ReturnToDefaultState();
        }

    }

    public override void OnExit()
    {
        stateMachine.isRespawning = false;
        stateMachine.pacman.GameManager.shouldPlayRespawnSounds = false;
    }

    void ReturnToDefaultState()
    {
        switch (stateMachine.ghostName)
        {
            case GhostName.Blinky:
            case GhostName.Clyde:
                stateMachine.SwapState(new GhostChaseState(stateMachine));
                break;
            case GhostName.Inky:
            case GhostName.Pinky:
                stateMachine.SwapState(new GhostFlankState(stateMachine));
                break;
        }
    }
}


