namespace SDG.Unturned;

public class ServerListComparer_CheatsInverted : ServerListComparer_CheatsDefault
{
    public override int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
