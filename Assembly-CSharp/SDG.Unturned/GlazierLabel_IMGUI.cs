using UnityEngine;

namespace SDG.Unturned;

internal class GlazierLabel_IMGUI : GlazierElementBase_IMGUI, ISleekLabel, ISleekElement
{
    private string _text = "";

    private string _tooltip = "";

    protected int fontSizeInt;

    private ESleekFontSize fontSizeEnum;

    public GUIContent content;

    protected GUIContent shadowContent;

    public string text
    {
        get
        {
            return _text;
        }
        set
        {
            _text = value;
            calculateContent();
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
            calculateContent();
        }
    }

    public FontStyle fontStyle { get; set; }

    public TextAnchor fontAlignment { get; set; } = TextAnchor.MiddleCenter;


    public ESleekFontSize fontSize
    {
        get
        {
            return fontSizeEnum;
        }
        set
        {
            fontSizeEnum = value;
            fontSizeInt = GlazierUtils_IMGUI.GetFontSize(fontSizeEnum);
        }
    }

    public ETextContrastContext shadowStyle { get; set; }

    public SleekColor textColor { get; set; } = GlazierConst.DefaultLabelForegroundColor;


    public bool enableRichText { get; set; }

    protected virtual void calculateContent()
    {
        content = new GUIContent(text, tooltipText);
        if (enableRichText)
        {
            shadowContent = RichTextUtil.makeShadowContent(content);
        }
        else
        {
            shadowContent = null;
        }
    }

    public GlazierLabel_IMGUI()
    {
        calculateContent();
        fontSize = ESleekFontSize.Default;
    }

    public override void OnGUI()
    {
        GlazierUtils_IMGUI.drawLabel(drawRect, fontStyle, fontAlignment, fontSizeInt, shadowContent, textColor, content, shadowStyle);
        ChildrenOnGUI();
    }
}
