using System.Collections.Generic;

namespace SDG.Unturned;

/// <summary>
/// Sort servers by ping low to high.
/// </summary>
public class ServerListComparer_PingAscending : IComparer<SteamServerAdvertisement>
{
    public virtual int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        if (lhs.sortingPing == rhs.sortingPing)
        {
            if (lhs.players == rhs.players)
            {
                return lhs.name.CompareTo(rhs.name);
            }
            return rhs.players - lhs.players;
        }
        return lhs.sortingPing - rhs.sortingPing;
    }
}
