namespace SDG.Unturned;

public interface ISleekFloat64Field : ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    double state { get; set; }

    event TypedDouble onTypedDouble;
}
