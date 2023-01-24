using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    int scoreValue;
    public ObjectTypes type;
    AudioClip soundEffect;
    public GameManager gameManager;

    private void Awake()
    {
        // gameManager = GetComponent<GameManager>();
    }

    private void Start()
    {
        SetAudioAndValue();
    }

    public void SetAudioAndValue() {
        switch (type)
        {
            case ObjectTypes.Pellet:
                soundEffect = gameManager.GetRandomPelletSound();
                scoreValue = 1;
                break;
            case ObjectTypes.BigPellet:
                soundEffect = gameManager.bigPelletEat;
                scoreValue = 1;
                break;
            case ObjectTypes.Enemy:
                soundEffect = gameManager.ghostEat;
                scoreValue = 5;
                break;
            case ObjectTypes.BonusItem:
                soundEffect = gameManager.bonusEat;
                scoreValue = 50;
                break;
        }
    }


    public void AddAndDisappear()
    {
        if (type == ObjectTypes.BigPellet ||
            type == ObjectTypes.Pellet ||
            type == ObjectTypes.BonusItem)
        {
            gameManager.audioSource.PlayOneShot(soundEffect);
            gameManager.addToScore(scoreValue);
            if (type == ObjectTypes.BigPellet ||
            type == ObjectTypes.Pellet)
            {
                gameManager.RemovePelletFromListandCount(this);
                // if (type == ObjectTypes.BigPellet) {

                    
                //     foreach (GhostFSM ghost in gameManager.ghosts) {
                //         ghost.isFleeing = true;
                //     }
                //     // gameManager.blinky.GetComponent<GhostFSM>().isFleeing = true;
                //     // gameManager.inky.GetComponent<GhostFSM>().isFleeing = true;
                //     // gameManager.pinky.GetComponent<GhostFSM>().isFleeing = true;
                //     // gameManager.clyde.GetComponent<GhostFSM>().isFleeing = true;

                // }
            }
            else
            {
                gameManager.HandleBonusItemEat(this);
                gameManager.bonusItemSpawned = false;
            }
        }
        else if (type == ObjectTypes.Enemy)
        {
            // AND IF ENEMY IS FLEEING {}
            // this.GetComponent<GhostFSM>().CurrentState == GhostFleeState;
        }
        Destroy(this.gameObject);
    }
}
