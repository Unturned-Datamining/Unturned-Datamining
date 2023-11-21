namespace SDG.Unturned;

/// <summary>
/// Sort servers by name Z to A.
/// </summary>
public class ServerListComparer_NameDescending : ServerListComparer_NameAscending
{
    public override int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
