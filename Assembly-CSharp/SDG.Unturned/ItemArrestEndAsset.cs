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
        if (!builder.shouldRestrictToLegacyContent && _recover != 0 && Assets.find(EAssetType.ITEM, _recover) is ItemArrestStartAsset itemArrestStartAsset)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ArrestEnd_UnlocksItem", "<color=" + Palette.hex(ItemTool.getRarityColorUI(itemArrestStartAsset.rarity)) + ">" + itemArrestStartAsset.itemName + "</color>"), 2000);
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _use = bundle.load<AudioClip>("Use");
        _recover = data.ParseUInt16("Recover", 0);
    }
}
