using System.Collections.Generic;

namespace SDG.Unturned;

public class SteamServerInfoNameDescendingComparator : IComparer<SteamServerInfo>
{
    public int Compare(SteamServerInfo a, SteamServerInfo b)
    {
        return b.name.CompareTo(a.name);
    }
}
