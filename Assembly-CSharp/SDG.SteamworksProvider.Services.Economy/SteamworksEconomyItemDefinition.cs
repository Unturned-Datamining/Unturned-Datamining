using SDG.Framework.IO.Streams;
using SDG.Provider.Services.Economy;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Economy;

public class SteamworksEconomyItemDefinition : IEconomyItemDefinition, INetworkStreamable
{
    public SteamItemDef_t steamItemDef { get; protected set; }

    public void readFromStream(NetworkStream networkStream)
    {
        steamItemDef = (SteamItemDef_t)networkStream.readInt32();
    }

    public void writeToStream(NetworkStream networkStream)
    {
        networkStream.writeInt32((int)steamItemDef);
    }

    public string getPropertyValue(string key)
    {
        uint punValueBufferSizeOut = 1024u;
        SteamInventory.GetItemDefinitionProperty(steamItemDef, key, out var pchValueBuffer, ref punValueBufferSizeOut);
        return pchValueBuffer;
    }

    public SteamworksEconomyItemDefinition(SteamItemDef_t newSteamItemDef)
    {
        steamItemDef = newSteamItemDef;
    }
}
