namespace SDG.Unturned;

public interface ISleekNumericField : ISleekWithTooltip
{
    SleekColor BackgroundColor { get; set; }

    SleekColor TextColor { get; set; }

    bool IsClickable { get; set; }
}
