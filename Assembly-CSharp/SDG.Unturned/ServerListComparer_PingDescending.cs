namespace SDG.Unturned;

/// <summary>
/// Sort servers by ping high to low.
/// </summary>
public class ServerListComparer_PingDescending : ServerListComparer_PingAscending
{
    public override int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
