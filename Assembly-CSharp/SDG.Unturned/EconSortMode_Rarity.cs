using System.Collections.Generic;
using SDG.Provider;
using Steamworks;

namespace SDG.Unturned;

public class EconSortMode_Rarity : Comparer<SteamItemDetails_t>
{
    public override int Compare(SteamItemDetails_t x, SteamItemDetails_t y)
    {
        UnturnedEconInfo.ERarity inventoryRarity = Provider.provider.economyService.getInventoryRarity(x.m_iDefinition.m_SteamItemDef);
        UnturnedEconInfo.ERarity inventoryRarity2 = Provider.provider.economyService.getInventoryRarity(y.m_iDefinition.m_SteamItemDef);
        int num = inventoryRarity.CompareTo(inventoryRarity2);
        if (num == 0)
        {
            string inventoryName = Provider.provider.economyService.getInventoryName(x.m_iDefinition.m_SteamItemDef);
            string inventoryName2 = Provider.provider.economyService.getInventoryName(y.m_iDefinition.m_SteamItemDef);
            return inventoryName.CompareTo(inventoryName2);
        }
        return -num;
    }
}
