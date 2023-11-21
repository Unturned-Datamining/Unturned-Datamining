namespace SDG.Unturned;

public class ServerListComparer_GoldInverted : ServerListComparer_GoldDefault
{
    public override int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
