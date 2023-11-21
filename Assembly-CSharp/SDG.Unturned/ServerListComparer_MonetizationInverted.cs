namespace SDG.Unturned;

public class ServerListComparer_MonetizationInverted : ServerListComparer_MonetizationDefault
{
    public override int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
