namespace SDG.Unturned;

public interface ISleekField : ISleekElement, ISleekLabel, ISleekWithTooltip
{
    bool IsPasswordField { get; set; }

    string PlaceholderText { get; set; }

    bool IsMultiline { get; set; }

    int MaxLength { get; set; }

    SleekColor BackgroundColor { get; set; }

    event Entered OnTextSubmitted;

    event Typed OnTextChanged;

    event Escaped OnTextEscaped;

    void FocusControl();

    void ClearFocus();
}
