using SDG.Provider.Services;
using SDG.Provider.Services.Workshop;
using SDG.Unturned;
using Steamworks;

namespace SDG.SteamworksProvider.Services.Workshop;

public class SteamworksWorkshopService : Service, IWorkshopService, IService
{
    public bool canOpenWorkshop => true;

    public void open(PublishedFileId_t id)
    {
        SDG.Unturned.Provider.openURL("http://steamcommunity.com/sharedfiles/filedetails/?id=" + id.m_PublishedFileId);
    }
}
