namespace SDG.Unturned;

public class ServerListComparer_PluginsInverted : ServerListComparer_PluginsDefault
{
    public override int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
