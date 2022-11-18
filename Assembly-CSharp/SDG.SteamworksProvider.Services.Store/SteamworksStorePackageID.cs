using SDG.Provider.Services.Store;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Store;

public class SteamworksStorePackageID : IStorePackageID
{
    public AppId_t appID { get; protected set; }

    public SteamworksStorePackageID(uint appID)
    {
        this.appID = new AppId_t(appID);
    }
}
