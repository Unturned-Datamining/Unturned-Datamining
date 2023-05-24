namespace SDG.Unturned;

public class LiveConfigData
{
    public MainMenuAlert mainMenuAlert;

    public MainMenuWorkshopLiveConfig mainMenuWorkshop;

    public ItemStoreLiveConfig itemStore;

    public bool shouldAllowJoiningInternetServersWithoutGslt;

    public bool shouldServersWithoutMonetizationTagBeVisibleInInternetServerList;

    public int playtimeGeneratorItemDefId;

    public int queryPingWarningOffsetMs = 200;

    public void Parse(DatDictionary data)
    {
        mainMenuAlert = default(MainMenuAlert);
        if (data.TryGetDictionary("MainMenuAlert", out var node))
        {
            mainMenuAlert.Parse(node);
        }
        mainMenuWorkshop = default(MainMenuWorkshopLiveConfig);
        if (data.TryGetDictionary("MainMenuWorkshop", out var node2))
        {
            mainMenuWorkshop.Parse(node2);
        }
        itemStore = default(ItemStoreLiveConfig);
        if (data.TryGetDictionary("ItemStore", out var node3))
        {
            itemStore.Parse(node3);
        }
        shouldAllowJoiningInternetServersWithoutGslt = data.ParseBool("ShouldAllowJoiningInternetServersWithoutGslt");
        shouldServersWithoutMonetizationTagBeVisibleInInternetServerList = data.ParseBool("ShouldServersWithoutMonetizationTagBeVisibleInInternetServerList");
        playtimeGeneratorItemDefId = data.ParseInt32("PlaytimeGeneratorItemDefId");
        queryPingWarningOffsetMs = data.ParseInt32("QueryPingWarningOffsetMs", 200);
    }
}
