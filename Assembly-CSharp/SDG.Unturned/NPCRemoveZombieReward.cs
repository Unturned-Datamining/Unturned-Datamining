using UnityEngine;

namespace SDG.Unturned;

public class NPCRemoveZombieReward : INPCReward
{
    /// <summary>
    /// If not none, only remove zombies of this type.
    /// </summary>
    public EZombieSpeciality ZombieSpeciality { get; set; }

    /// <summary>
    /// If greater than zero, only remove zombies matching this table unique ID.
    /// </summary>
    public int LevelTableUniqueId { get; set; }

    /// <summary>
    /// Navmesh index to remove zombies within. If set to byte.MaxValue then zombies are removed everywhere.
    /// </summary>
    public byte NavmeshIndex { get; set; }

    public override void GrantReward(Player player)
    {
        if (ZombieManager.regions == null)
        {
            return;
        }
        if (NavmeshIndex == byte.MaxValue)
        {
            ZombieRegion[] regions = ZombieManager.regions;
            foreach (ZombieRegion region in regions)
            {
                ApplyToRegion(region);
            }
        }
        else if (NavmeshIndex < ZombieManager.regions.Length)
        {
            ApplyToRegion(ZombieManager.regions[NavmeshIndex]);
        }
    }

    private void ApplyToRegion(ZombieRegion region)
    {
        foreach (Zombie zombie in region.zombies)
        {
            if (!(zombie == null) && !zombie.isDead && (LevelTableUniqueId <= 0 || zombie.type == LevelTableUniqueId) && (ZombieSpeciality == EZombieSpeciality.NONE || zombie.speciality == ZombieSpeciality))
            {
                zombie.askDamage(65000, Vector3.up, out var _, out var _, trackKill: false, dropLoot: false);
            }
        }
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        ZombieSpeciality = p.data.ParseEnum("Zombie", EZombieSpeciality.NONE);
        LevelTableUniqueId = p.data.ParseInt32("LevelTable", -1);
        NavmeshIndex = p.data.ParseUInt8("Nav", byte.MaxValue);
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        ZombieSpeciality = p.data.ParseEnum(p.legacyPrefix + "_Zombie", EZombieSpeciality.NONE);
        LevelTableUniqueId = p.data.ParseInt32(p.legacyPrefix + "_LevelTable", -1);
        NavmeshIndex = p.data.ParseUInt8(p.legacyPrefix + "_Nav", byte.MaxValue);
    }
}
