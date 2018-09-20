using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GODSEND;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager CM;
    public TutorialManager tutorialManager; //turns objects off and on based on location in tutorial
    public InGameUIListener inGameUIListener; //interprets input to trigger UI updates 
    public MovesetUI movesetUI; //handles UI of ability icons
    public ArenaManager arenaManager; //handles game timing and end game content
    public ControlPillarManager pillarManager; //handles collection of objectives/updates relevant text
    public PointsManager pointsManager; //receives data about kills and updates points accordingly
    public DeathScreen deathScreen; //shows a "You Died" screen

    public delegate void UIEvent();
    public event UIEvent StartedUsingController, StartedUsingKeyboard;
    public static event UIEvent ResetPlayer, ReloadLevel;
    public bool UsingController { get; set; }
    public bool PreventUIControls { get; set; } //prevent pausing in the middle of death/game ending. Prevent clicking accept before a menu pops up

    public PlayerController player { get; private set; }
    bool oldPlayerInputValue;
    public bool PreventAllPlayerInput
    {
        get { return player.PreventInput; }
        set
        {
            if (value)
            {
                oldPlayerInputValue = player.PreventInput;
                player.PreventInput = value;
            }
            else
                player.PreventInput = oldPlayerInputValue;
        }
    }

    public static bool IsPrepped()
    {
        if (CM is CanvasManager)
            return true;
        else
        {
            Debug.LogError("Canvas manager not assigned, likely because you didn't turn on the UI");
            return false;
        }
    }

    private void Awake()
    {
        if (CM != null)
        {
            Debug.LogWarning("duplicate canvas manager detected. Turning script off");
            this.enabled = false;
        }
        else
            CM = this;
        player = GameObject.FindObjectOfType<PlayerController>().GetComponent<PlayerController>();

        if (GameSceneManager.GSM != null)
            UsingController = GameSceneManager.GSM.UINavActions.LastInputType == InControl.BindingSourceType.DeviceBindingSource;
    }

    public void StartUsingController()
    {
        //UsingController = true;
        if (StartedUsingController is UIEvent)
            StartedUsingController();
    }

    public void StartUsingKeyboard()
    {
        //UsingController = false;
        if (StartedUsingKeyboard is UIEvent)
            StartedUsingKeyboard();
    }

    public void ResetLevel()
    {
        arenaManager.OnRestart();
        pointsManager.OnRestart();
        pillarManager.OnRestart();

        ResetPlayer();
        ReloadLevel();
    }
}
