using System.Collections.Generic;

namespace SDG.Unturned;

public class SteamServerInfoMapAscendingComparator : IComparer<SteamServerInfo>
{
    public int Compare(SteamServerInfo a, SteamServerInfo b)
    {
        return a.map.CompareTo(b.map);
    }
}
