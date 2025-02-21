using Steamworks;

namespace SDG.Unturned;

internal class ServerBookmarkDetails
{
    /// <summary>
    /// Persistent identifier for server. Relies on server assigning a Game Server Login Token (GSLT).
    /// i.e., servers without GSLT cannot be bookmarked.
    /// </summary>
    public CSteamID steamId;

    /// <summary>
    /// IP address or DNS name to use as-is, or a web address to perform GET request.
    /// Servers not using Fake IP can specify just a DNS entry and a static query port.
    /// Servers using Fake IP are assigned random ports at startup, but can implement a web API endpoint to return
    /// the IP and port.
    ///
    /// Nelson 2025-01-20: Making this optional now. The downside is we can't perform a Steam A2S query without
    /// IP/port, but the upside is players can more easily join their non-port-forwarded servers.
    /// </summary>
    public string host;

    /// <summary>
    /// Steam query port. Zero for servers using Fake IP.
    /// </summary>
    public ushort queryPort;

    /// <summary>
    /// Name updated from SteamServerAdvertisement.
    /// </summary>
    public string name;

    /// <summary>
    /// Short description updated from SteamServerAdvertisement.
    /// </summary>
    public string description;

    /// <summary>
    /// Small icon updated from SteamServerAdvertisement.
    /// </summary>
    public string thumbnailUrl;

    /// <summary>
    /// Used by UI to track whether it's been added/removed.
    /// </summary>
    public bool isBookmarked = true;

    public void UpdateFromAdvertisement(SteamServerAdvertisement advertisement)
    {
        if (advertisement.IsAddressUsingSteamFakeIP())
        {
            queryPort = 0;
        }
        else
        {
            queryPort = advertisement.queryPort;
        }
        name = advertisement.name;
        description = advertisement.descText;
        thumbnailUrl = advertisement.thumbnailURL;
    }

    public void UpdateFromWorkshopResponse(Provider.CachedWorkshopResponse workshopResponse)
    {
        host = workshopResponse.bookmarkHost;
        name = workshopResponse.serverName;
        description = workshopResponse.serverDescription;
        thumbnailUrl = workshopResponse.thumbnailUrl;
    }

    public override string ToString()
    {
        return $"SteamID: {steamId} Host: \"{host}\" Port: {queryPort} Name: \"{name}\" Description: \"{description}\" Thumbnail URL: \"{thumbnailUrl}\"";
    }
}
