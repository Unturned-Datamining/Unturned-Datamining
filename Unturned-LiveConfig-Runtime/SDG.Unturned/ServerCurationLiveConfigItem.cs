namespace SDG.Unturned;

public struct ServerCurationLiveConfigItem : IDatParseable
{
    public int id;

    public string url;

    public bool TryParse(IDatNode node)
    {
        if (node is DatDictionary datDictionary)
        {
            id = datDictionary.ParseInt32("Id");
            url = datDictionary.GetString("Url");
            return true;
        }
        return false;
    }
}
