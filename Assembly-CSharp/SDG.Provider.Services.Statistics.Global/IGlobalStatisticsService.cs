namespace SDG.Provider.Services.Statistics.Global;

public interface IGlobalStatisticsService : IService
{
    event GlobalStatisticsRequestReady onGlobalStatisticsRequestReady;

    bool getStatistic(string name, out long data);

    bool getStatistic(string name, out double data);

    bool requestStatistics();
}
