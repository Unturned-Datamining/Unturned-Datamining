using UnityEngine;

namespace SDG.Unturned;

internal class GlazierToggle_IMGUI : GlazierElementBase_IMGUI, ISleekToggle, ISleekElement, ISleekWithTooltip
{
    private string _tooltip;

    protected GUIContent content = new GUIContent();

    public bool Value { get; set; }

    public string TooltipText
    {
        get
        {
            return _tooltip;
        }
        set
        {
            _tooltip = value;
            content = new GUIContent(string.Empty, _tooltip);
        }
    }

    public SleekColor BackgroundColor { get; set; } = GlazierConst.DefaultToggleBackgroundColor;


    public SleekColor ForegroundColor { get; set; } = GlazierConst.DefaultToggleForegroundColor;


    public bool IsInteractable { get; set; } = true;


    public event Toggled OnValueChanged;

    public override void OnGUI()
    {
        bool enabled = GUI.enabled;
        GUI.enabled = IsInteractable;
        bool flag = GlazierUtils_IMGUI.drawToggle(drawRect, BackgroundColor, Value, content);
        GUI.enabled = enabled;
        if (flag != Value)
        {
            this.OnValueChanged?.Invoke(this, flag);
        }
        Value = flag;
        ChildrenOnGUI();
    }
}
