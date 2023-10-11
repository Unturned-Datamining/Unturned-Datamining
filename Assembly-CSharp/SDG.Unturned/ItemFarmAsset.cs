using System;

namespace SDG.Unturned;

public class ItemFarmAsset : ItemBarricadeAsset
{
    protected uint _growth;

    protected ushort _grow;

    public Guid growSpawnTableGuid;

    public uint harvestRewardExperience;

    internal NPCRewardsList harvestRewardsList;

    public uint growth => _growth;

    public ushort grow => _grow;

    public bool ignoreSoilRestrictions { get; protected set; }

    public bool canFertilize { get; protected set; }

    public bool isAffectedByAgricultureSkill { get; protected set; }

    public bool shouldRainAffectGrowth { get; protected set; }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (builder.shouldRestrictToLegacyContent)
        {
            return;
        }
        if (grow != 0 && Assets.find(EAssetType.ITEM, grow) is ItemAsset itemAsset)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Farmable_GrowSpecificItem", "<color=" + Palette.hex(ItemTool.getRarityColorUI(itemAsset.rarity)) + ">" + itemAsset.itemName + "</color>"), 2000);
        }
        builder.stringBuilder.Clear();
        if (!ignoreSoilRestrictions)
        {
            builder.stringBuilder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Farmable_RequiresSoil"));
        }
        if (canFertilize)
        {
            if (builder.stringBuilder.Length > 0)
            {
                builder.stringBuilder.Append(' ');
            }
            builder.stringBuilder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Farmable_CanFertilize"));
        }
        if (isAffectedByAgricultureSkill)
        {
            if (builder.stringBuilder.Length > 0)
            {
                builder.stringBuilder.Append(' ');
            }
            builder.stringBuilder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Farmable_AffectedByAgricultureSkill"));
        }
        if (shouldRainAffectGrowth)
        {
            if (builder.stringBuilder.Length > 0)
            {
                builder.stringBuilder.Append(' ');
            }
            builder.stringBuilder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Farmable_AffectedByRain"));
        }
        if (builder.stringBuilder.Length > 0)
        {
            builder.Append(builder.stringBuilder.ToString(), 15000);
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _growth = data.ParseUInt32("Growth");
        _grow = data.ParseUInt16("Grow", 0);
        growSpawnTableGuid = data.ParseGuid("Grow_SpawnTable");
        ignoreSoilRestrictions = data.ContainsKey("Ignore_Soil_Restrictions");
        canFertilize = data.ParseBool("Allow_Fertilizer", defaultValue: true);
        harvestRewardExperience = data.ParseUInt32("Harvest_Reward_Experience", 1u);
        isAffectedByAgricultureSkill = data.ParseBool("Affected_By_Agriculture_Skill", defaultValue: true);
        shouldRainAffectGrowth = data.ParseBool("Rain_Affects_Growth", defaultValue: true);
        harvestRewardsList.Parse(data, localization, this, "Harvest_Rewards", "Harvest_Reward_");
    }
}
