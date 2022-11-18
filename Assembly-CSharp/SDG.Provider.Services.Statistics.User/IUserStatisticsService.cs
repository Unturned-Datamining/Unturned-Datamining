namespace SDG.Provider.Services.Statistics.User;

public interface IUserStatisticsService : IService
{
    event UserStatisticsRequestReady onUserStatisticsRequestReady;

    bool getStatistic(string name, out int data);

    bool setStatistic(string name, int data);

    bool getStatistic(string name, out float data);

    bool setStatistic(string name, float data);

    bool requestStatistics();
}
