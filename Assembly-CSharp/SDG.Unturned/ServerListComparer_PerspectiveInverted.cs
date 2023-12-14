namespace SDG.Unturned;

public class ServerListComparer_PerspectiveInverted : ServerListComparer_PerspectiveDefault
{
    public override int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
