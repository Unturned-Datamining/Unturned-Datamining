namespace SDG.Unturned;

public class ServerListComparer_AnticheatInverted : ServerListComparer_AnticheatDefault
{
    public override int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
