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

    public string Text
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

    public string TooltipText
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

    public FontStyle FontStyle { get; set; }

    public TextAnchor TextAlignment { get; set; } = TextAnchor.MiddleCenter;


    public ESleekFontSize FontSize
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

    public ETextContrastContext TextContrastContext { get; set; }

    public SleekColor TextColor { get; set; } = GlazierConst.DefaultLabelForegroundColor;


    public bool AllowRichText { get; set; }

    protected virtual void calculateContent()
    {
        content = new GUIContent(Text, TooltipText);
        if (AllowRichText)
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
        FontSize = ESleekFontSize.Default;
    }

    public override void OnGUI()
    {
        GlazierUtils_IMGUI.drawLabel(drawRect, FontStyle, TextAlignment, fontSizeInt, shadowContent, TextColor, content, TextContrastContext);
        ChildrenOnGUI();
    }
}
