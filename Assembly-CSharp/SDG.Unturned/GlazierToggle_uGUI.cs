using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

internal class GlazierToggle_uGUI : GlazierElementBase_uGUI, ISleekToggle, ISleekElement, ISleekWithTooltip
{
    private GlazieruGUITooltip tooltipComponent;

    private SleekColor _backgroundColor;

    private SleekColor _foregroundColor;

    private Image backgroundImageComponent;

    private Image foregroundImageComponent;

    private Toggle toggleComponent;

    public bool Value
    {
        get
        {
            return toggleComponent.isOn;
        }
        set
        {
            toggleComponent.SetIsOnWithoutNotify(value);
        }
    }

    public string TooltipText
    {
        get
        {
            if (!(tooltipComponent != null))
            {
                return null;
            }
            return tooltipComponent.text;
        }
        set
        {
            if (tooltipComponent == null)
            {
                tooltipComponent = base.gameObject.AddComponent<GlazieruGUITooltip>();
                tooltipComponent.color = new SleekColor(ESleekTint.FONT);
            }
            tooltipComponent.text = value;
        }
    }

    public SleekColor BackgroundColor
    {
        get
        {
            return _backgroundColor;
        }
        set
        {
            _backgroundColor = value;
            backgroundImageComponent.color = _backgroundColor;
        }
    }

    public SleekColor ForegroundColor
    {
        get
        {
            return _foregroundColor;
        }
        set
        {
            _foregroundColor = value;
            foregroundImageComponent.color = _foregroundColor;
        }
    }

    public bool IsInteractable
    {
        get
        {
            return toggleComponent.interactable;
        }
        set
        {
            toggleComponent.interactable = value;
            SynchronizeColors();
        }
    }

    public event Toggled OnValueChanged;

    public GlazierToggle_uGUI(Glazier_uGUI glazier)
        : base(glazier)
    {
    }

    public override void ConstructNew()
    {
        base.ConstructNew();
        base.SizeOffset_X = 40f;
        base.SizeOffset_Y = 40f;
        GameObject gameObject = new GameObject("Background", typeof(RectTransform));
        gameObject.transform.SetParent(base.transform, worldPositionStays: false);
        RectTransform rectTransform = gameObject.GetRectTransform();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(20f, 20f);
        backgroundImageComponent = gameObject.AddComponent<Image>();
        backgroundImageComponent.enabled = false;
        backgroundImageComponent.raycastTarget = true;
        GameObject gameObject2 = new GameObject("Foreground", typeof(RectTransform));
        gameObject2.transform.SetParent(base.transform, worldPositionStays: false);
        gameObject2.GetRectTransform().reset();
        foregroundImageComponent = gameObject2.AddComponent<Image>();
        foregroundImageComponent.enabled = false;
        foregroundImageComponent.raycastTarget = false;
        toggleComponent = base.gameObject.AddComponent<Toggle>();
        toggleComponent.enabled = false;
        toggleComponent.transition = Selectable.Transition.SpriteSwap;
        toggleComponent.targetGraphic = backgroundImageComponent;
        toggleComponent.graphic = foregroundImageComponent;
        toggleComponent.onValueChanged.AddListener(uGUIonValueChanged);
        _backgroundColor = GlazierConst.DefaultToggleBackgroundColor;
        _foregroundColor = GlazierConst.DefaultToggleForegroundColor;
    }

    public override void SynchronizeColors()
    {
        Color color = _backgroundColor;
        Color color2 = _foregroundColor;
        if (!IsInteractable)
        {
            color.a *= 0.25f;
            color2.a *= 0.25f;
        }
        backgroundImageComponent.color = color;
        foregroundImageComponent.color = color2;
        if (tooltipComponent != null)
        {
            tooltipComponent.color = new SleekColor(ESleekTint.FONT);
        }
    }

    public override void SynchronizeTheme()
    {
        backgroundImageComponent.sprite = GlazierResources_uGUI.Theme.BoxSprite;
        foregroundImageComponent.sprite = GlazierResources_uGUI.Theme.ToggleForegroundSprite;
        SpriteState spriteState = default(SpriteState);
        spriteState.highlightedSprite = GlazierResources_uGUI.Theme.BoxHighlightedSprite;
        spriteState.selectedSprite = GlazierResources_uGUI.Theme.BoxSelectedSprite;
        spriteState.disabledSprite = backgroundImageComponent.sprite;
        spriteState.pressedSprite = GlazierResources_uGUI.Theme.BoxPressedSprite;
        toggleComponent.spriteState = spriteState;
    }

    protected override void EnableComponents()
    {
        backgroundImageComponent.enabled = true;
        foregroundImageComponent.enabled = true;
        toggleComponent.enabled = true;
    }

    private void uGUIonValueChanged(bool isOn)
    {
        this.OnValueChanged?.Invoke(this, isOn);
    }
}
