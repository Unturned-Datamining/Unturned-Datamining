using System.Collections.Generic;

namespace SDG.Unturned;

/// <summary>
/// Sort servers by name A to Z.
/// </summary>
public class ServerListComparer_NameAscending : IComparer<SteamServerInfo>
{
    public virtual int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        return lhs.name.CompareTo(rhs.name);
    }
}
