using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualizeViaCanvas : VisualizeVia
{
    public Text Cursor, Narration;
    public TextNarration narrationObject;
    public float itemSizeMultipler = 0.5f;
    
    Color narrationColor;

    public float TextAnimationSpeed = 0.05f;
    float n, showForSeconds = 2;
    ColorChangeState colorState = ColorChangeState.hide;
    enum ColorChangeState
    {
        show,
        sustain,
        hide
    }

    private void Start()
    {
        if (Cursor == null) Debug.LogError("UI text not assigned");
        if (Narration == null)
            Debug.LogError("UI text not assigned");
        else
            narrationColor = Narration.color;
    }

    public override void VisualizeInteraction(ObjectInteractions action)
    {
        base.VisualizeInteraction(action);
        switch (action)
        {
            case (ObjectInteractions.leave):
                Cursor.text = "o";
                break;
            case (ObjectInteractions.look):
                Cursor.text = "o";
                break;
            case (ObjectInteractions.take):
                Cursor.text = "x";
                break;
            case (ObjectInteractions.use):
                Cursor.text = "^";
                break;
            case (ObjectInteractions.drag):
                Cursor.text = "#";
                break;
            case (ObjectInteractions.UI):
                Cursor.text = "I";
                break;
            default:
                Cursor.text = "+";
                break;
        }
    }

    public override void AddItemToHand(InteractionObject item)
    {
        base.AddItemToHand(item);
        item.ShrinkItem(itemSizeMultipler);
    }

    public override void NarrateInteraction(bool succeeded)
    {
        Narration.color = Color.clear;
        colorState = ColorChangeState.show;
        if (succeeded)
            Narration.text = narrationObject.RandomTextSuccess();
        else
            Narration.text = narrationObject.RandomTextFail();
    }

    private void Update()
    {
        if (colorState == ColorChangeState.show)
        {
            n += TextAnimationSpeed;
            Narration.color = Color.Lerp(Narration.color, narrationColor, n);
            if (Narration.color.a == 1)
            {
                colorState = ColorChangeState.sustain;
                n = Time.time;
            }
        }
        else if (colorState == ColorChangeState.sustain)
        {
            if (Time.time > n + showForSeconds)
            {
                colorState = ColorChangeState.hide;
                n = 0;
            }
        }
        else if (colorState == ColorChangeState.hide)
        {
            if (Narration.color.a > 0)
            {
                n += TextAnimationSpeed;
                Narration.color = Color.Lerp(Narration.color, Color.clear, n);
            }
        }
    }
}

[System.Serializable]
[CreateAssetMenu(fileName = "NarrationAsset", menuName = "Scriptable Objects/NarrationAsset")]
public class TextNarration : ScriptableObject
{
    public string[] successText;
    public string[] failureText;

    public string RandomTextFail()
    {
        return failureText[Random.Range((int)0, failureText.Length)];
    }

    public string RandomTextSuccess()
    {
        return successText[Random.Range((int)0, successText.Length)];
    }
}