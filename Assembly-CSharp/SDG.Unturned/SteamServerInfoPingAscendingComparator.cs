using System.Collections.Generic;

namespace SDG.Unturned;

public class SteamServerInfoPingAscendingComparator : IComparer<SteamServerInfo>
{
    public int Compare(SteamServerInfo a, SteamServerInfo b)
    {
        return a.ping - b.ping;
    }
}
