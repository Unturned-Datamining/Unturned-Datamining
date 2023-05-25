namespace SDG.Unturned;

public class MainMenuWorkshopPopularLiveConfig
{
    public uint trendDays;

    public int carouselItems;

    public void Parse(DatDictionary data)
    {
        trendDays = data.ParseUInt32("TrendDays");
        carouselItems = data.ParseInt32("CarouselItems");
    }
}
