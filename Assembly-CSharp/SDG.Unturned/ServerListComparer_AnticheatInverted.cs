namespace SDG.Unturned;

public class ServerListComparer_AnticheatInverted : ServerListComparer_AnticheatDefault
{
    public override int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
