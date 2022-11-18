namespace SDG.Provider.Services.Economy;

public class EconomyRequestResult : IEconomyRequestResult
{
    public EEconomyRequestState economyRequestState { get; protected set; }

    public IEconomyItem[] items { get; protected set; }

    public EconomyRequestResult(EEconomyRequestState newEconomyRequestState, IEconomyItem[] newItems)
    {
        economyRequestState = newEconomyRequestState;
        items = newItems;
    }
}
