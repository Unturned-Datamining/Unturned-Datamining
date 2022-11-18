using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SDG.Unturned;

internal class GlazierStringField_uGUI : GlazierElementBase_uGUI, ISleekField, ISleekElement, ISleekLabel, ISleekWithTooltip
{
    private char _replace = ' ';

    private GlazieruGUITooltip tooltipComponent;

    private FontStyle _fontStyle;

    private TextAnchor _fontAlignment;

    private ESleekFontSize _fontSize;

    private ETextContrastContext _contrastContext;

    private SleekColor _textColor;

    private SleekColor _backgroundColor;

    protected TMP_InputField fieldComponent;

    private Image imageComponent;

    private TextMeshProUGUI placeholderComponent;

    private TextMeshProUGUI textComponent;

    public char replace
    {
        get
        {
            return _replace;
        }
        set
        {
            if (_replace != value)
            {
                _replace = value;
                if (_replace == ' ')
                {
                    fieldComponent.contentType = TMP_InputField.ContentType.Standard;
                }
                else
                {
                    fieldComponent.contentType = TMP_InputField.ContentType.Password;
                }
                fieldComponent.asteriskChar = _replace;
                fieldComponent.ForceLabelUpdate();
            }
        }
    }

    public string hint
    {
        get
        {
            return placeholderComponent.text;
        }
        set
        {
            placeholderComponent.text = value;
        }
    }

    public bool multiline
    {
        get
        {
            return fieldComponent.lineType != TMP_InputField.LineType.SingleLine;
        }
        set
        {
            fieldComponent.lineType = (value ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine);
            fieldComponent.lineLimit = ((!value) ? 1 : 0);
        }
    }

    public string text
    {
        get
        {
            return fieldComponent.text;
        }
        set
        {
            fieldComponent.SetTextWithoutNotify(value);
        }
    }

    public string tooltipText
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

    public FontStyle fontStyle
    {
        get
        {
            return _fontStyle;
        }
        set
        {
            _fontStyle = value;
            textComponent.fontStyle = GlazierUtils_uGUI.GetFontStyleFlags(_fontStyle);
            placeholderComponent.fontStyle = textComponent.fontStyle;
        }
    }

    public TextAnchor fontAlignment
    {
        get
        {
            return _fontAlignment;
        }
        set
        {
            _fontAlignment = value;
            textComponent.alignment = GlazierUtils_uGUI.TextAnchorToTMP(_fontAlignment);
            placeholderComponent.alignment = textComponent.alignment;
        }
    }

    public ESleekFontSize fontSize
    {
        get
        {
            return _fontSize;
        }
        set
        {
            _fontSize = value;
            textComponent.fontSize = GlazierUtils_uGUI.GetFontSize(_fontSize);
            placeholderComponent.fontSize = GlazierUtils_uGUI.GetFontSize(_fontSize);
        }
    }

    public ETextContrastContext shadowStyle
    {
        get
        {
            return _contrastContext;
        }
        set
        {
            _contrastContext = value;
            ETextContrastStyle eTextContrastStyle = SleekShadowStyle.ContextToStyle(value);
            textComponent.fontSharedMaterial = base.glazier.GetFontMaterial(eTextContrastStyle);
            textComponent.characterSpacing = GlazierUtils_uGUI.GetCharacterSpacing(eTextContrastStyle);
            placeholderComponent.fontSharedMaterial = textComponent.fontSharedMaterial;
            placeholderComponent.characterSpacing = textComponent.characterSpacing;
        }
    }

    public SleekColor textColor
    {
        get
        {
            return _textColor;
        }
        set
        {
            _textColor = value;
            placeholderComponent.color = _textColor.Get() * 0.5f;
            textComponent.color = _textColor;
            if (tooltipComponent != null)
            {
                tooltipComponent.color = _textColor;
            }
        }
    }

    public bool enableRichText
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public SleekColor backgroundColor
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

    public int maxLength
    {
        get
        {
            return fieldComponent.characterLimit;
        }
        set
        {
            fieldComponent.characterLimit = value;
        }
    }

    public event Entered onEntered;

    public event Typed onTyped;

    public event Escaped onEscaped;

    public void FocusControl()
    {
        fieldComponent.ActivateInputField();
    }

    public void ClearFocus()
    {
        fieldComponent.DeactivateInputField();
    }

    public GlazierStringField_uGUI(Glazier_uGUI glazier)
        : base(glazier)
    {
    }

