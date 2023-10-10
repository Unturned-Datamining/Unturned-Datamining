using Unturned.SystemEx;

namespace SDG.HostBans;

public struct HostBanIPv4Filter
{
    public IPv4Filter filter;

    public EHostBanFlags flags;

    public override string ToString()
    {
        return filter.ToString();
    }
}
