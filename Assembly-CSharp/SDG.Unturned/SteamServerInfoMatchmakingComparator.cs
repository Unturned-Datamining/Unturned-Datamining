using System.Collections.Generic;

namespace SDG.Unturned;

public class SteamServerInfoMatchmakingComparator : IComparer<SteamServerInfo>
{
    public int Compare(SteamServerInfo a, SteamServerInfo b)
    {
        int num = b.players - a.players;
        if (num != 0)
        {
            return num;
        }
        return a.sortingPing - b.sortingPing;
    }
}
