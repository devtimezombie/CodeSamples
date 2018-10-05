using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    public bool DeathScreenIsVisible { get; private set; }
    public bool PreventDeathScreenFromShowing { get; set; }
    [SerializeField]
    GameObject gameOverBase;
    [SerializeField]
    Image screen;
    public Text youDiedText, continueText;
    Color youDiedTextColor;
    float uDiedTextColorInc;
    //string uDied;
    WaitForSeconds oneSec = new WaitForSeconds(0.8f);
    Coroutine showPopup;

    void Start()
    {
        //uDied = youDiedText.text;
        gameOverBase.SetActive(true);
        youDiedTextColor = youDiedText.color;
        screen.gameObject.SetActive(false);
    }

    //request death notification on death. 
    public void ShowScreen()
    {
        if (PreventDeathScreenFromShowing)
            return;

        DeathScreenIsVisible = true;
        screen.gameObject.SetActive(true);
        if (showPopup is Coroutine)
        {
            StopCoroutine(showPopup);
        }

        showPopup = StartCoroutine(revealDelay());
    }

    //hide when the player acknowledges it
    public void HideScreen()
    {
        DeathScreenIsVisible = false;
        screen.gameObject.SetActive(false);
    }

    //like in dark souls, show a banner, show "YOU DIED" and give the player the input to continue once they've seen the message
    IEnumerator revealDelay()
    {
        CanvasManager.CM.PreventUIControls = true;
        screen.fillAmount = 0;
        uDiedTextColorInc = 0;
        youDiedText.color = Color.clear;
        continueText.text = "";

        while (screen.fillAmount < 1)
        {
            screen.fillAmount += Time.deltaTime * 1.25f;
            if (screen.fillAmount > 0.5f)
            {
                uDiedTextColorInc += Time.deltaTime * 2.25f;
                youDiedText.color = Color.Lerp(Color.clear, youDiedTextColor, uDiedTextColorInc);
            }
            yield return null;
        }

        CanvasManager.CM.PreventUIControls = false;
        yield return oneSec;
        continueText.text = "Press " + (CanvasManager.CM.UsingController ? "(A)" : "[Enter]") + " to respawn.";
    }

    //disallow death notifications after the game is over
    public void StopDeathScreenOnGameResolution()
    {
        if (showPopup != null)
        {
            StopCoroutine(showPopup);
        }
        screen.gameObject.SetActive(false);
        PreventDeathScreenFromShowing = true;
        CanvasManager.CM.PreventUIControls = false;
    }
}
