namespace SDG.Unturned;

public static class LocalNews
{
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
