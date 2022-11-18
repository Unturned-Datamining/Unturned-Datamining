namespace SDG.Provider.Services.Economy;

public interface IEconomyService : IService
{
    bool canOpenInventory { get; }

    IEconomyRequestHandle requestInventory(EconomyRequestReadyCallback economyRequestReadyCallback);

    IEconomyRequestHandle requestPromo(EconomyRequestReadyCallback economyRequestReadyCallback);

    IEconomyRequestHandle exchangeItems(IEconomyItemInstance[] inputItemInstanceIDs, uint[] inputItemQuantities, IEconomyItemDefinition[] outputItemDefinitionIDs, uint[] outputItemQuantities, EconomyRequestReadyCallback economyRequestReadyCallback);

    void open(ulong id);
}
