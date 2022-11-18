namespace SDG.Unturned;

public class BlueprintOutput
{
    private ushort _id;

    public ushort amount;

    public EItemOrigin origin;

    public ushort id => _id;

    public BlueprintOutput(ushort newID, byte newAmount, EItemOrigin newOrigin)
    {
        _id = newID;
        amount = newAmount;
        origin = newOrigin;
    }
}
