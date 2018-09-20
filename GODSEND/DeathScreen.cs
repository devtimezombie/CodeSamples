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
    public void ShowScreen()
    {
        if (PreventDeathScreenFromShowing)
            return;

        DeathScreenIsVisible = true;
        screen.gameObject.SetActive(true);
        if (showPopup is Coroutine)
            StopCoroutine(showPopup);

        showPopup = StartCoroutine(revealDelay());
    }

    public void HideScreen()
    {
        DeathScreenIsVisible = false;
        screen.gameObject.SetActive(false);
    }

    IEnumerator revealDelay()
    {
        //CanvasManager.CM.player.PreventAttacks=true;
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
        //yield return oneSec;
        //youDiedText.text = uDied;
        CanvasManager.CM.PreventUIControls = false;
        yield return oneSec;
        continueText.text = "Press " + (CanvasManager.CM.UsingController ? "(A)" : "[Enter]") + " to respawn.";
        //CanvasManager.CM.player.PreventAttacks = false;
    }

    public void StopDeathScreenOnGameResolution()
    {
        if (showPopup != null)
            StopCoroutine(showPopup);
        screen.gameObject.SetActive(false);
        PreventDeathScreenFromShowing = true;
        CanvasManager.CM.PreventUIControls = false;
    }
}
