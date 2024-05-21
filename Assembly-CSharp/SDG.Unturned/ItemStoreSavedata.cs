namespace SDG.Unturned;

/// <summary>
/// Tracks whether we should show the "NEW" label on listings and item store button.
/// </summary>
public static class ItemStoreSavedata
{
    public static bool WasNewCraftingPageSeen()
    {
        LiveConfigData liveConfigData = LiveConfig.Get();
        if (liveConfigData.craftingPromotionId <= 0)
        {
            return true;
        }
        if (ConvenientSavedata.get().read("CraftingSeenPromotionId", out long value) && value >= liveConfigData.craftingPromotionId)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Track that player has seen the new crafting blueprints.
    /// </summary>
    public static void MarkNewCraftingPageSeen()
    {
        LiveConfigData liveConfigData = LiveConfig.Get();
        ConvenientSavedata.get().write("CraftingSeenPromotionId", liveConfigData.craftingPromotionId);
    }

    public static bool WasNewListingsPageSeen()
    {
        ItemStoreLiveConfig itemStore = LiveConfig.Get().itemStore;
        if (ConvenientSavedata.get().read("ItemStoreSeenPromotionId", out long value) && value >= itemStore.promotionId)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Track that player has seen the page with all new listings.
    /// </summary>
    public static void MarkNewListingsPageSeen()
    {
        ItemStoreLiveConfig itemStore = LiveConfig.Get().itemStore;
        ConvenientSavedata.get().write("ItemStoreSeenPromotionId", itemStore.promotionId);
    }

    /// <summary>
    /// Has player seen the given listing?
    /// </summary>
    public static bool WasNewListingSeen(int itemdefid)
    {
        string flag = FormatNewListingSeenFlag(itemdefid);
        return ConvenientSavedata.get().hasFlag(flag);
    }

    /// <summary>
    /// Track that the player has seen the given listing.
    /// </summary>
    public static void MarkNewListingSeen(int itemdefid)
    {
        string flag = FormatNewListingSeenFlag(itemdefid);
        ConvenientSavedata.get().setFlag(flag);
    }

    private static string FormatNewListingSeenFlag(int itemdefid)
    {
        return "New_Listing_Seen_" + itemdefid;
    }
}
