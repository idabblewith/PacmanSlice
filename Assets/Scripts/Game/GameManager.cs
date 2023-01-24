using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/*
    Handles the logic for lives, timers, scoring, input action map swapping.
    Also stores reference to the end and pause panels.
*/

public class GameManager : MonoBehaviour
{
    #region Variables
    // UI
    public GameObject pausePanel;
    public RawImage fullMapImage;
    public GameObject endPanel;
    public TMP_FontAsset fontTMPType;
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI remainingPelletText;

    // Spawns
    public GameObject clydespawn;
    public GameObject blinkyspawn;
    public GameObject inkyspawn;
    public GameObject pinkyspawn;
    public GameObject bonusitemspawn;
    public GameObject pacmanspawn;
    private Vector3 clydeSpawnPoint;
    private Vector3 blinkySpawnPoint;
    private Vector3 inkySpawnPoint;
    private Vector3 pinkySpawnPoint;
    private Vector3 bonusItemSpawnPoint;
    private Vector3 pacmanSpawnPoint;

    // Audio]
    public AudioSource ambienceSource;
    public AudioSource audioSource;
    public AudioClip[] pelletEatSounds;
    public AudioClip ambienceSound;
    public AudioClip bigPelletEat;
    public AudioClip bonusEat;
    public AudioClip ghostEat;
    public AudioClip deathClip;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip teleportSound;
    public AudioClip bonusItemSpawnSound;
    public AudioClip ghostRespawningSound;
    public AudioClip ghostActiveSound;
    public AudioClip ghostFleeingSound;
    public bool shouldPlayRespawnSounds = false;
    public bool shouldPlayChaseSounds = false;
    public bool shouldPlayFleeSounds = false;
    public float fleeAudioCounter;
    public float chaseAudioCounter;
    public float respawnAudioCounter;
    public AudioSource fleeSource;
    public AudioSource respawnSource;
    public AudioSource chaseSource;

   // Ingame Gameobjects/TP
    public GameObject Grid;
    public GameObject clyde;
    public GameObject blinky;
    public GameObject inky;
    public GameObject pinky;
    public GameObject pacman;
    public GameObject pelletHolderGO;
    public GameObject bonusItem;

    // Materials
    public Material pinkyMat;
    public Material inkyMat;
    public Material clydeMat;
    public Material blinkyMat;
    public Material respawnMat;
    public Material fleeMat;


    // Variables
    // float updateTimer = 0;
    private int actualScore;
    public int lives = 3;
    private int MaxPelletCount;
    public int TotalPelletCount;
    private int CollectedPelletCount;
    int ItemSpawnInterval;
    private Collectable[] collectables;
    public Teleporter[] teleporters;
    public List<GhostFSM> ghosts;
    private List<Collectable> pellets;
    public bool bonusItemSpawned = false;
    public bool finalBonusSpawned = false;

    // Key Variables
    public PacmanHumanInput InputHandler;
    #endregion

    #region Functions
    // Unity Functions
    private void Awake()
    {
        InputHandler = pacman.GetComponent<PacmanHumanInput>();
        teleporters = Grid.GetComponentsInChildren<Teleporter>();
        collectables = pelletHolderGO.GetComponentsInChildren<Collectable>();
        pellets = new List<Collectable>();
        foreach (Collectable collectable in collectables)
        {
            if (collectable.type == ObjectTypes.Pellet || collectable.type == ObjectTypes.BigPellet)
            {
                pellets.Add(collectable);
            }
        }
        MaxPelletCount = GetTotalPellets();
        AssignSpawners();
        SpawnGhosts();
        SetSourceVolume();
        scoreText.font = fontTMPType;
        actualScore = 0;
        pausePanel.SetActive(false);
        fullMapImage.gameObject.SetActive(false);
        initializeLives();
        ItemSpawnInterval = MaxPelletCount / 100 * 75;
        PlayAmbience();
    }

    private void Update()
    {
        CollectedPelletCount = MaxPelletCount - TotalPelletCount;
        scoreText.text = $"Score: {actualScore}";
        TotalPelletCount = GetTotalPellets();
        remainingPelletText.text = $"{CollectedPelletCount}/{MaxPelletCount}";
        SpawnBonusItem(ItemSpawnInterval);

        SoundLogic();
    }

    // Main / UI
    public void GGLogic(bool areYaWinningSon)
    {
        PacmanFSM stateMachine = pacman.GetComponent<PacmanFSM>();
        stateMachine.SwapState(new PacmanGGState(stateMachine, areYaWinningSon));
    }

