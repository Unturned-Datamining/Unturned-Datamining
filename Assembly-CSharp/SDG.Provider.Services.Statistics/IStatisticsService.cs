using SDG.Provider.Services.Statistics.Global;
using SDG.Provider.Services.Statistics.User;

namespace SDG.Provider.Services.Statistics;

public interface IStatisticsService : IService
{
    IUserStatisticsService userStatisticsService { get; }

    IGlobalStatisticsService globalStatisticsService { get; }
}
