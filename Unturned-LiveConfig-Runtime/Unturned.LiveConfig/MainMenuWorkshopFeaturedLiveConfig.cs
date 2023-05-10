namespace Unturned.LiveConfig;

public struct MainMenuWorkshopFeaturedLiveConfig
{
    public long Id;

    public ulong[] FileIds;

    public EMapStatus Status;

    public EFeaturedWorkshopType Type;

    public bool AutoExpandDescription;

    public string OverrideDescription;

    public string LinkText;

    public string LinkURL;

    public int[] AssociatedStockpileItems;

    public bool IsFeatured(ulong fileId)
    {
        if (fileId == 0L || FileIds == null)
        {
            return false;
        }
        ulong[] fileIds = FileIds;
        for (int i = 0; i < fileIds.Length; i++)
        {
            if (fileIds[i] == fileId)
            {
                return true;
            }
        }
        return false;
    }
}
