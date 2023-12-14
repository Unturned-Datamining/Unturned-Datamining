namespace SDG.Unturned;

/// <summary>
/// Sort servers by name Z to A.
/// </summary>
public class ServerListComparer_NameDescending : ServerListComparer_NameAscending
{
    public override int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
