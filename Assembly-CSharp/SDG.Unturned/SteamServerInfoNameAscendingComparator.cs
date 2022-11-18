using System.Collections.Generic;

namespace SDG.Unturned;

public class SteamServerInfoNameAscendingComparator : IComparer<SteamServerInfo>
{
    public int Compare(SteamServerInfo a, SteamServerInfo b)
    {
        return a.name.CompareTo(b.name);
    }
}
