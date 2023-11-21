namespace SDG.Unturned;

public class ServerListComparer_WorkshopInverted : ServerListComparer_WorkshopDefault
{
    public override int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
