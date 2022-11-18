namespace SDG.Unturned;

public class BlueprintSupply
{
    private ushort _id;

    private bool _isCritical;

    public ushort amount;

    public ushort hasAmount;

    public ushort id => _id;

    public bool isCritical => _isCritical;

    public BlueprintSupply(ushort newID, bool newCritical, byte newAmount)
    {
        _id = newID;
        _isCritical = newCritical;
        amount = newAmount;
        hasAmount = 0;
    }
}
