using UnityEngine;

namespace SDG.Unturned;

internal class GlazierStringField_IMGUI : GlazierElementBase_IMGUI, ISleekField, ISleekElement, ISleekLabel, ISleekWithTooltip
{
    private int fontSizeInt;

    private ESleekFontSize fontSizeEnum;

    private string controlName;

    public char replace { get; set; } = ' ';


    public string hint { get; set; } = string.Empty;


    public bool multiline { get; set; }

    public string text { get; set; } = string.Empty;


    public string tooltipText { get; set; } = string.Empty;


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

    public SleekColor textColor { get; set; } = GlazierConst.DefaultFieldForegroundColor;


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

    public SleekColor backgroundColor { get; set; } = GlazierConst.DefaultFieldBackgroundColor;


    public int maxLength { get; set; } = 100;


    public event Entered onEntered;

    public event Typed onTyped;

    public event Escaped onEscaped;

    public void FocusControl()
    {
        GUI.FocusControl(controlName);
    }

    public void ClearFocus()
    {
        if (GUI.GetNameOfFocusedControl() == controlName)
        {
            GUI.FocusControl(string.Empty);
        }
    }

    public override void OnGUI()
    {
        GUI.SetNextControlName(controlName);
        if (replace != ' ')
        {
            text = GlazierUtils_IMGUI.DrawPasswordField(drawRect, fontStyle, fontAlignment, fontSizeInt, backgroundColor, textColor, text, maxLength, hint, replace, shadowStyle);
        }
        else
        {
            text = GlazierUtils_IMGUI.DrawTextInputField(drawRect, fontStyle, fontAlignment, fontSizeInt, backgroundColor, textColor, text, maxLength, hint, multiline, shadowStyle);
        }
        if (GUI.changed)
        {
            this.onTyped?.Invoke(this, text);
        }
        if (GUI.GetNameOfFocusedControl() == controlName && Event.current.isKey && Event.current.type == EventType.KeyUp)
        {
            if (Event.current.keyCode == KeyCode.Escape)
            {
                GUI.FocusControl(string.Empty);
                this.onEscaped?.Invoke(this);
            }
            else if (Event.current.keyCode == KeyCode.Return && !multiline)
            {
                this.onEntered?.Invoke(this);
                GUI.FocusControl(string.Empty);
            }
        }
        ChildrenOnGUI();
    }

    public GlazierStringField_IMGUI()
    {
        backgroundColor = GlazierConst.DefaultFieldBackgroundColor;
        controlName = GlazierUtils_IMGUI.CreateUniqueControlName();
        fontSize = ESleekFontSize.Default;
    }
}
