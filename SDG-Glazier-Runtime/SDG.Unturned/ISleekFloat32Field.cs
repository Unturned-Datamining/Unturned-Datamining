namespace SDG.Unturned;

public interface ISleekFloat32Field : ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    float Value { get; set; }

    event TypedSingle OnValueSubmitted;

    event TypedSingle OnValueChanged;
}
