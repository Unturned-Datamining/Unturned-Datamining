using SDG.Framework.Devkit;

namespace SDG.Unturned;

public class NPCTeleportReward : INPCReward
{
    public string spawnpoint { get; protected set; }

    public override void GrantReward(Player player)
    {
        Spawnpoint spawnpoint = SpawnpointSystemV2.Get().FindSpawnpoint(this.spawnpoint);
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

    public NPCTeleportReward(string newSpawnpoint, string newText)
        : base(newText)
    {
        spawnpoint = newSpawnpoint;
    }
}
