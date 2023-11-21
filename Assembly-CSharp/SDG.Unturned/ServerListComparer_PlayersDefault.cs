using System.Collections.Generic;

namespace SDG.Unturned;

/// <summary>
/// Sort servers by player count high to low.
/// </summary>
public class ServerListComparer_PlayersDefault : IComparer<SteamServerInfo>
{
    public virtual int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        if (lhs.players == rhs.players)
        {
            return lhs.name.CompareTo(rhs.name);
        }
        return rhs.players - lhs.players;
    }
}
