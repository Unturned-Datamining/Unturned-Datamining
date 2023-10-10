namespace SDG.HostBans;

public struct HostBanSteamIdFilter
{
    public ulong steamId;

    public EHostBanFlags flags;

    public override string ToString()
    {
        return steamId.ToString();
    }
}
