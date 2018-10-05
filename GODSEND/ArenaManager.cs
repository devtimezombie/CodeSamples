using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GODSEND;

public class ArenaManager : MonoBehaviour
{
    public bool isGameLevel = true; //is not tutorial
    StartScreen objectiveScreen;
    public Text objectiveNav;

    public GameOverScreen gameOverScreen;
    bool hasWon;
    public bool AllowedToGainPoints { get; private set; }

    #region Time Limit UI
    public Slider countdownSlider;
    Image sliderFill;

    [SerializeField]
    ParticleSystem timerSparks, timerPulse;
    float priorTimeScale;

    Coroutine countdown;
    public int maxSeconds = 180; //length of game
    int secondsElapsed, completionTime;
    WaitForSeconds oneSecond = new WaitForSeconds(1); //cache coroutine gap

    int countdownStageCurrent = 1;
    int countdownStagePrior = 1;

    //UI data for timer
    float perSecondLerp; //lerp ui visual for timer
    float timerHandleOld, timerHandleGoal;
    Color timerGoalColor;

    ParticleSystem.MainModule sparksMain, pulseMain;
    #endregion

    #region intro and outro cutscene
    //tells character to run into the scene at the beginning of the game, for a length of time
    public float startCutsceneLength = 2;
    public Vector3 startCutsceneWalkDir = Vector3.right;
    public float endCutsceneLength = 2;
    public Vector3 endCutsceneWalkDir = Vector3.right;
    public Vector3 CutsceneWalkVector
    {
        get
        {
            return hasWon ? endCutsceneWalkDir : startCutsceneWalkDir;
        }
    }
    public bool useCountdown = true;
    #endregion

    //events for stopping enemy attacks and other things relating to the game state
    public delegate void TimedEvent();
    public static event TimedEvent LevelLoaded;
    public event TimedEvent ObjectiveScreenHidden, CountdownCompleted, GameResolved, CutsceneStarted, CutsceneEnded;


    void Start()
    {
        objectiveScreen = GetComponent<StartScreen>();

        if (!Application.isEditor && isGameLevel)
        {
            useCountdown = true;
        }

        countdownSlider.gameObject.SetActive(false);
        sliderFill = countdownSlider.fillRect.GetComponent<Image>();
        AllowedToGainPoints = false;
        gameOverScreen.gameObject.SetActive(false);
        StartCoroutine(StartUpWatcher());
        LevelLoaded();
    }

    void StartTimer()
    {
        secondsElapsed = 0;
        countdown = StartCoroutine(FinalCountdown());
        if (CanvasManager.CM.pointsManager == null)
        {
            Debug.LogWarning("No points manager assigned to canvas manager script.", CanvasManager.CM.gameObject);
        }
        else
        {
            CanvasManager.CM.pointsManager.gameObject.SetActive(true);
        }
    }

    public void OnRestart()
    {
        hasWon = false;
        gameOverScreen.gameObject.SetActive(false);
        secondsElapsed = 0;
        if (countdown is Coroutine)
        {
            StopCoroutine(countdown);
        }

        countdown = StartCoroutine(FinalCountdown());
    }

    void Update()
    {
        HandleSparksWithTimescaleChanges();
    }

    /// <summary>
    /// Animate timer so player actually looks at it
    /// </summary>
    void HandleSparksWithTimescaleChanges()
    {
        perSecondLerp += Time.deltaTime;
        countdownSlider.value = Mathf.Lerp(timerHandleOld, timerHandleGoal, perSecondLerp);
        if (countdownStageCurrent == 4) //final 30 sec
        {
            sliderFill.color = Color.Lerp(sliderFill.color, timerGoalColor, perSecondLerp / 2);
        }

        //The sparks highlight when the time is going down. Only show when relevant
        if (timerSparks != null && Time.timeScale != priorTimeScale)
        {
            Time.timeScale == 0 ? timerSparks.Stop() : timerSparks.Play();
            priorTimeScale = Time.timeScale;
        }
    }

    public void AddTimeToTimer(int timeToAdd)
    {
        completionTime += secondsElapsed;
        secondsElapsed = Mathf.Clamp(secondsElapsed, 0, Mathf.Max(0, secondsElapsed - timeToAdd));
    }

    public void SetWin()
    {
        hasWon = true;
    }

    /// <summary>
    /// When the level loads, show the level objectives until the player acknowledges them
    /// </summary>
    IEnumerator StartUpWatcher()
    {
        objectiveNav.text = "";
        CanvasManager.CM.tutorialManager.HideTheUI();
        objectiveScreen.PlayStartScreenAnim();
        CanvasManager.CM.player.PreventInput = true;
        yield return oneSecond;
        yield return oneSecond;

        objectiveNav.text = "Press " + (CanvasManager.CM.UsingController ? "(A)" : "[Space]") + " to begin.";
        while (!CanvasManager.CM.player.AcceptButtonClicked)
        {
            yield return null;
        }

        if (ObjectiveScreenHidden != null)
        {
            ObjectiveScreenHidden();
        }
        objectiveScreen.StopStartAnim();
        CanvasManager.CM.player.PreventInput = false;
        if (isGameLevel)
        {
            CanvasManager.CM.tutorialManager.ShowAllUI();
        }
        StartTimer();
    }

