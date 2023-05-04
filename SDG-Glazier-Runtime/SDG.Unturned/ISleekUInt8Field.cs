namespace SDG.Unturned;

public interface ISleekUInt8Field : ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    byte state { get; set; }

    event TypedByte onTypedByte;
}
