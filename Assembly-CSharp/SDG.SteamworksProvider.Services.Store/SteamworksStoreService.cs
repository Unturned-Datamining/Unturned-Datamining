using SDG.Provider.Services;
using SDG.Provider.Services.Store;
using SDG.Unturned;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Store;

public class SteamworksStoreService : Service, IStoreService, IService
{
    private SteamworksAppInfo appInfo;

    public void open(IStorePackageID packageID)
    {
        AppId_t appID = ((SteamworksStorePackageID)packageID).appID;
        if (SteamUtils.IsOverlayEnabled())
        {
            SteamFriends.ActivateGameOverlayToStore(appID, EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
        }
        else
        {
            SDG.Unturned.Provider.openURL("https://store.steampowered.com/app/" + appID.m_AppId);
        }
    }

    public SteamworksStoreService(SteamworksAppInfo newAppInfo)
    {
        appInfo = newAppInfo;
    }
}
