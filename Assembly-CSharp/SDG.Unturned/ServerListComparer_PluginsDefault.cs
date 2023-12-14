using System.Collections.Generic;

namespace SDG.Unturned;

public class ServerListComparer_PluginsDefault : IComparer<SteamServerAdvertisement>
{
    private int[] orderMap;

    public virtual int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        if (lhs.pluginFramework == rhs.pluginFramework)
        {
            return lhs.name.CompareTo(rhs.name);
        }
        int num = orderMap[(int)lhs.pluginFramework];
        int num2 = orderMap[(int)rhs.pluginFramework];
        return num - num2;
    }

    public ServerListComparer_PluginsDefault()
    {
        orderMap = new int[4];
        orderMap[0] = 0;
        orderMap[1] = 1;
        orderMap[2] = 1;
        orderMap[3] = 1;
    }
}
