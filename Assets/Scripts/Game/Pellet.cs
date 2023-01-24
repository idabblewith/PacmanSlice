using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

*/

public class Pellet : MonoBehaviour
{
    int scoreValue;
    AudioClip soundEffect;


    private void Awake() {
        if (this.gameObject.tag == "Big Pellet") {
            scoreValue = 2;
        }
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.TryGetComponent(out GameManager GameManager)) {
            AddAndDisappear(GameManager);
        } else return;
    }

    private void AddAndDisappear(GameManager GameManager) {
        GameManager.addToScore(this.scoreValue);
        this.gameObject.SetActive(false);
    }
}
