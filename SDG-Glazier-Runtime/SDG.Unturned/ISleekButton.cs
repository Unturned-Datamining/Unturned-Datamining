namespace SDG.Unturned;

public interface ISleekButton : ISleekElement, ISleekLabel, ISleekWithTooltip
{
    SleekColor backgroundColor { get; set; }

    bool isClickable { get; set; }

    bool isRaycastTarget { get; set; }

    event ClickedButton onClickedButton;

    event ClickedButton onRightClickedButton;
}
