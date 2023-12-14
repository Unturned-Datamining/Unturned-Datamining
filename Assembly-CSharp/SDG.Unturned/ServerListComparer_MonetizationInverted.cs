namespace SDG.Unturned;

public class ServerListComparer_MonetizationInverted : ServerListComparer_MonetizationDefault
{
    public override int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
