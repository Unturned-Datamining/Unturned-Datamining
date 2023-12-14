using System.Collections.Generic;

namespace SDG.Unturned;

public class ServerListComparer_PasswordDefault : IComparer<SteamServerAdvertisement>
{
    public virtual int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
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
