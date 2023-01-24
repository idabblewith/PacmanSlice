using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Jarid Prince
// Last Updated: 26/10/22 - Added Comments

/// <summary>
/// This script is attached to the minimap camera in order to 
/// create a constant offset above the player and show surroundings
/// with an orthographic perspective. 
/// </summary>

public class MinimapCameraOffset : MonoBehaviour
{
    // Target is player
    [SerializeField] private Transform target;
    // The size of the orthographic camera lens
    [SerializeField] private float size = 25f;
    // The camera this script is attached to
    private Camera thisCam;


    void Start()
    {
        // Assigns thisCam variable as the Camera this script is attached to.
        thisCam = GetComponent<Camera>();
    }

    void Update()
    {
        // Creates a new vector3 with the location of the player
        Vector3 pos = target.transform.position;
        // Adds 100 to the y pos and moves the camera to the new location (for easy selection in scene)
        pos.y += 100;
        this.transform.position = pos;
        // Sets the lens orthographic size to the variable 'size'
        thisCam.orthographicSize = this.size;
    }
}
