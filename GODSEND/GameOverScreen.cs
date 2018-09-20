using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    public Text LEVELText, endStateText, highScoreText, finishTime, hintText;
    public Color textColor;
    public Image sideBar;
    public GameObject[] objectsToShow;
    public string scorePrefix = "Score:", scoreSuffix = "Points";
    public string timePrefix = "Completed in:";
    public string endStateWin = "";
    public string endStateLose = "";

    public GameObject NextLevelButton;
    float colorChangeMultiplier = 1.5f;

    WaitForSeconds delay = new WaitForSeconds(0.3f);
    WaitForSeconds colorLerpDelay = new WaitForSeconds(0.01f);

    private void Start()
    {
        foreach (GameObject g in objectsToShow)
            g.SetActive(false);

    }

    public void AddHighScore(bool endStateIsLoss, float score, string time)
    {
        if (endStateIsLoss)
        {
            endStateText.text = "FAILED";
            hintText.text = "You can win next time!";
        }
        else
        {
            endStateText.text = "COMPLETED";
            hintText.text = "Thanks for playing the demo!";
        }
        StartCoroutine(SlowFadeIn(endStateIsLoss, score.ToString(), time));
    }

    IEnumerator SlowFadeIn(bool endStateIsLoss, string score, string time)
    {
        LEVELText.color = Color.clear;
        endStateText.color = Color.clear;
        finishTime.color = Color.clear;
        highScoreText.color = Color.clear;

        sideBar.rectTransform.localPosition = new Vector3(0, -100, 0);

        while (!gameObject.activeInHierarchy)
            yield return null;

        while (sideBar.rectTransform.localPosition.y < -50)
        {
            sideBar.rectTransform.localPosition += Vector3.up * 2;
            yield return null;
        }

        float f = 0;
        while (LEVELText.color.a < 1)
        {
            LEVELText.color = Color.Lerp(Color.clear, textColor, f);
            f += Time.unscaledDeltaTime * colorChangeMultiplier;
            yield return colorLerpDelay;
        }

        f = 0;
        while (endStateText.color.a < 1)
        {
            endStateText.color = Color.Lerp(Color.clear, textColor, f);
            f += Time.unscaledDeltaTime * colorChangeMultiplier;
            yield return colorLerpDelay;
        }


        finishTime.text = endStateIsLoss ? "" : timePrefix + " " + time.ToString();

        if (finishTime.gameObject.activeInHierarchy && finishTime.text != "")
        {
            //yield return delay;
            finishTime.text = timePrefix + " " + time.ToString();
            f = 0;
            while (finishTime.color.a < 1)
            {
                finishTime.color = Color.Lerp(Color.clear, textColor, f);
                f += Time.unscaledDeltaTime * colorChangeMultiplier;
                yield return colorLerpDelay;
            }
        }

        if (highScoreText.gameObject.activeInHierarchy && highScoreText.text != "")
        {
            //yield return delay;
            highScoreText.text = scorePrefix + " " + score + " " + scoreSuffix;
            f = 0;
            while (highScoreText.color.a < 1)
            {
                highScoreText.color = Color.Lerp(Color.clear, textColor, f);
                f += Time.unscaledDeltaTime * colorChangeMultiplier;
                yield return colorLerpDelay;
            }
        }

        if (NextLevelButton != null)
        {
            yield return delay;
            NextLevelButton.gameObject.SetActive(false);
        }

        yield return delay;
        foreach (GameObject g in objectsToShow)
        {
            g.SetActive(true);
            yield return delay;
        }

    }
}
