using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class GhostFSM : FSM
{
    [SerializeField] private GameObject trail;
    public Animator anim;
    public Material ghostMat;
    public PacmanFSM pacman;
    public Vector3 respawnLocation;
    public float moveSpeed = 20f;
    public float countdown = -1f;
    public bool hasBeenEaten = false;
    public bool isRespawning = false;
    public bool isHunting = false;
    public bool isFleeing = false;
    public bool canTeleport = true;
    private Teleporter[] teleporters;
    public float teleportCounter = 2f;
    public NavMeshAgent navMesh { get; private set; }
    public bool shouldTP = false;
    public Teleporter closeTP;
    public GhostName ghostName;
    public GameObject GhostModel;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        navMesh = GetComponent<NavMeshAgent>();
        pacman = FindObjectOfType<PacmanFSM>();
        GhostModel.SetActive(true);
        trail = this.gameObject.GetComponentInChildren<TrailRenderer>().gameObject;
        if (this.ghostName == GhostName.Inky || this.ghostName == GhostName.Pinky)
        {
            currentState = new GhostFlankState(this);
        }
        else
        {
            currentState = new GhostChaseState(this);
        }
    }

    public void SetTrailOn(bool yesno)
    {
        if (yesno == true)
        {
            trail.SetActive(true);
        }
        else 
        {
            trail.SetActive(false);
        }
    }

    public enum AnimStates 
    {
        Dying,
        Respawning,
        Chasing
    }

    public void SetAnimState(AnimStates newState) 
    {
        switch (newState) 
        {
            case AnimStates.Dying:
            if(anim != null && anim.isActiveAndEnabled){
                anim.SetBool("isRespawning", false);
                anim.SetBool("isChasing", false);
                anim.SetBool("isDying", true);
            }
            break;
            case AnimStates.Respawning:
            if(anim != null && anim.isActiveAndEnabled){
                anim.SetBool("isChasing", false);
                anim.SetBool("isDying", false);
                anim.SetBool("isRespawning", true);
            }
            break;
            case AnimStates.Chasing:
            if(anim != null && anim.isActiveAndEnabled){

                anim.SetBool("isDying", false);
                anim.SetBool("isRespawning", false);
                anim.SetBool("isChasing", true);
            }
            break;
        }
    }

    void Start()
    {           
        teleporters = pacman.GameManager.teleporters;
        closeTP = teleporters[0];
        SeekNearestTeleporter();
        AssignMaterials();
        SetBools();
        AssignDefaultState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.TryGetComponent<Teleporter>(out Teleporter tele)) { return; }
        tele.HandleTeleportation(this.gameObject);
    }

    public void AssignMaterials()
    {
        switch (ghostName)
        {
            case GhostName.Blinky:
                ghostMat = pacman.GameManager.blinkyMat;
                break;
            case GhostName.Pinky:
                ghostMat = pacman.GameManager.pinkyMat;
                break;
            case GhostName.Inky:
                ghostMat = pacman.GameManager.inkyMat;
                break;
            case GhostName.Clyde:
                ghostMat = pacman.GameManager.clydeMat;
                break;
        }
        GetComponentInChildren<Renderer>().material = ghostMat;
    }

    public Vector3 GetRespawnLocation(GhostName ghostName)
    {
        Vector3 target = new Vector3();
        switch (ghostName)
        {
            case GhostName.Blinky:
                target = pacman.GameManager.blinkyspawn.transform.position;
                break;
            case GhostName.Pinky:
                target = pacman.GameManager.pinkyspawn.transform.position;
                break;
            case GhostName.Inky:
                target = pacman.GameManager.inkyspawn.transform.position;
                break;
            case GhostName.Clyde:
                target = pacman.GameManager.clydespawn.transform.position;
                break;
        }
        return target;
    }

    public void SetBools()
    {
        hasBeenEaten = false;
        isRespawning = false;
        isHunting = false;
        isFleeing = false;
    }

    public void AssignDefaultState()
    {
        respawnLocation = GetRespawnLocation(this.ghostName);
        if (this.ghostName == GhostName.Inky || this.ghostName == GhostName.Pinky)
        {
            SwapState(new GhostFlankState(this));
        }
        else
        {
            SwapState(new GhostChaseState(this));
        }
    }

    public void AssignRespawnMaterial()
    {
        GetComponentInChildren<Renderer>().material = pacman.GameManager.respawnMat;
    }

    public void AssignFleeMaterial()
    {
        GetComponentInChildren<Renderer>().material = pacman.GameManager.fleeMat;
    }

    public void TPLOGIC() {
        HandleTPCounter();
        SeekNearestTeleporter();
        CheckShouldTP();
        CalculateDestination();
    }

    void SeekNearestTeleporter() {
        foreach(Teleporter teleporter in teleporters) {
            if (
                Vector3.Distance(
                this.gameObject.transform.position, teleporter.gameObject.transform.position) 
                < 
                Vector3.Distance(
                    this.gameObject.transform.position, closeTP.gameObject.transform.position)
            ) {
                closeTP = teleporter;
            }
        }
    }

    void CheckShouldTP() {
        Vector3 thisPos = this.gameObject.transform.position;
        float distanceToPacman = Vector3.Distance(pacman.transform.position, this.transform.position);
        float distanceToTeleporter = Vector3.Distance(closeTP.transform.position, this.transform.position);

        if (distanceToPacman >= 70f) {
            shouldTP = true;
            if (!canTeleport) {
                shouldTP = false;
            }
        } else {
            shouldTP = false;
        }
    }


    void CalculateDestination() {
        if(navMesh.enabled) {
            if (shouldTP) {
                navMesh.SetDestination(closeTP.gameObject.transform.position);
            } else {
                navMesh.SetDestination(pacman.gameObject.transform.position);
            }
        }

    }


    void HandleTPCounter() {
        if (canTeleport == true) {return;}
        if (canTeleport == false && teleportCounter > 0) {
            teleportCounter -= 1 * Time.deltaTime;
        } else if (teleportCounter < 0 && canTeleport == false) {
            canTeleport = true;
            teleportCounter = 2f;
        }
    }
}

public enum GhostName
{
    Blinky,
    Pinky,
    Clyde,
    Inky
}