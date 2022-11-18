using SDG.Provider.Services;
using SDG.Provider.Services.Browser;
using SDG.Unturned;

namespace SDG.SteamworksProvider.Services.Browser;

public class SteamworksBrowserService : Service, IBrowserService, IService
{
    public bool canOpenBrowser => true;

    public void open(string url)
    {
        SDG.Unturned.Provider.openURL(url);
    }
}
