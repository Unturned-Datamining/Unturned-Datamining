namespace SDG.Unturned;

public interface ISleekToggle : ISleekElement, ISleekWithTooltip
{
    bool state { get; set; }

    SleekColor backgroundColor { get; set; }

    SleekColor foregroundColor { get; set; }

    bool isInteractable { get; set; }

    event Toggled onToggled;
}
