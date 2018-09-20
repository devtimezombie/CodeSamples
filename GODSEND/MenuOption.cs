using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuOption : MonoBehaviour
{
    public virtual void NavTo() //highlight, via controller
    {
        AkSoundEngine.PostEvent("Select", gameObject);
    }
    public virtual void Click()
    {
        // AkSoundEngine.PostEvent("Accept", gameObject);
    }
    public virtual void NavFrom() { }
    public virtual void OnPointerEnter(PointerEventData eventData) { } //highlight, via mouse
    public virtual void OnPointerExit(PointerEventData eventData) { }
}

/*
 * 
 * child classes added to parent file for demonstration purposes
 * 
 */

//Menu buttons, for navigating menus

public class MenuButton : MenuOption
{
    IMenu sceneMenu;
    Button m_Button;
    UIFontFade m_FontFade;

    public override void NavTo()
    {
        base.NavTo();
        m_FontFade.SetEnter();
    }

    public override void Click()
    {
        base.Click();
        m_Button.onClick.Invoke();
    }

    public override void NavFrom()
    {
        m_FontFade.SetExit();

    }

    public void Awake()
    {
        m_Button = GetComponent<Button>();
        m_FontFade = GetComponent<UIFontFade>();
        sceneMenu = GetComponentInParent<IMenu>();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        sceneMenu.SetMenuButton(this);
        m_FontFade.SetEnter();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        m_FontFade.SetExit();
    }
}

//menu toggles, for turning options on and off

public class MenuToggle : MenuOption
{
    IMenu sceneMenu;
    Toggle m_Toggle;
    [SerializeField]
    GameObject highlightImg;

    public override void NavTo()
    {
        base.NavTo();
        m_Toggle.targetGraphic.color = m_Toggle.colors.highlightedColor;
    }

    public override void Click()
    {
        base.Click();
        m_Toggle.isOn = m_Toggle.isOn ? false : true;
    }

    public override void NavFrom()
    {
        m_Toggle.targetGraphic.color = m_Toggle.colors.normalColor;
    }

    public void Awake()
    {
        m_Toggle = GetComponent<Toggle>();
        sceneMenu = GetComponentInParent<IMenu>();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        sceneMenu.SetMenuButton(this);
        m_Toggle.targetGraphic.color = m_Toggle.colors.highlightedColor;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        m_Toggle.targetGraphic.color = m_Toggle.colors.normalColor;
    }
}
