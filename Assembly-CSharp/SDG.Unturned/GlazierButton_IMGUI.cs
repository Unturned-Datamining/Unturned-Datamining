using UnityEngine;

namespace SDG.Unturned;

internal class GlazierButton_IMGUI : GlazierLabel_IMGUI, ISleekButton, ISleekElement, ISleekLabel, ISleekWithTooltip
{
    private bool _isRaycastTarget = true;

    public bool isClickable { get; set; } = true;


    public bool isRaycastTarget
    {
        get
        {
            return _isRaycastTarget;
        }
        set
        {
            _isRaycastTarget = value;
            calculateContent();
        }
    }

    public SleekColor backgroundColor { get; set; }

    public event ClickedButton onClickedButton;

    public event ClickedButton onRightClickedButton;

    public override void OnGUI()
    {
        bool enabled = GUI.enabled;
        GUI.enabled = isClickable;
        if (isRaycastTarget)
        {
            if (GlazierUtils_IMGUI.drawButton(drawRect, backgroundColor))
            {
                if (Event.current.button == 0)
                {
                    this.onClickedButton?.Invoke(this);
                }
                else if (Event.current.button == 1)
                {
                    this.onRightClickedButton?.Invoke(this);
                }
            }
        }
        else
        {
            GlazierUtils_IMGUI.drawBox(drawRect, backgroundColor);
        }
        GUI.enabled = enabled;
        GlazierUtils_IMGUI.drawLabel(drawRect, base.fontStyle, base.fontAlignment, fontSizeInt, shadowContent, base.textColor, content, base.shadowStyle);
        ChildrenOnGUI();
    }

    protected override void calculateContent()
    {
        base.calculateContent();
        if (!_isRaycastTarget)
        {
            content.tooltip = null;
        }
    }

    public GlazierButton_IMGUI()
    {
        backgroundColor = GlazierConst.DefaultButtonBackgroundColor;
    }
}
