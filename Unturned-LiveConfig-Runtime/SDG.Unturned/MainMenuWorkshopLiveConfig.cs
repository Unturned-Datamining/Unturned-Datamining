namespace SDG.Unturned;

public class MainMenuWorkshopLiveConfig
{
    public bool allowNews;

    public MainMenuWorkshopFeaturedLiveConfig featured = new MainMenuWorkshopFeaturedLiveConfig();

    public MainMenuWorkshopPopularLiveConfig popular = new MainMenuWorkshopPopularLiveConfig();

    public void Parse(DatDictionary data)
    {
        allowNews = data.ParseBool("AllowNews");
        if (data.TryGetDictionary("Featured", out var node))
        {
            featured.Parse(node);
        }
        if (data.TryGetDictionary("Popular", out var node2))
        {
            popular.Parse(node2);
        }
    }
}
