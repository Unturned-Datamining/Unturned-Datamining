namespace SDG.Unturned;

public class ServerListComparer_GoldInverted : ServerListComparer_GoldDefault
{
    public override int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
