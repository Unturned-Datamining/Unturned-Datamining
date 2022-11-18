using System.Collections.Generic;
using Steamworks;

namespace SDG.Unturned;

public class EconItemRarityComparer : Comparer<SteamItemDetails_t>
{
    public override int Compare(SteamItemDetails_t x, SteamItemDetails_t y)
    {
        EItemRarity gameRarity = Provider.provider.economyService.getGameRarity(x.m_iDefinition.m_SteamItemDef);
        EItemRarity gameRarity2 = Provider.provider.economyService.getGameRarity(y.m_iDefinition.m_SteamItemDef);
        int num = gameRarity.CompareTo(gameRarity2);
        if (num == 0)
        {
            string inventoryName = Provider.provider.economyService.getInventoryName(x.m_iDefinition.m_SteamItemDef);
            string inventoryName2 = Provider.provider.economyService.getInventoryName(y.m_iDefinition.m_SteamItemDef);
            return -inventoryName.CompareTo(inventoryName2);
        }
        return -num;
    }
}
