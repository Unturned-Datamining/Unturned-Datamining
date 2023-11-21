namespace SDG.Unturned;

public class ServerListComparer_PerspectiveInverted : ServerListComparer_PerspectiveDefault
{
    public override int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
