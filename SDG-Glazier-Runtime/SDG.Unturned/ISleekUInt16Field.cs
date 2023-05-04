namespace SDG.Unturned;

public interface ISleekUInt16Field : ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    ushort state { get; set; }

    ushort minValue { get; set; }

    ushort maxValue { get; set; }

    event TypedUInt16 onTypedUInt16;
}
