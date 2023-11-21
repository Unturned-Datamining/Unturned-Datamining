using System.Collections.Generic;

namespace SDG.Unturned;

public class ServerListComparer_PasswordDefault : IComparer<SteamServerInfo>
{
    public virtual int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        if (lhs.isPassworded == rhs.isPassworded)
        {
            return lhs.name.CompareTo(rhs.name);
        }
        if (!lhs.isPassworded)
        {
            return -1;
        }
        return 1;
    }
}
