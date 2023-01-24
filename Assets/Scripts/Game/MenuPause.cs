using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*

*/

public class MenuPause : MonoBehaviour
{

    ButtonGameUI[] menuButtons;
    GameObject currentButton;

    private void OnEnable() {
        menuButtons = GetComponentsInChildren<ButtonGameUI>();
        EventSystem.current.SetSelectedGameObject(menuButtons[0].gameObject);
    }


    private void Awake() {
        menuButtons = GetComponentsInChildren<ButtonGameUI>();
        EventSystem.current.SetSelectedGameObject(menuButtons[0].gameObject);
    }

    // void Start()
    // {
        
    // }

    void Update()
    {
        if (currentButton != EventSystem.current.currentSelectedGameObject) {
            currentButton = EventSystem.current.currentSelectedGameObject;
        }
        // Debug.Log(currentButton.name);
    }

    // private void OnDisable() {
    //     EventSystem.current.SetSelectedGameObject(null);
    // }

}