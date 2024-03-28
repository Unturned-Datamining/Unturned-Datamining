using System.Collections.Generic;

namespace SDG.Unturned;

/// <summary>
/// Sort servers by max player count high to low.
/// </summary>
public class ServerListComparer_MaxPlayersDefault : IComparer<SteamServerAdvertisement>
{
    public virtual int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        if (lhs.maxPlayers == rhs.maxPlayers)
        {
            if (lhs.players == rhs.players)
            {
                return lhs.name.CompareTo(rhs.name);
            }
            return rhs.players - lhs.players;
        }
        return rhs.maxPlayers - lhs.maxPlayers;
    }
}