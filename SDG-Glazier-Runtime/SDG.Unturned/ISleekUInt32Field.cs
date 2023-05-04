namespace SDG.Unturned;

public interface ISleekUInt32Field : ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    uint state { get; set; }

    event TypedUInt32 onTypedUInt32;
}
