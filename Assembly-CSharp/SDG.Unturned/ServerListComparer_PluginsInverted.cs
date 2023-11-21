namespace SDG.Unturned;

public class ServerListComparer_PluginsInverted : ServerListComparer_PluginsDefault
{
    public override int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
