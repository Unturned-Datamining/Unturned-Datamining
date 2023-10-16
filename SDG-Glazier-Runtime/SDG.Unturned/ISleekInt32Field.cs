namespace SDG.Unturned;

public interface ISleekInt32Field : ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    int Value { get; set; }

    event TypedInt32 OnValueChanged;
}
