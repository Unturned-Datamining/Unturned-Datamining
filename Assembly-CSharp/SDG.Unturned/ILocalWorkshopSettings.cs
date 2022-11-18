using Steamworks;

namespace SDG.Unturned;

public interface ILocalWorkshopSettings
{
    bool getEnabled(PublishedFileId_t fileId);

    void setEnabled(PublishedFileId_t fileId, bool newEnabled);
}
