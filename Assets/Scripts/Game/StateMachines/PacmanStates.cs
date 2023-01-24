using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
    All Pacman States in one area.
*/

#region DEFAULT PACMAN STATE
public class PacmanDefaultState : PacmanState
{
    Light[] li;
    public PacmanDefaultState(PacmanFSM stateMachine) : base(stateMachine) { }
    ~PacmanDefaultState() { }

    public override void OnEnter()
    {
        Debug.Log("Entered default");
        this.stateMachine.SubLife(true);
        this.stateMachine.SubPellets(true);
        this.stateMachine.SubMap(true);
        this.stateMachine.SubPause(true);

        li = this.stateMachine.gameObject.GetComponentsInChildren<Light>();
        foreach(Light l in li)
        {
            if (l.enabled == true)
            l.enabled = false;
        }       
        if (Time.timeScale != 1)
        {
            Time.timeScale = 1;
        }
    }

    public override void OnEveryUpdate(float deltaTime)
    {
        Debug.Log("Updated default");

        if (stateMachine.GameManager.TotalPelletCount == 0)
        {
            stateMachine.GameManager.GGLogic(true);
        }
        this.stateMachine.HandleTPCounter();
        this.stateMachine.MovementLogic(deltaTime);

    }

    public override void OnExit()
    {
        Debug.Log("Exited default");
        this.stateMachine.SubLife(false);
        this.stateMachine.SubPellets(false);
        this.stateMachine.SubMap(false);
        this.stateMachine.SubPause(false);
    }

}
#endregion


#region PAUSED PACMAN STATE
public class PacmanPausedState : PacmanState
{
    public PacmanPausedState(PacmanFSM stateMachine) : base(stateMachine) { }

    public override void OnEnter()
    {
        /*
            Subscribe to UI controls
            Bring up Pause
            Timescale 0
            Disable Pac base controls
            Enable Pac ui controls

        */
        this.stateMachine.SubBack(true);
        this.stateMachine.SubAccept(true);
        this.stateMachine.SubPauseFromUI(true);
        this.stateMachine.PacmanInputReader.ClickEvent += PrintClick;
        Time.timeScale = 0;
        stateMachine.GameManager.pausePanel.SetActive(!stateMachine.GameManager.pausePanel.activeInHierarchy);
        stateMachine.isPaused = stateMachine.GameManager.pausePanel.activeInHierarchy;

        stateMachine.PacmanInputReader.ToggleActionMap(stateMachine.PacmanInputReader.controls.Pacman);
        stateMachine.PacmanInputReader.ToggleActionMap(stateMachine.PacmanInputReader.controls.UIPanels);
    
        Debug.Log("Pacman base controls disabled");
    }

    public override void OnEveryUpdate(float deltaTime)
    {
        // Debug.Log("Paused State");
        deltaTime = Time.unscaledDeltaTime; // to have timer in pause
        stateMachine.navigationCooldown -= deltaTime;
        this.stateMachine.UIMovementLogic(deltaTime);
    }

    public override void OnExit()
    {
        this.stateMachine.SubBack(false);
        this.stateMachine.SubAccept(false);
        this.stateMachine.SubPauseFromUI(false);
        this.stateMachine.PacmanInputReader.ClickEvent += PrintClick;
        stateMachine.GameManager.pausePanel.SetActive(!stateMachine.GameManager.pausePanel.activeInHierarchy);
        stateMachine.isPaused = stateMachine.GameManager.pausePanel.activeInHierarchy;

        stateMachine.PacmanInputReader.ToggleActionMap(stateMachine.PacmanInputReader.controls.Pacman);
        stateMachine.PacmanInputReader.ToggleActionMap(stateMachine.PacmanInputReader.controls.UIPanels);
        Debug.Log("Pacman base controls enabled");
    }

    private void PrintClick(Vector2 pos)
    {
        Debug.Log(pos);
        foreach (ButtonGameUI b in stateMachine.pauseButtons)
        {
            if(RectTransformUtility.RectangleContainsScreenPoint(b.rt, pos, stateMachine.UICam))
            {
                EventSystem.current.SetSelectedGameObject(b.gameObject);
                b.GetComponent<Button>().onClick.Invoke();
            }
        }
    }
}
#endregion

#region GG STATE
public class PacmanGGState : PacmanState
{
    bool areYaWinningSon;
    public PacmanGGState(PacmanFSM stateMachine, bool areYaWinningSon) : base(stateMachine) { 
        this.areYaWinningSon = areYaWinningSon;
    }

