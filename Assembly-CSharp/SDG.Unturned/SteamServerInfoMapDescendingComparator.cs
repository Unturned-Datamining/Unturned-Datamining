using System.Collections.Generic;

namespace SDG.Unturned;

public class SteamServerInfoMapDescendingComparator : IComparer<SteamServerInfo>
{
    public int Compare(SteamServerInfo a, SteamServerInfo b)
    {
        return b.map.CompareTo(a.map);
    }
}
