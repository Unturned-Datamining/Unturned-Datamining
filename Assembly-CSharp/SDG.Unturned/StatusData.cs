namespace SDG.Unturned;

public class StatusData
{
    public AchievementStatusData Achievements;

    public GameStatusData Game;

    public MenuStatusData Menu;

    public NewsStatusData News;

    public MapsStatusData Maps;

    public StatusData()
    {
        Achievements = new AchievementStatusData();
        Game = new GameStatusData();
        Menu = new MenuStatusData();
        News = new NewsStatusData();
        Maps = new MapsStatusData();
    }
}
