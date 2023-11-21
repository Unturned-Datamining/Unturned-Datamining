using System.Collections.Generic;

namespace SDG.Unturned;

public class ServerListComparer_CheatsDefault : IComparer<SteamServerInfo>
{
    public virtual int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        if (lhs.hasCheats == rhs.hasCheats)
        {
            return lhs.name.CompareTo(rhs.name);
        }
        if (!lhs.hasCheats)
        {
            return -1;
        }
        return 1;
    }
}
