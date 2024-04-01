using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

internal class GlazierButton_uGUI : GlazierElementBase_uGUI, ISleekButton, ISleekElement, ISleekLabel, ISleekWithTooltip
{
    public class ButtonPoolData : PoolData
    {
        public Image imageComponent;

        public ButtonEx buttonComponent;

        public TextMeshProUGUI textComponent;
    }

    private GlazieruGUITooltip tooltipComponent;

    private FontStyle _fontStyle;

    private TextAnchor _fontAlignment;

    private ESleekFontSize _fontSize;

    private ETextContrastContext _contrastContext;

    private SleekColor _textColor = GlazierConst.DefaultButtonForegroundColor;

    private SleekColor _backgroundColor = GlazierConst.DefaultButtonBackgroundColor;

    private Image imageComponent;

    private ButtonEx buttonComponent;

    private TextMeshProUGUI textComponent;

    public string Text
    {
        get
        {
            return textComponent.text;
        }
        set
        {
            textComponent.text = value;
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
                tooltipComponent.color = _textColor;
            }
            tooltipComponent.text = value;
        }
    }

    public FontStyle FontStyle
    {
        get
        {
            return _fontStyle;
        }
        set
        {
            _fontStyle = value;
            textComponent.fontStyle = GlazierUtils_uGUI.GetFontStyleFlags(_fontStyle);
        }
    }

    public TextAnchor TextAlignment
    {
        get
        {
            return _fontAlignment;
        }
        set
        {
            _fontAlignment = value;
            textComponent.alignment = GlazierUtils_uGUI.TextAnchorToTMP(_fontAlignment);
        }
    }

    public ESleekFontSize FontSize
    {
        get
        {
            return _fontSize;
        }
        set
        {
            _fontSize = value;
            textComponent.fontSize = GlazierUtils_uGUI.GetFontSize(_fontSize);
        }
    }

    public ETextContrastContext TextContrastContext
    {
        get
        {
            return _contrastContext;
        }
        set
        {
            _contrastContext = value;
            ETextContrastStyle shadowStyle = SleekShadowStyle.ContextToStyle(value);
            textComponent.fontSharedMaterial = base.glazier.GetFontMaterial(shadowStyle);
            textComponent.characterSpacing = GlazierUtils_uGUI.GetCharacterSpacing(shadowStyle);
        }
    }

    public bool IsClickable
    {
        get
        {
            return buttonComponent.interactable;
        }
        set
        {
            buttonComponent.interactable = value;
            SynchronizeColors();
        }
    }

    public bool IsRaycastTarget
    {
        get
        {
            return imageComponent.raycastTarget;
        }
        set
        {
            imageComponent.raycastTarget = value;
        }
    }

    public SleekColor TextColor
    {
        get
        {
            return _textColor;
        }
        set
        {
            _textColor = value;
            textComponent.color = _textColor;
            if (tooltipComponent != null)
            {
                tooltipComponent.color = _textColor;
            }
        }
    }

    public bool AllowRichText
    {
        get
        {
            return textComponent.richText;
        }
        set
        {
            textComponent.richText = value;
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
            imageComponent.color = _backgroundColor;
        }
    }

    public event ClickedButton OnClicked;

    public event ClickedButton OnRightClicked;

    private void PostConstructButton()
    {
        TextAlignment = TextAnchor.MiddleCenter;
        FontSize = ESleekFontSize.Default;
        TextContrastContext = ETextContrastContext.Default;
        FontStyle = FontStyle.Normal;
        AllowRichText = false;
    }

    protected override bool ReleaseIntoPool()
    {
        if (imageComponent == null || buttonComponent == null || textComponent == null)
        {
            return false;
        }
        if (tooltipComponent != null)
        {
            Object.Destroy(tooltipComponent);
            tooltipComponent = null;
        }
        imageComponent.enabled = false;
        buttonComponent.enabled = false;
        textComponent.enabled = false;
        ButtonPoolData buttonPoolData = new ButtonPoolData();
        PopulateBasePoolData(buttonPoolData);
        buttonPoolData.imageComponent = imageComponent;
        imageComponent = null;
        buttonPoolData.buttonComponent = buttonComponent;
        buttonComponent = null;
        buttonPoolData.textComponent = textComponent;
        textComponent = null;
        base.glazier.ReleaseButtonToPool(buttonPoolData);
        return true;
    }

    protected override void EnableComponents()
    {
        imageComponent.enabled = true;
        buttonComponent.enabled = true;
        textComponent.enabled = true;
    }

    public GlazierButton_uGUI(Glazier_uGUI glazier)
        : base(glazier)
    {
    }

    public override void ConstructNew()
    {
        base.ConstructNew();
        imageComponent = base.gameObject.AddComponent<Image>();
        imageComponent.enabled = false;
        imageComponent.type = Image.Type.Sliced;
        buttonComponent = base.gameObject.AddComponent<ButtonEx>();
        buttonComponent.enabled = false;
        buttonComponent.transition = Selectable.Transition.SpriteSwap;
        buttonComponent.onClick.AddListener(OnUnityButtonClicked);
        buttonComponent.onRightClick.AddListener(OnUnityButtonRightClicked);
        GameObject gameObject = new GameObject("ButtonText", typeof(RectTransform));
        gameObject.transform.SetParent(base.transform, worldPositionStays: false);
        gameObject.GetRectTransform().reset();
        textComponent = gameObject.AddComponent<TextMeshProUGUI>();
        textComponent.enabled = false;
        textComponent.raycastTarget = false;
        textComponent.font = GlazierResources_uGUI.Font;
        textComponent.overflowMode = TextOverflowModes.Truncate;
        textComponent.margin = GlazierConst_uGUI.DefaultTextMargin;
        textComponent.extraPadding = true;
        PostConstructButton();
    }

    public void ConstructFromButtonPool(ButtonPoolData poolData)
    {
        ConstructFromPool(poolData);
        imageComponent = poolData.imageComponent;
        buttonComponent = poolData.buttonComponent;
        textComponent = poolData.textComponent;
        imageComponent.raycastTarget = true;
        textComponent.text = string.Empty;
        textComponent.rectTransform.reset();
        buttonComponent.interactable = true;
        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onRightClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(OnUnityButtonClicked);
        buttonComponent.onRightClick.AddListener(OnUnityButtonRightClicked);
        PostConstructButton();
    }

    public override void SynchronizeColors()
    {
        Color color = _backgroundColor;
        if (!IsClickable)
        {
            color.a *= 0.25f;
        }
        imageComponent.color = color;
        textComponent.color = TextColor;
        if (tooltipComponent != null)
        {
            tooltipComponent.color = _textColor;
        }
    }

    public override void SynchronizeTheme()
    {
        imageComponent.sprite = GlazierResources_uGUI.Theme.BoxSprite;
        SpriteState spriteState = default(SpriteState);
        spriteState.disabledSprite = imageComponent.sprite;
        spriteState.highlightedSprite = GlazierResources_uGUI.Theme.BoxHighlightedSprite;
        spriteState.selectedSprite = GlazierResources_uGUI.Theme.BoxSelectedSprite;
        spriteState.pressedSprite = GlazierResources_uGUI.Theme.BoxPressedSprite;
        buttonComponent.spriteState = spriteState;
    }

    private void OnUnityButtonClicked()
    {
        this.OnClicked?.Invoke(this);
    }

    private void OnUnityButtonRightClicked()
    {
        this.OnRightClicked?.Invoke(this);
    }
}
