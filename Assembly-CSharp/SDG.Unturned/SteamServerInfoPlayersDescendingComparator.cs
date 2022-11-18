using System.Collections.Generic;

namespace SDG.Unturned;

public class SteamServerInfoPlayersDescendingComparator : IComparer<SteamServerInfo>
{
    public int Compare(SteamServerInfo a, SteamServerInfo b)
    {
        return a.players - b.players;
    }
}
