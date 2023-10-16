namespace SDG.Unturned;

public interface ISleekUInt32Field : ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    uint Value { get; set; }

    event TypedUInt32 OnValueChanged;
}
