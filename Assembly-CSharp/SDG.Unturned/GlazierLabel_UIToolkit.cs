using UnityEngine;
using UnityEngine.UIElements;

namespace SDG.Unturned;

internal class GlazierLabel_UIToolkit : GlazierElementBase_UIToolkit, ISleekLabel, ISleekElement
{
    private string _text = string.Empty;

    private string _tooltip = string.Empty;

    private FontStyle _fontStyle;

    private TextAnchor _fontAlignment = TextAnchor.MiddleCenter;

    private ESleekFontSize _fontSize;

    private ETextContrastContext _contrastContext;

    private SleekColor _textColor = GlazierConst.DefaultLabelForegroundColor;

    private VisualElement containerElement;

    private Label labelElement;

    public string Text
    {
        get
        {
            return _text;
        }
        set
        {
            _text = value;
            labelElement.text = _text;
        }
    }

    public string tooltipText
    {
        get
        {
            return _tooltip;
        }
        set
        {
            _tooltip = value;
            labelElement.tooltip = _tooltip;
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

    public override bool UseManualLayout
    {
        set
        {
            base.UseManualLayout = value;
            labelElement.style.position = (_useManualLayout ? ((StyleEnum<Position>)StyleKeyword.Null) : ((StyleEnum<Position>)Position.Relative));
        }
    }

    public GlazierLabel_UIToolkit(Glazier_UIToolkit glazier)
        : base(glazier)
    {
        containerElement = new VisualElement();
        containerElement.userData = this;
        containerElement.AddToClassList("unturned-label");
        containerElement.pickingMode = PickingMode.Ignore;
        labelElement = new Label();
        labelElement.AddToClassList("unturned-box-label");
        labelElement.pickingMode = PickingMode.Ignore;
        labelElement.enableRichText = false;
        containerElement.Add(labelElement);
        visualElement = containerElement;
    }

    internal override void SynchronizeColors()
    {
        labelElement.style.color = _textColor;
        SynchronizeTextContrast();
    }

    private void SynchronizeTextContrast()
    {
        GlazierUtils_UIToolkit.ApplyTextContrast(labelElement.style, _contrastContext, _textColor.Get().a);
    }
}
