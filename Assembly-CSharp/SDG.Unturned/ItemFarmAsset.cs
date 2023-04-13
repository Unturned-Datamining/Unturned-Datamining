using System;

namespace SDG.Unturned;

public class ItemFarmAsset : ItemBarricadeAsset
{
    protected uint _growth;

    protected ushort _grow;

    public Guid growSpawnTableGuid;

    public uint harvestRewardExperience;

    public uint growth => _growth;

    public ushort grow => _grow;

    public bool ignoreSoilRestrictions { get; protected set; }

    public bool canFertilize { get; protected set; }

    public bool isAffectedByAgricultureSkill { get; protected set; }

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
    }
}
