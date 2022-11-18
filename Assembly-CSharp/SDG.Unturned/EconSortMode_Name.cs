using System.Collections.Generic;
using Steamworks;

namespace SDG.Unturned;

public class EconSortMode_Name : Comparer<SteamItemDetails_t>
{
    public override int Compare(SteamItemDetails_t x, SteamItemDetails_t y)
    {
        string inventoryName = Provider.provider.economyService.getInventoryName(x.m_iDefinition.m_SteamItemDef);
        string inventoryName2 = Provider.provider.economyService.getInventoryName(y.m_iDefinition.m_SteamItemDef);
        return inventoryName.CompareTo(inventoryName2);
    }
}
