namespace SDG.Unturned;

public class ServerListComparer_PasswordInverted : ServerListComparer_PasswordDefault
{
    public override int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
