using System.Collections.Generic;

namespace SDG.Unturned;

public class ServerListComparer_MonetizationDefault : IComparer<SteamServerAdvertisement>
{
    private int[] orderMap;

    public virtual int Compare(SteamServerAdvertisement lhs, SteamServerAdvertisement rhs)
    {
        if (lhs.monetization == rhs.monetization)
        {
            return lhs.name.CompareTo(rhs.name);
        }
        int num = orderMap[(int)lhs.monetization];
        int num2 = orderMap[(int)rhs.monetization];
        return num - num2;
    }

    public ServerListComparer_MonetizationDefault()
    {
        orderMap = new int[5];
        orderMap[2] = 0;
        orderMap[3] = 1;
        orderMap[0] = 2;
        orderMap[4] = 3;
        orderMap[1] = 4;
    }
}
