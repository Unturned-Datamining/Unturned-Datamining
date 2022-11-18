namespace SDG.Unturned;

public class StatusData
{
    public AchievementStatusData Achievements;

    public GameStatusData Game;

    public HolidayStatusData Holidays;

    public MenuStatusData Menu;

    public NewsStatusData News;

    public MapsStatusData Maps;

    public StockpileStatusData Stockpile;

    public StatusData()
    {
        Achievements = new AchievementStatusData();
        Game = new GameStatusData();
        Holidays = new HolidayStatusData();
        Menu = new MenuStatusData();
        News = new NewsStatusData();
        Maps = new MapsStatusData();
        Stockpile = new StockpileStatusData();
    }
}
