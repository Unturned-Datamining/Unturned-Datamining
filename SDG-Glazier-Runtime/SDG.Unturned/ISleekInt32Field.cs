namespace SDG.Unturned;

public interface ISleekInt32Field : ISleekElement, ISleekNumericField, ISleekWithTooltip
{
    int state { get; set; }

    event TypedInt32 onTypedInt;
}
