using Steamworks;

namespace SDG.Provider;

/// <summary>
/// Details of a workshop item that the game may want to refer to later.
/// Cached during client startup after getting installed items, and while
/// downloading UGC for the dedicated server.
/// </summary>
public struct CachedUGCDetails
{
    public PublishedFileId_t fileId;

    public string name;

    public byte compatibilityVersion;

    /// <summary>
    /// Banned workshop files are shown in red.
    /// </summary>
    public bool isBannedOrPrivate;

    /// <summary>
    /// Used on dedicated server to test whether map has been updated, and whether local copy of file is out-of-date.
    /// </summary>
    public uint updateTimestamp;

    /// <summary>
    /// Some workshop thieves use an empty title, in which case we show the file ID as title text.
    /// </summary>
    public string GetTitle()
    {
        if (!string.IsNullOrEmpty(name))
        {
            return name;
        }
        return fileId.ToString();
    }
}
