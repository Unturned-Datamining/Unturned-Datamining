using SDG.Provider.Services.Statistics.Global;
using SDG.Provider.Services.Statistics.User;

namespace SDG.Provider.Services.Statistics;

public interface IStatisticsService : IService
{
    /// <summary>
    /// Current user statistics implementation.
    /// </summary>
    IUserStatisticsService userStatisticsService { get; }

    /// <summary>
    /// Current global statistics implementation.
    /// </summary>
    IGlobalStatisticsService globalStatisticsService { get; }
}
