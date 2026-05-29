namespace SDG.Unturned;

public class ServerListComparer_AnticheatDefault : ServerListComparer_Base
{
    protected override int CompareDetails(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        if (lhs.IsThirdpartyAntiCheatEnabled != rhs.IsThirdpartyAntiCheatEnabled)
        {
            if (!lhs.IsThirdpartyAntiCheatEnabled)
            {
                return 1;
            }
            return -1;
        }
        if (lhs.IsVACSecure == rhs.IsVACSecure)
        {
            return lhs.name.CompareTo(rhs.name);
        }
        if (!lhs.IsVACSecure)
        {
            return 1;
        }
        return -1;
    }
}
