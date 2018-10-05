using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovesetUI : MonoBehaviour
{
    public Image health, armour;
    public GameObject armorStuff;

    public BodyType abilitySource;
    public MovesetUIIcon AbilityStandard;
    public MovesetUIIcon AbilityDown;

    [Header("Help Overlay")]
    [SerializeField]
    GameObject help_ability1Back;
    [SerializeField]
    GameObject help_ability2Back;
    [SerializeField]
    Text help_ability1, help_ability2;

    [Header("AI notifiers")]
    [SerializeField]
    Sprite tricksterNorm;
    [SerializeField]
    Sprite gruntNorm, hamNorm, hamDown, dashNorm, dashDown, throwNorm, throwDown;

    WaitForSeconds delay = new WaitForSeconds(0.1f);
    WaitForSeconds showHelpDelay = new WaitForSeconds(2);
    Coroutine addAbilityUI, showHelpUI;

    private void Awake()
    {
        SetBody(BodyType.Player);
    }

    private void Start()
    {
        help_ability1Back.SetActive(false);
        help_ability2Back.SetActive(false);
        CanvasManager.CM.StartedUsingController += ChangePromptsToController;
        CanvasManager.CM.StartedUsingKeyboard += ChangePromptsToKeyboard;
    }

    private void OnDestroy()
    {
        CanvasManager.CM.StartedUsingController -= ChangePromptsToController;
        CanvasManager.CM.StartedUsingKeyboard -= ChangePromptsToKeyboard;
    }

    private void OnEnable()
    {
        SetBody(BodyType.Player);
    }

    /// <summary>
    /// automatically switches input hints for controller
    /// </summary>
    void ChangePromptsToController()
    {
        AbilityStandard.InputHintIcon_KB.SetActive(false);
        AbilityDown.InputHintIcon_KB.SetActive(false);

        if (AbilityStandard.baseImage.IsActive())
            AbilityStandard.InputHintIcon_controller.SetActive(true);
        if (AbilityDown.baseImage.IsActive())
            AbilityDown.InputHintIcon_controller.SetActive(true);
    }

    /// <summary>
    /// automatically switches input hints for mouse + keyboard
    /// </summary>
    void ChangePromptsToKeyboard()
    {
        AbilityStandard.InputHintIcon_controller.SetActive(false);
        AbilityDown.InputHintIcon_controller.SetActive(false);

        if (AbilityStandard.baseImage.IsActive())
            AbilityStandard.InputHintIcon_KB.SetActive(true);
        if (AbilityDown.baseImage.IsActive())
            AbilityDown.InputHintIcon_KB.SetActive(true);
    }

    void ShowNormalPrompt()
    {
        if (CanvasManager.CM.UsingController)
            AbilityStandard.InputHintIcon_controller.SetActive(true);
        else
            AbilityStandard.InputHintIcon_KB.SetActive(true);
    }
    void ShowDownPrompt()
    {
        if (CanvasManager.CM.UsingController)
            AbilityDown.InputHintIcon_controller.SetActive(true);
        else
            AbilityDown.InputHintIcon_KB.SetActive(true);
    }

    public void SetBody(BodyType source)
    {
        abilitySource = source;

        if (addAbilityUI is Coroutine)
            StopCoroutine(addAbilityUI);

        if (gameObject.activeInHierarchy)
            addAbilityUI = StartCoroutine(AddAbilities(source));
    }

    //it's come to my attention that it would have been more effective to use a scriptable object reference
    //and then pass that through instead of hardcoding hint text
    //1. that is a great idea.
    //2. i'm still stuck localizing info do this UI component because people kept modifying the character prefabs. I'm lucky I got the character body tag to stick.
    IEnumerator AddAbilities(BodyType source) //show ability icons based on current body. Player has no abilities, grunts have 1, beasts have 2
    {
        help_ability1Back.SetActive(false);
        help_ability2Back.SetActive(false);

        help_ability1.text = "";
        help_ability2.text = "";

        AbilityStandard.SetUIIconInactive();
        AbilityDown.SetUIIconInactive();

        //icons for what keyboard/controller button triggers which ability
        AbilityStandard.InputHintIcon_controller.SetActive(false);
        AbilityDown.InputHintIcon_controller.SetActive(false);
        AbilityStandard.InputHintIcon_KB.SetActive(false);
        AbilityDown.InputHintIcon_KB.SetActive(false);

        //possessed bodies serve as additional health
        SetArmor(false);

        yield return delay;

        //slowly show ability icons and names. Then hide the names so they don't bother anyone
        switch (source)
        {
            case (BodyType.Player):
                ToggleHelp();
                break;
            case (BodyType.Grunt):
                SetArmor(true);
                yield return delay;
                AbilityStandard.SetBodyAbilities(gruntNorm);
                help_ability1.text = "Slap";
                ShowNormalPrompt();
                break;
            case (BodyType.Hammer):
                SetArmor(true);
                yield return delay;
                AbilityStandard.SetBodyAbilities(hamNorm);
                help_ability1.text = "Smash";
                ShowNormalPrompt();
                yield return delay;
                AbilityDown.SetBodyAbilities(hamDown);
                help_ability2.text = "Create Wall";
                ShowDownPrompt();
                break;
            case (BodyType.Dasher):
                SetArmor(true);
                yield return delay;
                AbilityStandard.SetBodyAbilities(dashNorm);
                help_ability1.text = "Stab";
                ShowNormalPrompt();
                yield return delay;
                AbilityDown.SetBodyAbilities(dashDown);
                help_ability2.text = "Dash";
                ShowDownPrompt();
                break;
            case (BodyType.Bomber):
                SetArmor(true);
                yield return delay;
                AbilityStandard.SetBodyAbilities(throwNorm);
                help_ability1.text = "Grab/Throw";
                ShowNormalPrompt();
                yield return delay;
                AbilityDown.SetBodyAbilities(throwDown, false, "X");
                help_ability2.text = "Lay Bombs";
                ShowDownPrompt();
                break;
            default:
                break;
        }
        if (showHelpUI is Coroutine)
            StopCoroutine(showHelpUI);

        showHelpUI = StartCoroutine(showHelpBriefly());
    }

    /// <summary>
    /// When a body is possessed, it serves as extra health. toggle the UI if it's relevant
    /// </summary>
    void SetArmor(bool active)
    {
        if (armorStuff != null)
            armorStuff.SetActive(active);
        if (armour != null)
            armour.fillAmount = 1;
    }

    /// <summary>
    /// show/hide the names of the abilities
    /// coroutines allows some animation offsetting for a pop
    /// </summary>
    public void ToggleHelp()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (showHelpUI is Coroutine)
            StopCoroutine(showHelpUI);

        if (help_ability1Back.activeInHierarchy)
            showHelpUI = StartCoroutine(toggleHelp(false));
        else
            showHelpUI = StartCoroutine(toggleHelp(true));
    }

    IEnumerator toggleHelp(bool uiOn)
    {
        if (abilitySource == BodyType.None)
            yield break;

        if (abilitySource != BodyType.Player)
        {
            yield return delay;
            help_ability1Back.SetActive(uiOn);
        }

        if (abilitySource == BodyType.Hammer || abilitySource == BodyType.Dasher || abilitySource == BodyType.Bomber)
        {
            yield return delay;
            help_ability2Back.SetActive(uiOn);
        }
    }

    /// <summary>
    /// automate turning off and on the names of the abilities
    /// </summary>
    IEnumerator showHelpBriefly()
    {
        yield return toggleHelp(true);
        yield return showHelpDelay;
        yield return toggleHelp(false);
    }
}
