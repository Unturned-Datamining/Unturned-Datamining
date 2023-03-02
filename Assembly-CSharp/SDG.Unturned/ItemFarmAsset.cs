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

    public ItemFarmAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _growth = data.readUInt32("Growth");
        _grow = data.readUInt16("Grow", 0);
        growSpawnTableGuid = data.readGUID("Grow_SpawnTable");
        ignoreSoilRestrictions = data.has("Ignore_Soil_Restrictions");
        canFertilize = data.readBoolean("Allow_Fertilizer", defaultValue: true);
        harvestRewardExperience = data.readUInt32("Harvest_Reward_Experience", 1u);
        isAffectedByAgricultureSkill = data.readBoolean("Affected_By_Agriculture_Skill", defaultValue: true);
    }
}
