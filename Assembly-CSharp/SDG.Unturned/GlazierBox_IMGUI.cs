namespace SDG.Unturned;

internal class GlazierBox_IMGUI : GlazierLabel_IMGUI, ISleekBox, ISleekLabel, ISleekElement, ISleekWithTooltip
{
    public SleekColor backgroundColor { get; set; } = GlazierConst.DefaultBoxBackgroundColor;


    public override void OnGUI()
    {
        GlazierUtils_IMGUI.drawBox(drawRect, backgroundColor);
        GlazierUtils_IMGUI.drawLabel(drawRect, base.fontStyle, base.fontAlignment, fontSizeInt, shadowContent, base.textColor, content, base.shadowStyle);
        ChildrenOnGUI();
    }
}
