namespace SDG.Provider.Services.Economy;

public interface IEconomyRequestResult
{
    EEconomyRequestState economyRequestState { get; }

    IEconomyItem[] items { get; }
}
