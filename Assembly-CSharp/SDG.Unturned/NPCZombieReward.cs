using System.Collections.Generic;
using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

public class NPCZombieReward : INPCReward
{
    private static List<Spawnpoint> spawnpointsWorkingCopy = new List<Spawnpoint>();

    /// <summary>
    /// Spawned zombie will be changed to this speciality type.
    /// </summary>
    public EZombieSpeciality ZombieSpeciality { get; set; }

    /// <summary>
    /// Zombie(s) will be spawned at a Spawnpoint node matching this ID.
    /// If multiple Spawnpoints match this ID a random spawnpoint is chosen for each zombie.
    /// </summary>
    public string SpawnpointId { get; set; }

    /// <summary>
    /// If greater than zero, find this zombie type configured in the level editor. For example, if the level editor
    /// lists "0 Fire (4)", then 4 is the unique ID, and if assigned to this reward a zombie from the "Fire"
    /// table will spawn.
    /// </summary>
    public int LevelTableUniqueId { get; set; }

    /// <summary>
    /// Number of zombies to spawn.
    /// </summary>
    public int SpawnQuantity { get; set; }

    /// <summary>
    /// If set, zombies will not spawn unless CooldownDuration seconds have passed since last run.
    /// </summary>
    public string CooldownId { get; set; }

    public float CooldownDuration { get; set; }

    public override void GrantReward(Player player)
    {
        if (SpawnQuantity < 1 || (!string.IsNullOrEmpty(CooldownId) && !ZombieManager.CheckCustomCooldown(CooldownId, CooldownDuration)))
        {
            return;
        }
        if (!SpawnpointSystemV2.Get().idToSpawnpoints.TryGetValue(SpawnpointId, out var value) || value.Count < 1)
        {
            UnturnedLog.error("No spawnpoints for NPC zombie reward matching ID \"" + SpawnpointId + "\"");
            return;
        }
        spawnpointsWorkingCopy.Clear();
        spawnpointsWorkingCopy.AddRange(value);
        int num = LevelZombies.FindTableIndexByUniqueId(LevelTableUniqueId);
        ZombieTable zombieTable = ((num >= 0) ? LevelZombies.tables[num] : null);
        int num2 = SpawnQuantity;
        do
        {
            int randomIndex = spawnpointsWorkingCopy.GetRandomIndex();
            Spawnpoint spawnpoint = spawnpointsWorkingCopy[randomIndex];
            spawnpointsWorkingCopy.RemoveAtFast(randomIndex);
            spawnpoint.transform.GetPositionAndRotation(out var position, out var rotation);
            if (!LevelNavigation.tryGetNavigation(position, out var nav))
            {
                UnturnedLog.error($"Spawnpoint for NPC zombie reward \"{SpawnpointId}\" at {position} isn't within a navmesh");
            }
            else if (SafezoneManager.checkPointValid(position))
            {
                if (ZombieManager.regions == null || nav >= ZombieManager.regions.Length)
                {
                    break;
                }
                Zombie zombie = ZombieManager.regions[nav].FindBestZombieToRespawnDifferentSpeciality(ZombieSpeciality);
                if (zombie == null)
                {
                    UnturnedLog.info("Unable to spawn all zombies for NPC zombie reward \"" + SpawnpointId + "\" because we ran out of candidates");
                    break;
                }
                position += new Vector3(0f, 0.1f, 0f);
                float y = rotation.eulerAngles.y;
                byte type = zombie.type;
                byte shirt = zombie.shirt;
                byte pants = zombie.pants;
                byte hat = zombie.hat;
                byte gear = zombie.gear;
                if (zombieTable != null)
                {
                    type = (byte)num;
                    zombieTable.GetSpawnClothingParameters(out shirt, out pants, out hat, out gear);
                }
                zombie.sendRevive(type, (byte)ZombieSpeciality, shirt, pants, hat, gear, position, y);
                num2--;
            }
        }
        while (num2 > 0 && spawnpointsWorkingCopy.Count > 0);
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseEnum<EZombieSpeciality>("Zombie", out var value))
        {
            ZombieSpeciality = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Zombie");
        }
        if (p.data.TryGetString("Spawnpoint", out var value2))
        {
            SpawnpointId = value2;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Spawnpoint");
        }
        LevelTableUniqueId = p.data.ParseInt32("LevelTableOverride", -1);
        SpawnQuantity = p.data.ParseInt32("SpawnQuantity", 1);
        CooldownId = p.data.GetString("CooldownId");
        CooldownDuration = p.data.ParseFloat("CooldownDuration", -1f);
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseEnum<EZombieSpeciality>(p.legacyPrefix + "_Zombie", out var value))
        {
            ZombieSpeciality = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Zombie");
        }
        if (p.data.TryGetString(p.legacyPrefix + "_Spawnpoint", out var value2))
        {
            SpawnpointId = value2;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Spawnpoint");
        }
        LevelTableUniqueId = p.data.ParseInt32(p.legacyPrefix + "_LevelTableOverride", -1);
        SpawnQuantity = p.data.ParseInt32(p.legacyPrefix + "_SpawnQuantity", 1);
        CooldownId = p.data.GetString(p.legacyPrefix + "_CooldownId");
        CooldownDuration = p.data.ParseFloat(p.legacyPrefix + "_CooldownDuration", -1f);
    }
}
