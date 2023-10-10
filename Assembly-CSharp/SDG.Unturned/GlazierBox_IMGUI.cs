namespace SDG.Unturned;

internal class GlazierBox_IMGUI : GlazierLabel_IMGUI, ISleekBox, ISleekLabel, ISleekElement, ISleekWithTooltip
{
    public SleekColor BackgroundColor { get; set; } = GlazierConst.DefaultBoxBackgroundColor;


    public override void OnGUI()
    {
        GlazierUtils_IMGUI.drawBox(drawRect, BackgroundColor);
        GlazierUtils_IMGUI.drawLabel(drawRect, base.FontStyle, base.TextAlignment, fontSizeInt, shadowContent, base.TextColor, content, base.TextContrastContext);
        ChildrenOnGUI();
    }
}
