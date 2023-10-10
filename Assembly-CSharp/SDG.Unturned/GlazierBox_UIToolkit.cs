using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned;

internal class GlazierBox_UIToolkit : GlazierElementBase_UIToolkit, ISleekBox, ISleekLabel, ISleekElement, ISleekWithTooltip
{
    private FontStyle _fontStyle;

    private TextAnchor _fontAlignment = TextAnchor.MiddleCenter;

    private ESleekFontSize _fontSize;

    private ETextContrastContext _contrastContext;

    private SleekColor _textColor = GlazierConst.DefaultBoxForegroundColor;

    private SleekColor _backgroundColor = GlazierConst.DefaultBoxBackgroundColor;

    private VisualElement boxElement;

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
            bool flag = !string.IsNullOrEmpty(value);
            labelElement.visible = flag;
            labelElement.style.visibility = ((!flag) ? Visibility.Hidden : Visibility.Visible);
            labelElement.style.display = ((!flag) ? DisplayStyle.None : DisplayStyle.Flex);
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
            boxElement.style.unityBackgroundImageTintColor = _backgroundColor;
        }
    }

    public GlazierBox_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
        boxElement = new VisualElement();
        boxElement.AddToClassList("unturned-box");
        boxElement.userData = this;
        labelElement = new Label();
        labelElement.pickingMode = PickingMode.Ignore;
        labelElement.AddToClassList("unturned-box-label");
        labelElement.enableRichText = false;
        boxElement.Add(labelElement);
        Text = string.Empty;
        visualElement = boxElement;
    }

    internal override void SynchronizeColors()
    {
        labelElement.style.color = _textColor;
        boxElement.style.unityBackgroundImageTintColor = _backgroundColor;
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
}
