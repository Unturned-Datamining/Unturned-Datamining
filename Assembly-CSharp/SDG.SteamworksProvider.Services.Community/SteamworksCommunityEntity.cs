using SDG.Framework.IO.Streams;
using SDG.Provider.Services.Community;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Community;

public struct SteamworksCommunityEntity : ICommunityEntity, INetworkStreamable
{
    public static readonly SteamworksCommunityEntity INVALID = new SteamworksCommunityEntity(CSteamID.Nil);

    public CSteamID steamID;

    public bool isValid => steamID.IsValid();

    public void readFromStream(NetworkStream networkStream)
    {
        steamID = (CSteamID)networkStream.readUInt64();
    }

    public void writeToStream(NetworkStream networkStream)
    {
        networkStream.writeUInt64((ulong)steamID);
    }

    public SteamworksCommunityEntity(CSteamID newSteamID)
    {
        steamID = newSteamID;
    }
}
