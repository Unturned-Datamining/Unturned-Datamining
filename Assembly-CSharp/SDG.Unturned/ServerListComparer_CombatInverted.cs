namespace SDG.Unturned;

public class ServerListComparer_CombatInverted : ServerListComparer_CombatDefault
{
    public override int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        return -base.Compare(lhs, rhs);
    }
}
