using SDG.Provider.Services.Economy;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Economy;

public class SteamworksEconomyRequestHandle : IEconomyRequestHandle
{
    public SteamInventoryResult_t steamInventoryResult { get; protected set; }

    private EconomyRequestReadyCallback economyRequestReadyCallback { get; set; }

    public void triggerInventoryRequestReadyCallback(IEconomyRequestResult inventoryRequestResult)
    {
        if (economyRequestReadyCallback != null)
        {
            economyRequestReadyCallback(this, inventoryRequestResult);
        }
    }

    public SteamworksEconomyRequestHandle(SteamInventoryResult_t newSteamInventoryResult, EconomyRequestReadyCallback newEconomyRequestReadyCallback)
    {
        steamInventoryResult = newSteamInventoryResult;
        economyRequestReadyCallback = newEconomyRequestReadyCallback;
    }
}
