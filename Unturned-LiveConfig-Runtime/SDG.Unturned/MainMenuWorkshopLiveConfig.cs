namespace SDG.Unturned;

public struct MainMenuWorkshopLiveConfig
{
    public bool allowNews;

    public MainMenuWorkshopFeaturedLiveConfig featured;

    public MainMenuWorkshopPopularLiveConfig popular;

    public void Parse(DatDictionary data)
    {
        allowNews = data.ParseBool("AllowNews");
        featured = default(MainMenuWorkshopFeaturedLiveConfig);
        if (data.TryGetDictionary("Featured", out var node))
        {
            featured.Parse(node);
        }
        popular = default(MainMenuWorkshopPopularLiveConfig);
        if (data.TryGetDictionary("Popular", out var node2))
        {
            popular.Parse(node2);
        }
    }
}
