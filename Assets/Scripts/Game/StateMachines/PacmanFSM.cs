using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
/*

*/

public class PacmanFSM : FSM
{
    // public PacmanState currentState;
    public Animator anim;
    public GameObject pacmanObject;
    [field: SerializeField] public PacmanHumanInput PacmanInputReader { get; private set; }
    [field: SerializeField] public GameManager GameManager { get; private set; }
    [field: SerializeField] public CharacterController Controller { get; private set; }
    [field: SerializeField] public float MovementSpeed { get; private set; }

    public float invincibleTime = 3;
    public bool isInvincible = false;
    public Transform pacmanSpawn;
    public GameObject lifeIconsPanel;
    List<RawImage> lifeImages;
    public GameObject lifeIconPrefab;
    public AudioClip deathClip;
    public float respawnTimer = -1;
    public float powerUpTimer = 8f;
    public bool isPaused;
    public bool mapIsOpen;
    public bool canTeleport = true;
    public float teleportCounter = 2f;

    public Camera UICam;
    public GameObject pauseCanvas;
    public GameObject ggCanvas;
    public ButtonGameUI[] pauseButtons;
    public ButtonGameUI[] ggButtons;

    public GameObject models;
    float yRotValue = 0;
    public float navigationCooldown = 0f;


    public void HandleTPCounter() {
        if (canTeleport == true) {return;}
        if (canTeleport == false && teleportCounter > 0) {
            teleportCounter -= 1 * Time.deltaTime;
        } else if (teleportCounter < 0 && canTeleport == false) {
            canTeleport = true;
            teleportCounter = 2f;
        }
    }

    void Awake()
    {
        currentState = new PacmanDefaultState(this);
        anim = GetComponentInChildren<Animator>();
        pauseButtons = pauseCanvas.GetComponentsInChildren<ButtonGameUI>();
        ggButtons = ggCanvas.GetComponentsInChildren<ButtonGameUI>();
    }

    void Start()
    {
        SwapState(new PacmanDefaultState(this));
        UpdateHearts();
    }


