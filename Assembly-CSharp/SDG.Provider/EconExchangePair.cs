namespace SDG.Provider;

public struct EconExchangePair
{
    public ulong instanceId;

    public ushort quantity;

    public EconExchangePair(ulong newInstanceId, ushort newQuantity)
    {
        instanceId = newInstanceId;
        quantity = newQuantity;
    }
}
