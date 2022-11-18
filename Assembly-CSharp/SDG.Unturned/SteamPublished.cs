using Steamworks;

namespace SDG.Unturned;

public class SteamPublished
{
    private string _name;

    private PublishedFileId_t _id;

    public string name => _name;

    public PublishedFileId_t id => _id;

    public SteamPublished(string newName, PublishedFileId_t newID)
    {
        _name = newName;
        _id = newID;
    }
}
