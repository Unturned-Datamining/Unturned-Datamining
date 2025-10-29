using System;
using SDG.Framework.Devkit;

namespace SDG.Unturned;

public class NPCTeleportReward : INPCReward
{
    public string spawnpoint { get; protected set; }

    public override void GrantReward(Player player)
    {
        Spawnpoint spawnpoint = SpawnpointSystemV2.Get().FindFirstSpawnpoint(this.spawnpoint);
        if (spawnpoint == null)
        {
            UnturnedLog.error("Failed to find NPC teleport reward spawnpoint: " + this.spawnpoint);
        }
        else if (!player.teleportToLocation(spawnpoint.transform.position, spawnpoint.transform.rotation.eulerAngles.y))
        {
            UnturnedLog.error("Unable to reward NPC teleport because {0} was obstructed.", this.spawnpoint);
        }
    }

    public override string ToString()
    {
        if (grantDelaySeconds > 0f)
        {
            return $"teleport to \"{spawnpoint}\" after {grantDelaySeconds} s";
        }
        return "teleport to \"" + spawnpoint + "\"";
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryGetString("Spawnpoint", out var value))
        {
            spawnpoint = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Spawnpoint");
        }
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryGetString(p.legacyPrefix + "_Spawnpoint", out var value))
        {
            spawnpoint = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Spawnpoint");
        }
    }

    public NPCTeleportReward()
    {
    }

    [Obsolete]
    public NPCTeleportReward(string newSpawnpoint, string newText)
        : base(newText)
    {
        spawnpoint = newSpawnpoint;
    }
}
