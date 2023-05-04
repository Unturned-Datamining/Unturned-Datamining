namespace SDG.Unturned;

public interface ISleekField : ISleekElement, ISleekLabel, ISleekWithTooltip
{
    char replace { get; set; }

    string hint { get; set; }

    bool multiline { get; set; }

    int maxLength { get; set; }

    SleekColor backgroundColor { get; set; }

    event Entered onEntered;

    event Typed onTyped;

    event Escaped onEscaped;

    void FocusControl();

    void ClearFocus();
}
