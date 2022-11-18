namespace SDG.Provider.Services.Achievements;

public interface IAchievementsService : IService
{
    bool getAchievement(string name, out bool has);

    bool setAchievement(string name);
}
