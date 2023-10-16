namespace SDG.Unturned;

public interface ISleekButton : ISleekElement, ISleekLabel, ISleekWithTooltip
{
    SleekColor BackgroundColor { get; set; }

    bool IsClickable { get; set; }

    bool IsRaycastTarget { get; set; }

    event ClickedButton OnClicked;

    event ClickedButton OnRightClicked;
}
