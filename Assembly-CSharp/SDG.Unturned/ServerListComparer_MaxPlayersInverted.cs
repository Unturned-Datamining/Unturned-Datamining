namespace SDG.Unturned;

/// <summary>
/// Sort servers by max player count low to high.
/// </summary>
public class ServerListComparer_MaxPlayersInverted : ServerListComparer_MaxPlayersDefault
{
    public override int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