    public override void OnEnter()
    {
        this.stateMachine.SubAccept(true);
        this.stateMachine.PacmanInputReader.ClickEvent += PrintClick;

        if (stateMachine.GameManager.fullMapImage.gameObject.activeInHierarchy) {
            stateMachine.GameManager.fullMapImage.gameObject.SetActive(false);
        }

        if (areYaWinningSon)
        {
            TextMeshProUGUI[] titleObjects = stateMachine.GameManager.endPanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI titleObj in titleObjects)
            {
                if (titleObj.name == "Title")
                {
                    titleObj.text = "YOU WIN";
                }
            }
            stateMachine.GameManager.audioSource.volume = 0.3f;
            stateMachine.GameManager.audioSource.PlayOneShot(stateMachine.GameManager.winSound);
        } else {
            stateMachine.GameManager.audioSource.PlayOneShot(stateMachine.GameManager.loseSound);
        }
        Time.timeScale = 0;

        stateMachine.GameManager.endPanel.SetActive(!stateMachine.GameManager.endPanel.activeInHierarchy);
        stateMachine.PacmanInputReader.ToggleActionMap(stateMachine.PacmanInputReader.controls.Pacman);
        stateMachine.PacmanInputReader.ToggleActionMap(stateMachine.PacmanInputReader.controls.UIPanels);
        Debug.Log("Pacman base controls disabled");
    }

    public override void OnEveryUpdate(float deltaTime)
    {
        Debug.Log("Paused State");
        deltaTime = Time.unscaledDeltaTime; // to have timer in pause
        stateMachine.navigationCooldown -= deltaTime;
        this.stateMachine.UIMovementLogic(deltaTime);
    }

    public override void OnExit()
    {
        this.stateMachine.SubAccept(false);
        this.stateMachine.PacmanInputReader.ClickEvent -= PrintClick;
        stateMachine.GameManager.endPanel.SetActive(!stateMachine.GameManager.pausePanel.activeInHierarchy);
        stateMachine.PacmanInputReader.ToggleActionMap(stateMachine.PacmanInputReader.controls.Pacman);
        stateMachine.PacmanInputReader.ToggleActionMap(stateMachine.PacmanInputReader.controls.UIPanels);
        Debug.Log("Pacman base controls enabled");
    }

    private void PrintClick(Vector2 pos)
    {
        Debug.Log(pos);
        foreach (ButtonGameUI b in stateMachine.ggButtons)
        {
            if(RectTransformUtility.RectangleContainsScreenPoint(b.rt, pos, stateMachine.UICam))
            {
                EventSystem.current.SetSelectedGameObject(b.gameObject);
                b.GetComponent<Button>().onClick.Invoke();
            }
        }
    }
}
#endregion


#region RESPAWNING PACMAN STATE
public class PacmanRespawnState : PacmanState
{
    Light[] li;
    public PacmanRespawnState(PacmanFSM stateMachine) : base(stateMachine) { }
    ~PacmanRespawnState() { }

    public override void OnEnter()
    {
        li = this.stateMachine.gameObject.GetComponentsInChildren<Light>();
        foreach(Light l in li)
        {
            if (l.enabled == true)
            l.enabled = false;
        }       
        //Move player to spawn
        stateMachine.anim.SetBool("isDying", false);
        stateMachine.anim.SetBool("isRunning", false);
        this.stateMachine.PacmanInputReader.controls.Pacman.Disable();
        this.stateMachine.transform.position = this.stateMachine.pacmanSpawn.position;
        this.stateMachine.transform.forward = this.stateMachine.pacmanSpawn.forward;
        this.stateMachine.respawnTimer = 0;
    }

    public override void OnEveryUpdate(float deltaTime)
    {
        this.stateMachine.SwapState(new PacmanDefaultState(this.stateMachine));
    }

    public override void OnExit()
    {
        this.stateMachine.PacmanInputReader.controls.Pacman.Enable();
    }

}
#endregion


#region POWERUP PACMAN STATE
public class PacmanPowerupState : PacmanState
{
    Light[] li;

    public PacmanPowerupState(PacmanFSM stateMachine) : base(stateMachine) { }
    ~PacmanPowerupState() { }

    public override void OnEnter()
    {
        Debug.Log("Entered power up");
        li = this.stateMachine.gameObject.GetComponentsInChildren<Light>();
        foreach(Light l in li)
        {
            if (l.enabled == false)
            l.enabled = true;
        }
        this.stateMachine.powerUpTimer = 8f;
        this.stateMachine.SubPellets(true);
        this.stateMachine.SubMap(true);
        this.stateMachine.SubPause(true);    }

    public override void OnEveryUpdate(float deltaTime)
    {
        Debug.Log("Updated power up");

        if (stateMachine.GameManager.TotalPelletCount == 0)
        {
            stateMachine.GameManager.GGLogic(true);
        }
        this.stateMachine.HandleTPCounter();
        this.stateMachine.MovementLogic(deltaTime);
        stateMachine.powerUpTimer -= deltaTime;
        if (this.stateMachine.powerUpTimer <= 0)
        {
            this.stateMachine.SwapState(new PacmanDefaultState(this.stateMachine));
        }
    }

    public override void OnExit()
    {
        Debug.Log("Exited power up");
        this.stateMachine.SubPellets(false);
        this.stateMachine.SubMap(false);
        this.stateMachine.SubPause(false);
    }


}
#endregion

