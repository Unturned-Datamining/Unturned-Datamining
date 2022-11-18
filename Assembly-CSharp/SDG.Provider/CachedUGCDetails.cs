using Steamworks;

namespace SDG.Provider;

public struct CachedUGCDetails
{
    public PublishedFileId_t fileId;

    public string name;

    public byte compatibilityVersion;

    public bool isBannedOrPrivate;

    public uint updateTimestamp;

    public string GetTitle()
    {
        if (!string.IsNullOrEmpty(name))
        {
            return name;
        }
        return fileId.ToString();
    }
}
