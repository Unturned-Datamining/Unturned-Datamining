using UnityEngine;

namespace SDG.Unturned;

internal class GlazierButton_IMGUI : GlazierLabel_IMGUI, ISleekButton, ISleekElement, ISleekLabel, ISleekWithTooltip
{
    private bool _isRaycastTarget = true;

    public bool IsClickable { get; set; } = true;


    public bool IsRaycastTarget
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

    public SleekColor BackgroundColor { get; set; }

    public event ClickedButton OnClicked;

    public event ClickedButton OnRightClicked;

    public override void OnGUI()
    {
        bool enabled = GUI.enabled;
        GUI.enabled = IsClickable;
        if (IsRaycastTarget)
        {
            if (GlazierUtils_IMGUI.drawButton(drawRect, BackgroundColor))
            {
                if (Event.current.button == 0)
                {
                    this.OnClicked?.Invoke(this);
                }
                else if (Event.current.button == 1)
                {
                    this.OnRightClicked?.Invoke(this);
                }
            }
        }
        else
        {
            GlazierUtils_IMGUI.drawBox(drawRect, BackgroundColor);
        }
        GUI.enabled = enabled;
        GlazierUtils_IMGUI.drawLabel(drawRect, base.FontStyle, base.TextAlignment, fontSizeInt, shadowContent, base.TextColor, content, base.TextContrastContext);
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
        BackgroundColor = GlazierConst.DefaultButtonBackgroundColor;
    }
}
