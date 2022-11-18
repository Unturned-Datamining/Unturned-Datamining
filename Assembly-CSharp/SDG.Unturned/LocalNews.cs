using System;

namespace SDG.Unturned;

public static class LocalNews
{
    private static DismissableTimeSpan featuredWorkshopTimeSpan;

    public static bool isNowWithinFeaturedWorkshopWindow()
    {
        if (Provider.statusData == null || Provider.statusData.News == null)
        {
            return false;
        }
        DateTime featured_Workshop_Start = Provider.statusData.News.Featured_Workshop_Start;
        DateTime featured_Workshop_End = Provider.statusData.News.Featured_Workshop_End;
        if (featured_Workshop_Start.Kind != DateTimeKind.Utc || featured_Workshop_End.Kind != DateTimeKind.Utc)
        {
            UnturnedLog.error("Featured workshop start/end timestamps should be in UTC!");
            return false;
        }
        if (featured_Workshop_Start > featured_Workshop_End)
        {
            UnturnedLog.error("Featured workshop start/end timestamps are backwards!");
            return false;
        }
        DateTime utcNow = DateTime.UtcNow;
        if (utcNow > featured_Workshop_Start)
        {
            return utcNow < featured_Workshop_End;
        }
        return false;
    }

    public static bool wasWorkshopItemDismissed(ulong id)
    {
        string flag = formatDismissedWorkshopItemFlag(id);
        return ConvenientSavedata.get().hasFlag(flag);
    }

    public static void dismissWorkshopItem(ulong id)
    {
        string flag = formatDismissedWorkshopItemFlag(id);
        ConvenientSavedata.get().setFlag(flag);
    }

    private static string formatDismissedWorkshopItemFlag(ulong id)
    {
        return "Dismissed_Workshop_Item_" + id;
    }

    public static bool isFeaturedWorkshopStatusRelevant()
    {
        return getFeaturedWorkshopTimeSpan()?.isRelevant() ?? false;
    }

    public static void dismissFeaturedWorkshopStatus()
    {
        getFeaturedWorkshopTimeSpan()?.dismiss();
    }

    private static DismissableTimeSpan getFeaturedWorkshopTimeSpan()
    {
        if (featuredWorkshopTimeSpan == null)
        {
            if (Provider.statusData == null || Provider.statusData.News == null)
            {
                return null;
            }
            DateTime featured_Workshop_Start = Provider.statusData.News.Featured_Workshop_Start;
            DateTime featured_Workshop_End = Provider.statusData.News.Featured_Workshop_End;
            string key = "Dismissed_Featured_Workshop_Status";
            featuredWorkshopTimeSpan = new DismissableTimeSpan(featured_Workshop_Start, featured_Workshop_End, key);
        }
        return featuredWorkshopTimeSpan;
    }

    public static bool hasAutoSubscribedToWorkshopItem(ulong id)
    {
        string flag = formatAutoSubscribedWorkshopItemFlag(id);
        return ConvenientSavedata.get().hasFlag(flag);
    }

    public static void markAutoSubscribedToWorkshopItem(ulong id)
    {
        string flag = formatAutoSubscribedWorkshopItemFlag(id);
        ConvenientSavedata.get().setFlag(flag);
    }

    private static string formatAutoSubscribedWorkshopItemFlag(ulong id)
    {
        return "Auto_Subscribed_Workshop_Item_" + id;
    }
}
