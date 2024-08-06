using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned;

internal class GlazierStringField_UIToolkit : GlazierElementBase_UIToolkit, ISleekField, ISleekElement, ISleekLabel, ISleekWithTooltip
{
    private FontStyle _fontStyle;

    private TextAnchor _fontAlignment = TextAnchor.MiddleCenter;

    private ESleekFontSize _fontSize;

    private ETextContrastContext _contrastContext;

    private SleekColor _textColor = GlazierConst.DefaultFieldForegroundColor;

    private SleekColor _backgroundColor = GlazierConst.DefaultFieldBackgroundColor;

    private TextField control;

    private Label placeholderLabel;

    private VisualElement inputElement;

    public bool IsPasswordField
    {
        get
        {
            return control.isPasswordField;
        }
        set
        {
            control.isPasswordField = value;
        }
    }

    public string PlaceholderText
    {
        get
        {
            return placeholderLabel.text;
        }
        set
        {
            placeholderLabel.text = value;
        }
    }

    public bool IsMultiline
    {
        get
        {
            return control.multiline;
        }
        set
        {
            control.multiline = value;
        }
    }

    public string Text
    {
        get
        {
            return control.text;
        }
        set
        {
            control.SetValueWithoutNotify(value);
            SynchronizePlaceholderVisible();
        }
    }

    public string TooltipText { get; set; }

    public FontStyle FontStyle
    {
        get
        {
            return _fontStyle;
        }
        set
        {
            _fontStyle = value;
            inputElement.style.unityFontStyleAndWeight = GlazierUtils_UIToolkit.GetFontStyle(_fontStyle);
            placeholderLabel.style.unityFontStyleAndWeight = GlazierUtils_UIToolkit.GetFontStyle(_fontStyle);
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
            inputElement.style.unityTextAlign = GlazierUtils_UIToolkit.GetTextAlignment(_fontAlignment);
            placeholderLabel.style.unityTextAlign = GlazierUtils_UIToolkit.GetTextAlignment(_fontAlignment);
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
            StyleLength fontSize = GlazierUtils_UIToolkit.GetFontSize(_fontSize);
            inputElement.style.fontSize = fontSize;
            placeholderLabel.style.fontSize = fontSize;
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
            SynchronizeTextContrast();
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
            Color color = _textColor;
            inputElement.style.color = color;
            placeholderLabel.style.color = color * 0.5f;
            SynchronizeTextContrast();
        }
    }

    public bool AllowRichText
    {
        get
        {
            return false;
        }
        set
        {
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
            inputElement.style.unityBackgroundImageTintColor = _backgroundColor;
        }
    }

    public bool IsClickable
    {
        get
        {
            return control.enabledSelf;
        }
        set
        {
            control.SetEnabled(value);
        }
    }

    public int MaxLength
    {
        get
        {
            return control.maxLength;
        }
        set
        {
            control.maxLength = value;
        }
    }

    public event Entered OnTextSubmitted;

    public event Typed OnTextChanged;

    public event Escaped OnTextEscaped;

    public void FocusControl()
    {
        if (control.focusController.focusedElement != control)
        {
            control.Focus();
        }
    }

    public void ClearFocus()
    {
        control.Blur();
    }

    public GlazierStringField_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
        control = new TextField();
        control.userData = this;
        control.AddToClassList("unturned-field");
        control.RegisterValueChangedCallback(OnControlValueChanged);
        control.RegisterCallback<KeyUpEvent>(OnControlKeyUp);
        control.maxLength = 100;
        inputElement = control.Q(null, TextField.inputUssClassName);
        placeholderLabel = new Label();
        placeholderLabel.AddToClassList("unturned-field__placeholder");
        placeholderLabel.pickingMode = PickingMode.Ignore;
        control.Add(placeholderLabel);
        visualElement = control;
    }

    protected virtual void OnControlValueChanged(ChangeEvent<string> changeEvent)
    {
        this.OnTextChanged?.Invoke(this, changeEvent.newValue);
        SynchronizePlaceholderVisible();
    }

    protected virtual void OnSubmitted()
    {
        this.OnTextSubmitted?.Invoke(this);
        SynchronizePlaceholderVisible();
    }

    internal override void SynchronizeColors()
    {
        Color color = _textColor;
        inputElement.style.color = color;
        inputElement.style.unityBackgroundImageTintColor = _backgroundColor;
        placeholderLabel.style.color = color * 0.5f;
        SynchronizeTextContrast();
    }

    internal override bool GetTooltipParameters(out string tooltipText, out Color tooltipColor)
    {
        tooltipText = TooltipText;
        tooltipColor = _textColor;
        return true;
    }

    private void SynchronizePlaceholderVisible()
    {
        placeholderLabel.visible = string.IsNullOrEmpty(control.text);
    }

    private void SynchronizeTextContrast()
    {
        float a = _textColor.Get().a;
        GlazierUtils_UIToolkit.ApplyTextContrast(inputElement.style, _contrastContext, a);
        GlazierUtils_UIToolkit.ApplyTextContrast(placeholderLabel.style, _contrastContext, a);
    }

    private void OnControlKeyUp(KeyUpEvent keyUpEvent)
    {
        if (keyUpEvent.keyCode == KeyCode.Escape)
        {
            control.Blur();
            this.OnTextEscaped?.Invoke(this);
        }
        else if ((keyUpEvent.keyCode == KeyCode.Return || keyUpEvent.keyCode == KeyCode.KeypadEnter) && !IsMultiline)
        {
            OnSubmitted();
            control.Blur();
        }
    }
}
