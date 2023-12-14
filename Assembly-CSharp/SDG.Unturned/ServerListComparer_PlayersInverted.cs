namespace SDG.Unturned;

/// <summary>
/// Sort servers by player count low to high.
/// </summary>
public class ServerListComparer_PlayersInverted : ServerListComparer_PlayersDefault
{
    public override int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
