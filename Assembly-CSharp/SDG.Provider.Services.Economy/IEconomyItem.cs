using SDG.Framework.IO.Streams;

namespace SDG.Provider.Services.Economy;

public interface IEconomyItem : INetworkStreamable
{
    IEconomyItemDefinition itemDefinitionID { get; }

    IEconomyItemInstance itemInstanceID { get; }
}