    public void RestartLogic()
    {
        if (Time.timeScale != 1)
        {
            Time.timeScale = 1;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

    public void QuitLogic()
    {
        Application.Quit();
    }


    // Audio Functions
    public void SetSourceVolume() {
        ambienceSource.volume = 0.4f;
        fleeSource.volume = 0.3f;
        respawnSource.volume = 0.3f;
        chaseSource.volume = 0.3f;
    }

    public void PlayAmbience() {
        ambienceSource.clip = ambienceSound;
        ambienceSource.Play();
    }

    public void SoundLogic() {
        chaseAudioCounter += Time.deltaTime * 1;
        fleeAudioCounter += Time.deltaTime * 1;
        respawnAudioCounter += Time.deltaTime * 1;
        
        if (shouldPlayChaseSounds && (float)Mathf.CeilToInt(chaseAudioCounter) %5 == 0) {
            chaseSource.PlayOneShot(ghostActiveSound);
            chaseAudioCounter = 0;
        }
        if (shouldPlayFleeSounds && (float)Mathf.CeilToInt(fleeAudioCounter) % 5 == 0) {
            fleeAudioCounter = 0;
            fleeSource.PlayOneShot(ghostFleeingSound);
        }
        if (shouldPlayRespawnSounds && (float)Mathf.CeilToInt(respawnAudioCounter) % 4 == 0) {
            respawnAudioCounter = 0;
            respawnSource.PlayOneShot(ghostRespawningSound);
        }

    }

    public AudioClip GetRandomPelletSound() {
        int pelletSFXLength = pelletEatSounds.Length;
        int randomNumber = Random.Range(0, pelletSFXLength);
        return pelletEatSounds[randomNumber];
    }


    //Score
    public void addToScore(int amount)
    {
        this.actualScore += amount;
    }

    public void resetScore()
    {
        this.actualScore = 0;
    }




    // Ghosts
    public void SpawnGhosts() {
        Instantiate(pinky, pinkyspawn.transform.position, Quaternion.identity);
        Instantiate(inky, inkyspawn.transform.position, Quaternion.identity);
        Instantiate(blinky, blinkyspawn.transform.position, Quaternion.identity);
        Instantiate(clyde, clydespawn.transform.position, Quaternion.identity);
        ghosts = GetGhosts();
    }

    public List<GhostFSM> GetGhosts() {
        List<GhostFSM> ghostss = new List<GhostFSM>();
        GameObject[] b =  UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach(GameObject o in b) {
            if(o.TryGetComponent(out GhostFSM ghost)) {
                ghostss.Add(ghost);
            }
        }
        return ghostss;
    }

    public void DestroyGhosts() {
        foreach(GhostFSM g in ghosts) {
            Destroy(g.gameObject);
        }
    }


    // Lives
    public void initializeLives()
    {
        this.lives = 3;
    }

    public void subtractFromLives(int amount = 1)
    {
        this.lives -= amount;
        if (this.lives <= 0)
        {
            GGLogic(false);
        }
    }

    // Pellets & Bonus Item
    private int GetTotalPellets()
    {
        return pellets.Count;
    }

    public void RemovePelletFromListandCount(Collectable pellet)
    {
        pellets.Remove(pellet);
    }

    public void HandleBonusItemEat(Collectable item)
    {
        bonusItemSpawned = false;
    }

    public void SpawnBonusItem(int interval)
    {
        if (TotalPelletCount <= MaxPelletCount / 100 * 25 - 1)
        {
            if (!finalBonusSpawned)
                finalBonusSpawned = true;
        }

        if (TotalPelletCount <= interval && !finalBonusSpawned)
        {

            if (interval <= MaxPelletCount / 100 * 25)
            {
                if (!bonusItemSpawned)
                {
                    Debug.Log("Spawned Final Bonus");
                    GameObject a = Instantiate(bonusItem, bonusItemSpawnPoint, Quaternion.Euler(-40f,-51f,0f));
                    a.gameObject.SetActive(true);
                    audioSource.PlayOneShot(bonusItemSpawnSound);
                    bonusItemSpawned = true;
                    finalBonusSpawned = true;
                }
            }
            else if (interval <= MaxPelletCount / 100 * 50)
            {
                if (!bonusItemSpawned)
                {
                    Debug.Log("Spawned Second Bonus");
                    GameObject a = Instantiate(bonusItem, bonusItemSpawnPoint, Quaternion.Euler(-40f,-51f,0f));
                    a.gameObject.SetActive(true);
                    audioSource.PlayOneShot(bonusItemSpawnSound);
                    bonusItemSpawned = true;
                }
                ItemSpawnInterval = MaxPelletCount / 100 * 25;

            }
            else if (interval <= MaxPelletCount / 100 * 75)
            {
                if (!bonusItemSpawned)
                {
                    Debug.Log("Spawned First Bonus");
                    GameObject a = Instantiate(bonusItem, bonusItemSpawnPoint, Quaternion.Euler(-40f,-51f,0f));
                    a.gameObject.SetActive(true);
                    audioSource.PlayOneShot(bonusItemSpawnSound);
                    bonusItemSpawned = true;
                }
                ItemSpawnInterval = MaxPelletCount / 100 * 50;
            }
        }
    }

    public void SetPelletsToZero()
    {
        Collectable[] p2 = pellets.ToArray();
        foreach (Collectable p in p2)
        {
            p.AddAndDisappear();
        }
    }

    // Spawns 
    void AssignSpawners() {
        clydeSpawnPoint = clydespawn.gameObject.transform.position;
        blinkySpawnPoint = blinkyspawn.gameObject.transform.position;
        inkySpawnPoint = inkyspawn.gameObject.transform.position;
        pinkySpawnPoint = pinkyspawn.gameObject.transform.position;
        bonusItemSpawnPoint = bonusitemspawn.gameObject.transform.position;
        pacmanSpawnPoint = pacmanspawn.gameObject.transform.position;
    }
    #endregion
}