namespace SDG.Unturned;

public interface ISleekToggle : ISleekElement, ISleekWithTooltip
{
    bool Value { get; set; }

    SleekColor BackgroundColor { get; set; }

    SleekColor ForegroundColor { get; set; }

    bool IsInteractable { get; set; }

    event Toggled OnValueChanged;
}
