namespace SDG.Unturned;

public class ServerListComparer_CombatInverted : ServerListComparer_CombatDefault
{
    public override int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
