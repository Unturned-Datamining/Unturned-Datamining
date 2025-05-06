using System;

namespace SDG.Unturned;

public class ItemTankAsset : ItemBarricadeAsset
{
    protected ETankSource _source;

    protected ushort _resource;

    private byte[] resourceState;

    public ETankSource source => _source;

    public ushort resource => _resource;

    public override byte[] getState(EItemOrigin origin)
    {
        byte[] array = new byte[2];
        if (origin == EItemOrigin.ADMIN)
        {
            array[0] = resourceState[0];
            array[1] = resourceState[1];
        }
        return array;
    }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        switch (source)
        {
        case ETankSource.FUEL:
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_FuelCapacity", resource), 2000);
            break;
        case ETankSource.WATER:
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WaterCapacity", resource), 2000);
            break;
        }
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _source = (ETankSource)Enum.Parse(typeof(ETankSource), p.data.GetString("Source"), ignoreCase: true);
        _resource = p.data.ParseUInt16("Resource", 0);
        resourceState = BitConverter.GetBytes(resource);
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Tank");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Source", source);
        orAddDeclaration.Append("Resource", resource);
    }
}
