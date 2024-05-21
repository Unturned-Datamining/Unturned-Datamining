namespace SDG.Unturned;

public class LiveConfigData
{
    public MainMenuAlert mainMenuAlert = new MainMenuAlert();

    public MainMenuWorkshopLiveConfig mainMenuWorkshop = new MainMenuWorkshopLiveConfig();

    public ItemStoreLiveConfig itemStore = new ItemStoreLiveConfig();

    public bool shouldAllowJoiningInternetServersWithoutGslt;

    public bool shouldServersWithoutMonetizationTagBeVisibleInInternetServerList;

    public bool arePbsCraftableItemsAvailable;

    public int playtimeGeneratorItemDefId;

    public int queryPingWarningOffsetMs = 200;

    public long craftingPromotionId = -1L;

    public void Parse(DatDictionary data)
    {
        if (data.TryGetDictionary("MainMenuAlert", out var node))
        {
            mainMenuAlert.Parse(node);
        }
        if (data.TryGetDictionary("MainMenuWorkshop", out var node2))
        {
            mainMenuWorkshop.Parse(node2);
        }
        if (data.TryGetDictionary("ItemStore", out var node3))
        {
            itemStore.Parse(node3);
        }
        shouldAllowJoiningInternetServersWithoutGslt = data.ParseBool("ShouldAllowJoiningInternetServersWithoutGslt");
        shouldServersWithoutMonetizationTagBeVisibleInInternetServerList = data.ParseBool("ShouldServersWithoutMonetizationTagBeVisibleInInternetServerList");
        arePbsCraftableItemsAvailable = data.ParseBool("ArePbsCraftableItemsAvailable");
        playtimeGeneratorItemDefId = data.ParseInt32("PlaytimeGeneratorItemDefId");
        queryPingWarningOffsetMs = data.ParseInt32("QueryPingWarningOffsetMs", 200);
        craftingPromotionId = data.ParseInt64("CraftingPromotionId", -1L);
    }
}
