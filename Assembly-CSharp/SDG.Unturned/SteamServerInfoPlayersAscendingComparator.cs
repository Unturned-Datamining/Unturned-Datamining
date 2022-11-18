using System.Collections.Generic;

namespace SDG.Unturned;

public class SteamServerInfoPlayersAscendingComparator : IComparer<SteamServerInfo>
{
    public int Compare(SteamServerInfo a, SteamServerInfo b)
    {
        return b.players - a.players;
    }
}
