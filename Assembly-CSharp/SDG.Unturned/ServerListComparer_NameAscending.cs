using System.Collections.Generic;

namespace SDG.Unturned;

/// <summary>
/// Sort servers by name A to Z.
/// </summary>
public class ServerListComparer_NameAscending : IComparer<SteamServerAdvertisement>
{
    public virtual int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        return lhs.name.CompareTo(rhs.name);
    }
}
