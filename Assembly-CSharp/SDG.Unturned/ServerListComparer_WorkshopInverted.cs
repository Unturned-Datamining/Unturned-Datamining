namespace SDG.Unturned;

public class ServerListComparer_WorkshopInverted : ServerListComparer_WorkshopDefault
{
    public override int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
