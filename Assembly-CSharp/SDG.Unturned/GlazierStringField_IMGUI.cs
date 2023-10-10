using UnityEngine;

namespace SDG.Unturned;

internal class GlazierStringField_IMGUI : GlazierElementBase_IMGUI, ISleekField, ISleekElement, ISleekLabel, ISleekWithTooltip
{
    private int fontSizeInt;

    private ESleekFontSize fontSizeEnum;

    private string controlName;

    public bool IsPasswordField { get; set; }

    public string PlaceholderText { get; set; } = string.Empty;


    public bool IsMultiline { get; set; }

    public string Text { get; set; } = string.Empty;


    public string TooltipText { get; set; } = string.Empty;


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

    public SleekColor TextColor { get; set; } = GlazierConst.DefaultFieldForegroundColor;


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

    public SleekColor BackgroundColor { get; set; } = GlazierConst.DefaultFieldBackgroundColor;


    public int MaxLength { get; set; } = 100;


    public event Entered OnTextSubmitted;

    public event Typed OnTextChanged;

    public event Escaped OnTextEscaped;

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
        if (IsPasswordField)
        {
            Text = GlazierUtils_IMGUI.DrawPasswordField(drawRect, FontStyle, TextAlignment, fontSizeInt, BackgroundColor, TextColor, Text, MaxLength, PlaceholderText, '*', TextContrastContext);
        }
        else
        {
            Text = GlazierUtils_IMGUI.DrawTextInputField(drawRect, FontStyle, TextAlignment, fontSizeInt, BackgroundColor, TextColor, Text, MaxLength, PlaceholderText, IsMultiline, TextContrastContext);
        }
        if (GUI.changed)
        {
            this.OnTextChanged?.Invoke(this, Text);
        }
        if (GUI.GetNameOfFocusedControl() == controlName && Event.current.isKey && Event.current.type == EventType.KeyUp)
        {
            if (Event.current.keyCode == KeyCode.Escape)
            {
                GUI.FocusControl(string.Empty);
                this.OnTextEscaped?.Invoke(this);
            }
            else if ((Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter) && !IsMultiline)
            {
                this.OnTextSubmitted?.Invoke(this);
                GUI.FocusControl(string.Empty);
            }
        }
        ChildrenOnGUI();
    }

    public GlazierStringField_IMGUI()
    {
        BackgroundColor = GlazierConst.DefaultFieldBackgroundColor;
        controlName = GlazierUtils_IMGUI.CreateUniqueControlName();
        FontSize = ESleekFontSize.Default;
    }
}
