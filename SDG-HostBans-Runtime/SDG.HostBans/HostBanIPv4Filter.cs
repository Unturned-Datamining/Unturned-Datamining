namespace SDG.HostBans;

public struct HostBanIPv4Filter
{
    public uint ip;

    public ushort minPort;

    public ushort maxPort;

    public EHostBanFlags flags;

    public override string ToString()
    {
        uint num = (ip >> 24) & 0xFFu;
        uint num2 = (ip >> 16) & 0xFFu;
        uint num3 = (ip >> 8) & 0xFFu;
        uint num4 = ip & 0xFFu;
        if (minPort == 0 && maxPort == ushort.MaxValue)
        {
            return $"{num}.{num2}.{num3}.{num4}";
        }
        if (minPort == maxPort)
        {
            return $"{num}.{num2}.{num3}.{num4}:{minPort}";
        }
        return $"{num}.{num2}.{num3}.{num4}:{minPort}-{maxPort}";
    }
}
