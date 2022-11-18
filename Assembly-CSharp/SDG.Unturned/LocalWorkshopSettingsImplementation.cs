using Steamworks;

namespace SDG.Unturned;

internal class LocalWorkshopSettingsImplementation : ILocalWorkshopSettings
{
    public bool getEnabled(PublishedFileId_t fileId)
    {
        string key = formatEnabledKey(fileId);
        if (ConvenientSavedata.get().read(key, out bool value))
        {
            return value;
        }
        return true;
    }

    public void setEnabled(PublishedFileId_t fileId, bool newEnabled)
    {
        string key = formatEnabledKey(fileId);
        ConvenientSavedata.get().write(key, newEnabled);
    }

    private string formatEnabledKey(PublishedFileId_t fileId)
    {
        PublishedFileId_t publishedFileId_t = fileId;
        return "Enabled_Workshop_Item_" + publishedFileId_t.ToString();
    }
}
