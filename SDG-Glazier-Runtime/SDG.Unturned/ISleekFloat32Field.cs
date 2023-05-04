namespace SDG.Unturned;

public interface ISleekFloat32Field : ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    float state { get; set; }

    event TypedSingle onEnteredSingle;

    event TypedSingle onTypedSingle;
}
