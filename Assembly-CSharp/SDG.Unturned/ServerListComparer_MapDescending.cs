namespace SDG.Unturned;

/// <summary>
/// Sort servers by map name Z to A.
/// </summary>
public class ServerListComparer_MapDescending : ServerListComparer_MapAscending
{
    public override int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
