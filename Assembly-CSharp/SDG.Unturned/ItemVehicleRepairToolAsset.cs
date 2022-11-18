namespace SDG.Unturned;

public class ItemVehicleRepairToolAsset : ItemToolAsset
{
    public override bool shouldFriendlySentryTargetUser => false;

    public ItemVehicleRepairToolAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
    }
}
