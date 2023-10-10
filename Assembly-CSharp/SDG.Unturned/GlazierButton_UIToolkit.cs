using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned;

internal class GlazierButton_UIToolkit : GlazierElementBase_UIToolkit, ISleekButton, ISleekElement, ISleekLabel, ISleekWithTooltip
{
    private FontStyle _fontStyle;

    private TextAnchor _fontAlignment = TextAnchor.MiddleCenter;

    private ESleekFontSize _fontSize;

    private ETextContrastContext _contrastContext;

    private SleekColor _textColor = GlazierConst.DefaultButtonForegroundColor;

    private SleekColor _backgroundColor = GlazierConst.DefaultButtonBackgroundColor;

    private VisualElement buttonElement;

    private Clickable clickable;

    private Label labelElement;

    public override bool UseManualLayout
    {
        set
        {
            base.UseManualLayout = value;
            labelElement.style.position = (_useManualLayout ? ((StyleEnum<Position>)StyleKeyword.Null) : ((StyleEnum<Position>)Position.Relative));
        }
    }

    public string Text
    {
        get
        {
            return labelElement.text;
        }
        set
        {
            labelElement.text = value;
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
            labelElement.style.unityFontStyleAndWeight = GlazierUtils_UIToolkit.GetFontStyle(_fontStyle);
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
            labelElement.style.unityTextAlign = GlazierUtils_UIToolkit.GetTextAlignment(_fontAlignment);
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
            labelElement.style.fontSize = GlazierUtils_UIToolkit.GetFontSize(_fontSize);
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

    public bool IsClickable
    {
        get
        {
            return buttonElement.enabledSelf;
        }
        set
        {
            buttonElement.SetEnabled(value);
        }
    }

    public bool IsRaycastTarget
    {
        get
        {
            return buttonElement.pickingMode == PickingMode.Position;
        }
        set
        {
            buttonElement.pickingMode = ((!value) ? PickingMode.Ignore : PickingMode.Position);
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
            labelElement.style.color = _textColor;
            SynchronizeTextContrast();
        }
    }

    public bool AllowRichText
    {
        get
        {
            return labelElement.enableRichText;
        }
        set
        {
            labelElement.enableRichText = value;
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
            buttonElement.style.unityBackgroundImageTintColor = _backgroundColor;
        }
    }

    public event ClickedButton OnClicked;

    public event ClickedButton OnRightClicked;

    public GlazierButton_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
        buttonElement = new VisualElement();
        buttonElement.userData = this;
        buttonElement.AddToClassList("unturned-button");
        clickable = new Clickable(OnClickedWithEventInfo);
        GlazierUtils_UIToolkit.AddClickableActivators(clickable);
        buttonElement.AddManipulator(clickable);
        labelElement = new Label();
        labelElement.pickingMode = PickingMode.Ignore;
        labelElement.AddToClassList("unturned-box-label");
        labelElement.enableRichText = false;
        buttonElement.Add(labelElement);
        visualElement = buttonElement;
    }

    internal override void SynchronizeColors()
    {
        labelElement.style.color = _textColor;
        buttonElement.style.unityBackgroundImageTintColor = _backgroundColor;
        SynchronizeTextContrast();
    }

    internal override bool GetTooltipParameters(out string tooltipText, out Color tooltipColor)
    {
        tooltipText = TooltipText;
        tooltipColor = _textColor;
        return true;
    }

    private void SynchronizeTextContrast()
    {
        GlazierUtils_UIToolkit.ApplyTextContrast(labelElement.style, _contrastContext, _textColor.Get().a);
    }

    private void OnClickedWithEventInfo(EventBase eventBase)
    {
        if (eventBase is IMouseEvent mouseEvent)
        {
            switch (mouseEvent.button)
            {
            case 0:
                this.OnClicked?.Invoke(this);
                break;
            case 1:
                this.OnRightClicked?.Invoke(this);
                break;
            }
        }
    }
}
