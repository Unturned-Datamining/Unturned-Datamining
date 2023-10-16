namespace SDG.Unturned;

public interface ISleekFloat64Field : ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    double Value { get; set; }

    event TypedDouble OnValueChanged;
}
