namespace SDG.Unturned;

public interface ISleekUInt16Field : ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    ushort Value { get; set; }

    ushort MinValue { get; set; }

    ushort MaxValue { get; set; }

    event TypedUInt16 OnValueChanged;
}
