using System;

namespace SDG.Unturned;

public static class ItemStoreSavedata
{
    private static DismissableTimeSpan newItemsTimeSpan;

    public static bool WasNewListingsPageSeen()
    {
        return GetNewItemsTimeSpan()?.hasDismissedSpan() ?? true;
    }

    public static void MarkNewListingsPageSeen()
    {
        GetNewItemsTimeSpan()?.dismiss();
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

    public static bool IsNowWithinSpan()
    {
        return GetNewItemsTimeSpan()?.isNowWithinSpan() ?? false;
    }

    private static DismissableTimeSpan GetNewItemsTimeSpan()
    {
        if (newItemsTimeSpan == null)
        {
            if (Provider.statusData == null || Provider.statusData.Stockpile == null)
            {
                return null;
            }
            DateTime new_Items_Start = Provider.statusData.Stockpile.New_Items_Start;
            DateTime new_Items_End = Provider.statusData.Stockpile.New_Items_End;
            string key = "Dismissed_New_On_Stockpile";
            newItemsTimeSpan = new DismissableTimeSpan(new_Items_Start, new_Items_End, key);
        }
        return newItemsTimeSpan;
    }
}
