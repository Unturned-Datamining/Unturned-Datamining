namespace SDG.Unturned;

public class ServerListComparer_CheatsInverted : ServerListComparer_CheatsDefault
{
    public override int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
