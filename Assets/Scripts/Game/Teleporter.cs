using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Teleporter : MonoBehaviour
{
    PacmanFSM pacman;
    public Vector3 teleporterLocation;
    public Teleporter otherSide;

    public void HandleTeleportation(GameObject entity)
    {
        if (CheckCanTeleport(entity) == true) { 
            if (entity.TryGetComponent<GhostFSM>(out GhostFSM ghost)) {
                ghost.canTeleport = false;
            } else if (entity.TryGetComponent<PacmanFSM>(out PacmanFSM paccy)) {
                paccy.canTeleport = false;
            }

            if (entity.TryGetComponent<NavMeshAgent>(out NavMeshAgent navMesh)) { 
                navMesh.enabled = false;
                entity.transform.position = otherSide.gameObject.transform.position;
                navMesh.enabled = true;
            } 
        } else {
            return;
        }
    }

    public bool CheckCanTeleport(GameObject entity)
    {
        if (entity.TryGetComponent<GhostFSM>(out GhostFSM ghost)) {
            if (ghost.canTeleport) {
                return true;
            } else {return false;}
        } else if(entity.TryGetComponent<PacmanFSM>(out PacmanFSM paccy)) {
            if(paccy.canTeleport) {
                return true;
            } else {
                return false;
            }
        } 
        else {
            return false;
        }
    }


    void Start()
    {

        pacman = GetComponent<PacmanFSM>();
        teleporterLocation = this.gameObject.transform.position;
    }

    void Update()
    {

    }


}
