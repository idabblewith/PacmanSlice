using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Moves item to location on spawn.
    Move to spawn made public for later calling.
*/

public class SpawnHandler : MonoBehaviour
{
    private GameObject spawnGameObject;
    [SerializeField] private Vector3 spawnLocation;

    private void Awake() {
        spawnGameObject = this.gameObject;
        this.MoveToSpawn();
    }

    public void MoveToSpawn() {
        this.spawnGameObject.transform.position = spawnLocation;
    }
}
