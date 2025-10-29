using System;
using System.Collections.Generic;

namespace SDG.Framework.Devkit;

[Obsolete("Made SpawnpointSystem no longer static")]
public static class SpawnpointSystem
{
    [Obsolete("Made SpawnpointSystem no longer static")]
    public static List<Spawnpoint> spawnpoints => SpawnpointSystemV2.Get().allSpawnpoints;

    [Obsolete("Made SpawnpointSystem no longer static")]
    public static Spawnpoint getSpawnpoint(string id)
    {
        return SpawnpointSystemV2.Get().FindFirstSpawnpoint(id);
    }
}
