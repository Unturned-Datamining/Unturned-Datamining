using System.Collections.Generic;

namespace SDG.Unturned;

public class ServerListComparer_GoldDefault : IComparer<SteamServerInfo>
{
    public virtual int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        if (lhs.isPro == rhs.isPro)
        {
            return lhs.name.CompareTo(rhs.name);
        }
        if (!lhs.isPro)
        {
            return 1;
        }
        return -1;
    }
}
