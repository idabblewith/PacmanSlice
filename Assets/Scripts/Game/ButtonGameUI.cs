using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*

*/

public enum ButtonState {
    Inactive,
    Hovered, 
    Selected
}

public class ButtonGameUI : MonoBehaviour
{

    Button unityButton;
    public ButtonState currentState;
    public GameObject lowerButton;
    public GameObject upperButton;
    public RectTransform rt;

    private void Awake() {
        unityButton = this.gameObject.GetComponent<Button>();
        currentState = ButtonState.Inactive;
        rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == this.gameObject) {
            currentState = ButtonState.Selected;
        } else if (currentState != ButtonState.Hovered || currentState != ButtonState.Inactive) {
            currentState = ButtonState.Inactive;
        }

        if (currentState == ButtonState.Selected) {
            SelectedFunctionality();
        } else if (currentState == ButtonState.Hovered) {
            HoveredFunctionality();
        } else if (currentState == ButtonState.Inactive) {
            InactiveFunctionality();
        }
    }

    private void onMouseEnter() {
        if (currentState != ButtonState.Selected)
        currentState = ButtonState.Hovered;
    }

    private void OnMouseExit() {
        if (currentState != ButtonState.Selected)
        currentState = ButtonState.Inactive;
    }


    void InactiveFunctionality() {
            SetButtonColor(Color.gray);

    }
    
    void HoveredFunctionality() {
        if (unityButton.image.color != Color.green) {
            SetButtonColor(Color.cyan);
        }
    }

    void SelectedFunctionality() {
        if (unityButton.image.color != Color.green) {
            SetButtonColor(Color.green);
        }
        // if accept, RunButtonFunctionality();
    }

    public void RunButtonFunction() {
        unityButton.onClick.Invoke();
    }

    void SetButtonColor(Color color) {
        unityButton.gameObject.GetComponent<Image>().color = color;
    }
}