    /// <summary>
    /// Coroutine that handles the play time of a given level
    /// Pro tip don't run your game using a coroutine because any errors will kill it
    /// </summary>
    IEnumerator FinalCountdown()
    {
        completionTime = 0;
        CanvasManager.CM.pointsManager.scoreUIText.transform.parent.gameObject.SetActive(false);
        countdownSlider.gameObject.SetActive(false);
        CanvasManager.CM.PreventAllPlayerInput = true;
        CanvasManager.CM.player.PreventAttacks = true;
        CanvasManager.CM.player.InCutscene = true;

        if (!Application.isEditor && isGameLevel)
        {
            useCountdown = true;
        }
        if (useCountdown)
        {
            yield return new WaitForSeconds(startCutsceneLength);
        }
        if (CountdownCompleted != null)
        {
            CountdownCompleted();
        }

        CanvasManager.CM.player.InCutscene = false;
        CanvasManager.CM.PreventAllPlayerInput = false;
        CanvasManager.CM.player.PreventAttacks = false;
        GameSceneManager.allowedToPause = true;

        if (CutsceneEnded is TimedEvent)
        {
            CutsceneEnded();
        }
        AllowedToGainPoints = true;
        if (isGameLevel)
        {
            CanvasManager.CM.pointsManager.scoreUIText.transform.parent.gameObject.SetActive(true);
        }

        //Set up timer UI variables
        sliderFill.color = Color.white;
        countdownSlider.maxValue = maxSeconds;
        if (isGameLevel)
        {
            countdownSlider.gameObject.SetActive(true);
        }
        countdownStageCurrent = 1;
        countdownStagePrior = 1;
        int quarter = maxSeconds / 4;

        //manage game
        while (!hasWon && secondsElapsed < maxSeconds)
        {
            AnimateTimerBasedOnGameProgress();

            secondsElapsed++;
            timerHandleOld = timerHandleGoal;
            timerHandleGoal = maxSeconds - secondsElapsed;
            perSecondLerp = 0;
            yield return oneSecond;
        }

        if (timerSparks != null)
        {
            timerSparks.Stop();
        }

        GameSceneManager.allowedToPause = false;
        GameResolved();
        AllowedToGainPoints = false;
        CanvasManager.CM.player.PreventInput = true;
        CanvasManager.CM.movesetUI.transform.parent.gameObject.SetActive(false);
        CutsceneStarted();

        //if they player wins, the character stops possessing a body and runs away
        //if they lose, they fall over dead
        if (hasWon)
        {
            CanvasManager.CM.player.HandleBodyExit();
            CanvasManager.CM.player.InCutscene = true;
        }
        else
        {
            CanvasManager.CM.player.DiedInCutscene();
        }

        if (!GameSceneManager.GameOverScreenShowing)
        {
            GameSceneManager.GameOverScreenShowing = true;
        }

        //after stopping inputs, wait a second before showing the end screen for a stronger effect on the player
        yield return oneSecond;

        CanvasManager.CM.deathScreen.HideScreen();

        //show game over screen
        gameOverScreen.gameObject.SetActive(true);
        if (hasWon)
        {
            gameOverScreen.AddHighScore(false, CanvasManager.CM.pointsManager.TotalPoints, secondsToMinutes(completionTime));
        }
        else
        {
            gameOverScreen.AddHighScore(true, CanvasManager.CM.pointsManager.TotalPoints, "");
        }
        CanvasManager.CM.inGameUIListener.ShowGameOverScreen();
    }

    void AnimateTimerBasedOnGameProgress()
    {
        //Timer slider changes color based on what quarter of time they player is at. Listed most to least time remaining.
        //blue
        //white
        //yellow
        //red + white flashing


        if (timerSparks != null && timerPulse is ParticleSystem)
        {
            sparksMain = timerSparks.main;
            if (secondsElapsed > quarter * 3)
            {
                countdownStageCurrent = 4;
                sparksMain.startColor = Color.red;
                if (timerGoalColor == Color.white)
                {
                    timerGoalColor = Color.red;
                    timerPulse.Emit(1);
                }
                else
                {
                    timerGoalColor = Color.white;
                    timerPulse.Emit(1);
                }
            }
            else if (secondsElapsed > quarter * 2)
            {
                countdownStageCurrent = 3;
                sliderFill.color = Color.yellow;
                sparksMain.startColor = Color.yellow;

            }
            else if (secondsElapsed > quarter * 1)
            {
                countdownStageCurrent = 2;
                sliderFill.color = Color.white;
                sparksMain.startColor = Color.white;
            }
            else
            {
                countdownStageCurrent = 1;
                sliderFill.color = Color.cyan;
                sparksMain.startColor = Color.cyan;
            }

            if (countdownStageCurrent > countdownStagePrior)
            {
                if (timerPulse is ParticleSystem)
                {
                    timerPulse.loop = false;
                    timerPulse.Play();
                }
                countdownStagePrior = countdownStageCurrent;
            }
        }
    }

    string secondsToMinutes(int sec)
    {
        return Mathf.Floor(sec / 60) + ":" + (sec % 60 < 10 ? "0" : "") + (sec % 60);
    }
}
