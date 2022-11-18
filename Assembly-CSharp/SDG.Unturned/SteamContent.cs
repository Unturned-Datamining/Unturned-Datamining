using Steamworks;

namespace SDG.Unturned;

public class SteamContent
{
    private PublishedFileId_t _publishedFileID;

    private string _path;

    private ESteamUGCType _type;

    public PublishedFileId_t publishedFileID => _publishedFileID;

    public string path => _path;

    public ESteamUGCType type => _type;

    public SteamContent(PublishedFileId_t newPublishedFileID, string newPath, ESteamUGCType newType)
    {
        _publishedFileID = newPublishedFileID;
        _path = newPath;
        _type = newType;
    }
}
