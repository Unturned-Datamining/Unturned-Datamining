namespace SDG.Unturned;

/// <summary>
/// Sort servers by normalized player count low to high.
/// </summary>
public class ServerListComparer_FullnessInverted : ServerListComparer_FullnessDefault
{
    public override int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
