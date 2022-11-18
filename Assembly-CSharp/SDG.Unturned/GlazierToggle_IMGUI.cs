using UnityEngine;

namespace SDG.Unturned;

internal class GlazierToggle_IMGUI : GlazierElementBase_IMGUI, ISleekToggle, ISleekElement, ISleekWithTooltip
{
    private string _tooltip;

    protected GUIContent content = new GUIContent();

    public bool state { get; set; }

    public string tooltipText
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

    public SleekColor backgroundColor { get; set; } = GlazierConst.DefaultToggleBackgroundColor;


    public SleekColor foregroundColor { get; set; } = GlazierConst.DefaultToggleForegroundColor;


    public bool isInteractable { get; set; } = true;


    public event Toggled onToggled;

    public override void OnGUI()
    {
        bool enabled = GUI.enabled;
        GUI.enabled = isInteractable;
        bool flag = GlazierUtils_IMGUI.drawToggle(drawRect, backgroundColor, state, content);
        GUI.enabled = enabled;
        if (flag != state && this.onToggled != null)
        {
            this.onToggled(this, flag);
        }
        state = flag;
        ChildrenOnGUI();
    }
}
