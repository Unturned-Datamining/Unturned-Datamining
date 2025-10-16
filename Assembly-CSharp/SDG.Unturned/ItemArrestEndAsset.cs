using UnityEngine;

namespace SDG.Unturned;

public class ItemArrestEndAsset : ItemAsset
{
    protected AudioClip _use;

    protected ushort _recover;

    public AudioClip use => _use;

    public ushort recover => _recover;

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (builder.HasFlag(EItemDescriptionFlags.Uncategorized) && _recover != 0 && Assets.find(EAssetType.ITEM, _recover) is ItemArrestStartAsset itemArrestStartAsset)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ArrestEnd_UnlocksItem", "<color=" + Palette.hex(ItemTool.getRarityColorUI(itemArrestStartAsset.rarity)) + ">" + itemArrestStartAsset.itemName + "</color>"), 2000);
        }
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _use = p.bundle.load<AudioClip>("Use");
        _recover = p.data.ParseUInt16("Recover", 0);
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("ArrestEnd");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Recover", recover);
    }
}
