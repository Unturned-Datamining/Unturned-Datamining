using System.Collections.Generic;

namespace SDG.Unturned;

/// <summary>
/// Sort servers by map name A to Z.
/// </summary>
public class ServerListComparer_MapAscending : IComparer<SteamServerInfo>
{
    public virtual int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        if (string.Equals(lhs.map, rhs.map))
        {
            return lhs.name.CompareTo(rhs.name);
        }
        return lhs.map.CompareTo(rhs.map);
    }
}
