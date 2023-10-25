using Steamworks;

namespace SDG.Provider.Services.Workshop;

public interface IWorkshopService : IService
{
    /// <summary>
    /// Whether the user has their overlay enabled.
    /// </summary>
    bool canOpenWorkshop { get; }

    void open(PublishedFileId_t id);
}
