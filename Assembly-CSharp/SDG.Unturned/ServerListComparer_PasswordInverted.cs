namespace SDG.Unturned;

public class ServerListComparer_PasswordInverted : ServerListComparer_PasswordDefault
{
    public override int Compare(SteamServerInfo lhs, SteamServerInfo rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
