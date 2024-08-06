using UnityEngine;

namespace SDG.Unturned;

internal abstract class GlazierNumericField_IMGUI : GlazierElementBase_IMGUI, ISleekNumericField, ISleekWithTooltip
{
    protected string text;

    public FontStyle fontStyle;

    public TextAnchor fontAlignment = TextAnchor.MiddleCenter;

    public int fontSizeInt = GlazierUtils_IMGUI.GetFontSize(ESleekFontSize.Default);

    private string controlName;

    public string TooltipText { get; set; } = string.Empty;


    public SleekColor TextColor { get; set; } = GlazierConst.DefaultFieldForegroundColor;


    public SleekColor BackgroundColor { get; set; } = GlazierConst.DefaultFieldBackgroundColor;


    public bool IsClickable { get; set; } = true;


    public GlazierNumericField_IMGUI()
    {
        controlName = GlazierUtils_IMGUI.CreateUniqueControlName();
    }

    public override void OnGUI()
    {
        bool enabled = GUI.enabled;
        GUI.enabled = IsClickable;
        GUI.SetNextControlName(controlName);
        string input = GlazierUtils_IMGUI.drawField(drawRect, fontStyle, fontAlignment, fontSizeInt, BackgroundColor, TextColor, text, 64, multiline: false, ETextContrastContext.Default);
        GUI.enabled = enabled;
        if (GUI.changed && ParseNumericInput(input))
        {
            text = input;
        }
        if (GUI.GetNameOfFocusedControl() == controlName && Event.current.isKey && Event.current.type == EventType.KeyUp)
        {
            if (Event.current.keyCode == KeyCode.Escape || Event.current.keyCode == ControlsSettings.dashboard)
            {
                GUI.FocusControl(string.Empty);
            }
            else if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
            {
                OnReturnPressed();
                GUI.FocusControl(string.Empty);
            }
        }
        ChildrenOnGUI();
    }

    protected abstract bool ParseNumericInput(string input);

    protected virtual void OnReturnPressed()
    {
    }
}
