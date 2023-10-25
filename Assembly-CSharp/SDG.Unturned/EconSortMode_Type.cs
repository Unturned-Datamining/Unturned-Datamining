using System.Collections.Generic;
using Steamworks;

namespace SDG.Unturned;

/// <summary>
/// Sorts type alphabetically to the front of the list.
/// </summary>
public class EconSortMode_Type : Comparer<SteamItemDetails_t>
{
    public override int Compare(SteamItemDetails_t x, SteamItemDetails_t y)
    {
        string inventoryType = Provider.provider.economyService.getInventoryType(x.m_iDefinition.m_SteamItemDef);
        string inventoryType2 = Provider.provider.economyService.getInventoryType(y.m_iDefinition.m_SteamItemDef);
        int num = inventoryType.CompareTo(inventoryType2);
        if (num == 0)
        {
            string inventoryName = Provider.provider.economyService.getInventoryName(x.m_iDefinition.m_SteamItemDef);
            string inventoryName2 = Provider.provider.economyService.getInventoryName(y.m_iDefinition.m_SteamItemDef);
            return inventoryName.CompareTo(inventoryName2);
        }
        return num;
    }
}