    public void UpdateHearts()
    {
        int lives = GameManager.lives;
        lifeImages = new List<RawImage>();
        RawImage[] icons = lifeIconsPanel.gameObject.GetComponentsInChildren<RawImage>();
        foreach(RawImage icon in icons) {
            lifeImages.Add(icon);
        }

        if (lifeImages.Count != lives) {
            RawImage last = lifeImages[lifeImages.Count-1];
            lifeImages.Remove(last);
            last.gameObject.SetActive(false);
        } 
        else  {return;}
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Teleporter>(out Teleporter tele))
        {
            StartCoroutine("TeleportToOtherSide", other.GetComponent<Teleporter>());

        }
        else if (other.gameObject.TryGetComponent<Collectable>(out Collectable item))
        {
            item?.AddAndDisappear();
            if (item.type == ObjectTypes.BigPellet)
            {
                GameManager.ghosts = GameManager.GetGhosts();
                foreach (GhostFSM ghost in GameManager.ghosts)
                {
                    ghost.SwapState(new GhostFleeState(ghost));
                } 
                SwapState(new PacmanPowerupState(this));
            }
        }
        else if (other.gameObject.TryGetComponent<GhostFSM>(out GhostFSM ghost))
        {
            if (ghost.isFleeing)
            {
                GameManager.addToScore(5);
                ghost.hasBeenEaten = true;
            }
            if (ghost.isHunting)
            {
                LifeLogic();
            }
        }
        else
        {
            return;
        }

    }

    void AcceptLogic()
    {
        Debug.Log("Accepted");
        Button selectedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        selectedButton.GetComponent<ButtonGameUI>().RunButtonFunction();
    }

    void CancelLogic()
    {
        if (mapIsOpen)
        {
            MapLogic();
        }
    }


    public void PauseLogicFromDefault()
    {
        SwapState(new PacmanPausedState(this));
    }

    public void PauseLogicFromPaused()
    {
        SwapState(new PacmanDefaultState(this));
    }

    void LifeLogic()
    {
        if (!isInvincible)
        {
            GameManager.lives -= 1;
            UpdateHearts();
            SwapState(new PacmanRespawnState(this));
            if (GameManager.lives == 0) {
                GameManager.GGLogic(false);
            } else {
                StartCoroutine("TeleportToSpawn");
                isInvincible = true;
            }
        }
        // Pause Time for 2 secs then resume
        // if (GameManager.lives <= 0)
        // {
        //     GameManager.Die();
        // }
    }

    IEnumerator TeleportToSpawn()
    {
        this.enabled = false;
        GameManager.DestroyGhosts();
        GameManager.audioSource.PlayOneShot(GameManager.deathClip);
        yield return new WaitForSeconds(0.5f);
        this.gameObject.transform.position = pacmanSpawn.position;

        yield return new WaitForSeconds(0.45f);

        yield return new WaitForSeconds(0.05f);
        this.enabled = true;

        GameManager.SpawnGhosts();

        isInvincible = false;
    }

    public void DisableTeleporters(bool shouldI)
    {
        if (shouldI)
        {
            foreach (Teleporter t in GameManager.teleporters)
            {
                t.gameObject.SetActive(false);
            }
        }
        else
        {
            foreach (Teleporter t in GameManager.teleporters)
            {
                t.gameObject.SetActive(true);
            }
        }

    }

    public IEnumerator TeleportToOtherSide(Teleporter teleporter)
    {
        Vector3 teleportLocation = teleporter.otherSide.transform.position;
        DisableTeleporters(true);
        this.enabled = false;
        GameManager.audioSource.PlayOneShot(GameManager.teleportSound);
        yield return new WaitForSeconds(0.2f);
        this.gameObject.transform.position = teleportLocation;
        yield return new WaitForSeconds(0.2f);
        this.enabled = true;
        yield return new WaitForSeconds(1f);
        DisableTeleporters(false);
    }

    void GetPelletsLogic()
    {
        GameManager.SetPelletsToZero();
    }

    public NavigationDirection UIMovementType(float deltaTime)
    {
        if (PacmanInputReader.MovementValue.y > 0) {
            return NavigationDirection.Up;
        } else if (PacmanInputReader.MovementValue.y < 0) {
            return NavigationDirection.Down;
        }
        if (PacmanInputReader.MovementValue.x == 0 && PacmanInputReader.MovementValue.y == 0) {
            return NavigationDirection.None;
        }
        return NavigationDirection.None;
    }

    public enum NavigationDirection {
        Up, 
        Down,
        Left, 
        Right, 
        None
    }

    public void UIMovementLogic(float deltaTime) {
        GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
        GameObject lowerButton = selectedButton.GetComponent<ButtonGameUI>().lowerButton;
        GameObject upperButton = selectedButton.GetComponent<ButtonGameUI>().upperButton;
        
        NavigationDirection direction = UIMovementType(deltaTime);
        if (navigationCooldown <= 0) {
            switch(direction) {
                case NavigationDirection.Up:
                    EventSystem.current.SetSelectedGameObject(upperButton);
                    navigationCooldown = 0.3f;
                    break;
                case NavigationDirection.Down:
                    EventSystem.current.SetSelectedGameObject(lowerButton);
                    navigationCooldown = 0.3f;
                    break;
                case NavigationDirection.Left:
                case NavigationDirection.Right:
                    break;
                case NavigationDirection.None:
                    break;
            }
        }

    }


    public void MovementLogic(float deltaTime)
    {
        Vector3 movement = new Vector3();
        movement.x = PacmanInputReader.MovementValue.x;
        movement.y = 0;
        movement.z = PacmanInputReader.MovementValue.y;

        // Create old-school pokemon style movement (based on feedback)
        if (movement.x < 1 && movement.x != 0 && movement.x != -1)
        {
            movement.x = 0;
        }
        else if (movement.z < 1 && movement.z != 0 && movement.z != -1)
        {
            movement.z = 0;
        }

        if (movement.z == 1 || movement.x == 1 || movement.x == -1 || movement.z == -1) 
        {
            anim.SetBool("isRunning", true);
        } 
        if (movement.x == 0 && movement.z == 0)
        {
            anim.SetBool("isRunning", false);
        }


        // Use CharContr Move method & ensure height doesnt change
        this.Controller.Move(movement * this.MovementSpeed * deltaTime);

        // Keep from jumping
        if (this.gameObject.transform.position.y > 1.5f)
        {
            Vector3 pos = new Vector3();
            pos = gameObject.transform.position;
            pos.y = 1.5f;
            gameObject.transform.position = pos;
        }

        // Handle Rotation
        if (movement.x > 0) {
            // Right
            yRotValue = 90;
        } else if (movement.x < 0) {
            // Left 
            yRotValue = -90;
        }

        if (movement.z > 0) {
            // Up
            yRotValue = 0;
        } else if (movement.z < 0) {
            // Down
            yRotValue = -180;
        } else if (movement.x == 0 && movement.z == 0) {
            // Nothing - keep previously set rotation
        }
        models.transform.localEulerAngles = new Vector3(0, yRotValue, 0);




        // Do nothing if no input on navigation.
        if (this.PacmanInputReader.MovementValue == Vector2.zero) { return; }
    }

    public void SubLife(bool subscribing = true)
    {
        if (!subscribing) { PacmanInputReader.LoseLifEvent -= LifeLogic; }
        else { PacmanInputReader.LoseLifEvent += LifeLogic; }
    }

    public void SubPellets(bool subscribing = true)
    {
        if (!subscribing) { PacmanInputReader.GetPelletsEvent -= GetPelletsLogic; }
        else { PacmanInputReader.GetPelletsEvent += GetPelletsLogic; }
    }

    public void MapLogic()
    {
        GameManager.fullMapImage.gameObject.SetActive(!GameManager.fullMapImage.gameObject.activeInHierarchy);
        mapIsOpen = GameManager.fullMapImage.gameObject.activeInHierarchy;
    }


    public void SubAccept(bool subscribing = true)
    {
        if (!subscribing) { PacmanInputReader.AcceptEvent -= AcceptLogic; }
        else { PacmanInputReader.AcceptEvent += AcceptLogic; }
    }

    public void SubBack(bool subscribing = true)
    {
        if (!subscribing) { PacmanInputReader.CancelEvent -= CancelLogic; }
        else { PacmanInputReader.CancelEvent += CancelLogic; }

    }

    public void SubPause(bool subscribing = true)
    {
        if (!subscribing) { PacmanInputReader.PauseEvent -= PauseLogicFromDefault; }
        else { PacmanInputReader.PauseEvent += PauseLogicFromDefault; }
    }

    public void SubPauseFromUI(bool subscribing = true)
    {
        if (!subscribing) { PacmanInputReader.CancelEvent -= PauseLogicFromPaused; }
        else { PacmanInputReader.CancelEvent += PauseLogicFromPaused; }
    }

    public void SubMap(bool subscribing = true)
    {
        if (!subscribing) { PacmanInputReader.MinimapEvent -= MapLogic; }
        else { PacmanInputReader.MinimapEvent += MapLogic; }

    }
}
