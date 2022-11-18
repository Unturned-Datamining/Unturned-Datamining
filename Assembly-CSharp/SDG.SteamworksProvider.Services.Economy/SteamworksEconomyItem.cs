using SDG.Framework.IO.Streams;
using SDG.Provider.Services.Economy;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Economy;

public class SteamworksEconomyItem : IEconomyItem, INetworkStreamable
{
    public SteamItemDetails_t steamItemDetail { get; protected set; }

    public IEconomyItemDefinition itemDefinitionID { get; protected set; }

    public IEconomyItemInstance itemInstanceID { get; protected set; }

    public void readFromStream(NetworkStream networkStream)
    {
        itemDefinitionID.readFromStream(networkStream);
        itemInstanceID.readFromStream(networkStream);
    }

    public void writeToStream(NetworkStream networkStream)
    {
        itemDefinitionID.writeToStream(networkStream);
        itemInstanceID.writeToStream(networkStream);
    }

    public SteamworksEconomyItem(SteamItemDetails_t newSteamItemDetail)
    {
        steamItemDetail = newSteamItemDetail;
        itemDefinitionID = new SteamworksEconomyItemDefinition(steamItemDetail.m_iDefinition);
        itemInstanceID = new SteamworksEconomyItemInstance(steamItemDetail.m_itemId);
    }
}
