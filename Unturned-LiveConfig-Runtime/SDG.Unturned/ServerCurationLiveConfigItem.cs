namespace SDG.Unturned;

public struct ServerCurationLiveConfigItem : IDatParseable
{
    public int id;

    public string url;

    public bool TryParse(IDatNode node)
    {
        if (node is IDatDictionary dictionary)
        {
            id = dictionary.ParseInt32("Id");
            url = dictionary.GetString("Url");
            return true;
        }
        return false;
    }
}
