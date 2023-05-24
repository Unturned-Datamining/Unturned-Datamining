namespace SDG.Unturned;

public static class ItemStoreSavedata
{
    public static bool WasNewListingsPageSeen()
    {
        ItemStoreLiveConfig itemStore = LiveConfig.Get().itemStore;
        if (ConvenientSavedata.get().read("ItemStoreSeenPromotionId", out long value) && value >= itemStore.promotionId)
        {
            return true;
        }
        return false;
    }

    public static void MarkNewListingsPageSeen()
    {
        ItemStoreLiveConfig itemStore = LiveConfig.Get().itemStore;
        ConvenientSavedata.get().write("ItemStoreSeenPromotionId", itemStore.promotionId);
    }

    public static bool WasNewListingSeen(int itemdefid)
    {
        string flag = FormatNewListingSeenFlag(itemdefid);
        return ConvenientSavedata.get().hasFlag(flag);
    }

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
