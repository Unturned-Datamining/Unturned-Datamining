namespace SDG.Unturned;

public interface ISleekUInt8Field : ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    byte Value { get; set; }

    event TypedByte OnValueChanged;
}