    public override void ConstructNew()
    {
        base.ConstructNew();
        imageComponent = base.gameObject.AddComponent<Image>();
        imageComponent.enabled = false;
        imageComponent.type = Image.Type.Sliced;
        imageComponent.raycastTarget = true;
        GameObject obj = new GameObject("Viewport", typeof(RectTransform));
        obj.transform.SetParent(base.transform, worldPositionStays: false);
        RectTransform rectTransform = obj.GetRectTransform();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = new Vector2(2f, 2f);
        rectTransform.offsetMax = new Vector2(-2f, -2f);
        obj.AddComponent<RectMask2D>();
        GameObject gameObject = new GameObject("Placeholder", typeof(RectTransform));
        gameObject.transform.SetParent(rectTransform, worldPositionStays: false);
        gameObject.GetRectTransform().reset();
        placeholderComponent = gameObject.AddComponent<TextMeshProUGUI>();
        placeholderComponent.enabled = false;
        placeholderComponent.raycastTarget = false;
        placeholderComponent.font = GlazierResources_uGUI.Font;
        placeholderComponent.margin = GlazierConst_uGUI.DefaultTextMargin;
        placeholderComponent.extraPadding = true;
        placeholderComponent.richText = false;
        GameObject gameObject2 = new GameObject("Text", typeof(RectTransform));
        gameObject2.transform.SetParent(rectTransform, worldPositionStays: false);
        gameObject2.GetRectTransform().reset();
        textComponent = gameObject2.AddComponent<TextMeshProUGUI>();
        textComponent.enabled = false;
        textComponent.raycastTarget = false;
        textComponent.font = GlazierResources_uGUI.Font;
        textComponent.margin = GlazierConst_uGUI.DefaultTextMargin;
        textComponent.extraPadding = true;
        textComponent.richText = false;
        fieldComponent = base.gameObject.AddComponent<TMP_InputField>();
        fieldComponent.enabled = false;
        fieldComponent.textViewport = rectTransform;
        fieldComponent.textComponent = textComponent;
        fieldComponent.placeholder = placeholderComponent;
        fieldComponent.transition = Selectable.Transition.SpriteSwap;
        fieldComponent.onSubmit.AddListener(OnUnitySubmit);
        fieldComponent.onValueChanged.AddListener(OnUnityValueChanged);
        fieldComponent.caretWidth = 2;
        fieldComponent.customCaretColor = true;
        fieldComponent.isRichTextEditingAllowed = false;
        fieldComponent.richText = false;
        _backgroundColor = GlazierConst.DefaultFieldBackgroundColor;
        _textColor = GlazierConst.DefaultFieldForegroundColor;
        shadowStyle = ETextContrastContext.Default;
        fontStyle = FontStyle.Normal;
        fontAlignment = TextAnchor.MiddleCenter;
        fontSize = ESleekFontSize.Default;
        maxLength = 100;
        multiline = false;
    }

    public override void SynchronizeColors()
    {
        imageComponent.color = _backgroundColor;
        placeholderComponent.color = _textColor.Get() * 0.5f;
        textComponent.color = _textColor;
        if (tooltipComponent != null)
        {
            tooltipComponent.color = _textColor;
        }
        fieldComponent.caretColor = OptionsSettings.foregroundColor;
        Color caretColor = fieldComponent.caretColor;
        caretColor.a = 0.5f;
        fieldComponent.selectionColor = caretColor;
    }

    public override void SynchronizeTheme()
    {
        imageComponent.sprite = GlazierResources_uGUI.Theme.BoxSprite;
        SpriteState spriteState = default(SpriteState);
        spriteState.disabledSprite = imageComponent.sprite;
        spriteState.highlightedSprite = GlazierResources_uGUI.Theme.BoxHighlightedSprite;
        spriteState.pressedSprite = GlazierResources_uGUI.Theme.BoxHighlightedSprite;
        spriteState.selectedSprite = GlazierResources_uGUI.Theme.BoxSelectedSprite;
        fieldComponent.spriteState = spriteState;
    }

    protected override void EnableComponents()
    {
        imageComponent.enabled = true;
        placeholderComponent.enabled = true;
        textComponent.enabled = true;
        fieldComponent.enabled = true;
    }

    protected virtual void OnUnitySubmit(string input)
    {
        this.onEntered?.Invoke(this);
    }

    protected virtual void OnUnityValueChanged(string input)
    {
        this.onTyped?.Invoke(this, input);
    }
}
